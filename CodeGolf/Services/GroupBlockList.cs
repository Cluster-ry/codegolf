using System.Text.Json;
using CodeGolf.Tools;

namespace CodeGolf.Services;

/// <summary>
/// Names an admin has removed. Stored on disk so a restart of the in-memory database does not lift the block.
/// </summary>
public sealed class GroupBlockList
{
    public const string FileName = "banned-groups.json";

    private readonly object _gate = new();
    private readonly string _path;
    private readonly ILogger _logger;
    private readonly HashSet<string> _names;

    public GroupBlockList(IHostEnvironment environment, ILogger<GroupBlockList> logger)
        : this(BlockFilePath(environment), logger)
    {
    }

    private static string BlockFilePath(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        return Path.Combine(environment.ContentRootPath, FileName);
    }

    public GroupBlockList(string filePath, ILogger logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(logger);
        _path = filePath;
        _logger = logger;
        _names = Load();
    }

    public bool IsBlocked(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        lock (_gate)
        {
            return _names.Contains(name.Trim());
        }
    }

    public IReadOnlyList<string> Names()
    {
        lock (_gate)
        {
            return _names.OrderBy(blockedName => blockedName, StringComparer.OrdinalIgnoreCase).ToArray();
        }
    }

    public void Block(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        lock (_gate)
        {
            if (!_names.Add(trimmed))
            {
                return;
            }

            try
            {
                Save();
            }
            catch (Exception ex)
            {
                _names.Remove(trimmed);
                _logger.BlockListSaveFailed(ex, trimmed);
                throw;
            }
        }
    }

    public bool Unblock(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var trimmed = name.Trim();
        lock (_gate)
        {
            if (!_names.Remove(trimmed))
            {
                return false;
            }

            try
            {
                Save();
            }
            catch (Exception ex)
            {
                _names.Add(trimmed);
                _logger.BlockListUnblockSaveFailed(ex, trimmed);
                throw;
            }

            return true;
        }
    }

    private HashSet<string> Load()
    {
        if (!File.Exists(_path))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var json = File.ReadAllText(_path);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var names = JsonSerializer.Deserialize<string[]>(json)
                ?? throw new InvalidDataException($"Blocked group list {_path} is empty.");
            return new HashSet<string>(
                names.Where(name => !string.IsNullOrWhiteSpace(name)).Select(name => name.Trim()),
                StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Blocked group list {_path} could not be read.", ex);
        }
    }

    private void Save()
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(_names.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray());
        var temporary = _path + ".tmp";
        File.WriteAllText(temporary, json);
        File.Move(temporary, _path, overwrite: true);
    }
}

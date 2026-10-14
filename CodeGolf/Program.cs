using CodeGolf.Python;
using CodeGolf.Services;
using CodeGolf.DataBase;
using CodeGolf.Pages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
builder.Services.AddPooledDbContextFactory<DataBaseContext>(options => options.UseInMemoryDatabase("Inmemory"));
builder.Services.AddSingleton<DataBaseContext>();
builder.Services.AddSingleton<PythonRunner>();
builder.Services.AddTransient<GroupService>();
builder.Services.AddTransient<ProblemService>();
builder.Services.AddTransient<EvaluateService>();

builder.Services.AddScoped<GroupHandler>();
builder.Services.AddScoped<ProblemHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Do initial loading
_ = app.Services.GetRequiredService<PythonRunner>();
var problemService = app.Services.GetRequiredService<ProblemService>();
problemService.InitializeProblems();
var evaluateService = app.Services.GetRequiredService<EvaluateService>();
evaluateService.LoadExampleResults().Wait();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

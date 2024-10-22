# Codegolf
Codegolf for cluster ry

Problems are defined in the problems.json
There are 4 required fields for each problem
"Name": "Name of the problem. Visible in the navbar."
"Explanation": "Explanation of the problem for the users."
"ExampleSolution": "Example solution which the user solutions are tried against"
"Input":"stdin for the solution. Leave empty if not needed."


## Dependencies
- Podman / Docker

## Using

### Running

```bash
dotnet publish -c Release
cd ./CodeGolf/bin/Release/net8.0/publish/
CodeGolf
```

### Developing 
```bash
cd ./CodeGolf
dotnet run 
```

### Deploying

Use a server or rent a vps from azure. Install rootless podman and ef core supported database. Don't forget https and others. 

Your computer
```bash
dotnet publish -c Release
scp -r ./CodeGolf/bin/Release/net8.0/publish/ user@server.com:/var/www/
```

Server
```bash
podman info
cd /var/www
CodeGolf

```
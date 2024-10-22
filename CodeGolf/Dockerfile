# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0@sha256:35792ea4ad1db051981f62b313f1be3b46b1f45cadbaa3c288cd0d3056eefb83 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0@sha256:6c4df091e4e531bb93bdbfe7e7f0998e7ced344f54426b7e874116a3dc3233ff
WORKDIR /App
COPY --from=build-env /App/out .

#Update sources as we need modern podman
RUN sed -i 's|bookworm|trixie|g' /etc/apt/sources.list.d/debian.sources \
    && apt-get update \
    && apt-get upgrade -y

# Install Podman
RUN apt-get install -y podman && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

COPY python-alpine.tar /tmp/python-alpine.tar
RUN podman load --input /tmp/python-alpine.tar && rm /tmp/python-alpine.tar
RUN podman info

ENTRYPOINT ["dotnet", "CodeGolf.dll"]
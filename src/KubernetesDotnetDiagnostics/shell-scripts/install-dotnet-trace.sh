#!/bin/bash

command -v unzip >/dev/null 2>&1 || (apt-get update && apt-get install unzip)
# cd /diagnostics || (mkdir /diagnostics && cd /diagnostics)

if [ ! -f "/diagnostics/dotnet-trace.nupkg" ]; then
    curl -L --output /diagnostics/dotnet-trace.nupkg https://www.nuget.org/api/v2/package/dotnet-trace/3.1.120604
fi

if [ ! -d "/diagnostics/dotnet-trace" ]; then
    unzip -o /diagnostics/dotnet-trace.nupkg -d /diagnostics/dotnet-trace
fi

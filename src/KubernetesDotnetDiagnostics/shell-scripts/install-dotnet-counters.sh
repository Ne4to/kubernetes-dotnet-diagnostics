#!/bin/bash

if [ ! -d '/diagnostics' ]; then
    mkdir /diagnostics;
fi

command -v unzip >/dev/null 2>&1 || (apt-get update && apt-get install unzip)

if [ ! -f "/diagnostics/dotnet-counters.nupkg" ]; then
    curl -L --output /diagnostics/dotnet-counters.nupkg https://www.nuget.org/api/v2/package/dotnet-counters/3.1.120604
fi

if [ ! -d "/diagnostics/dotnet-counters" ]; then
    unzip -o /diagnostics/dotnet-counters.nupkg -d /diagnostics/dotnet-counters
fi
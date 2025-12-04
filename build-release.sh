#/bin/bash
dotnet clean
dotnet clean -c Release
dotnet build -c Release
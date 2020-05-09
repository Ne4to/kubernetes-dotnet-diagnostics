dotnet tool uninstall -g KubernetesDotnetDiagnostics
dotnet pack
dotnet tool install --global --add-source ./nupkg KubernetesDotnetDiagnostics
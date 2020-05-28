# kubernetes-dotnet-diagnostics
kubectl plugin to run https://github.com/dotnet/diagnostics tools

##Installation
```
dotnet tool install --global KubernetesDotnetDiagnostics --version <version>
```

## Using

Run `dotnet-trace collect` [dotnet-trace instructions](https://github.com/dotnet/diagnostics/blob/master/documentation/dotnet-trace-instructions.md)
```shell
kubectl dotnetdiag --trace demo-global-admin-processor-58dd4cbcff-fmfgw
```

Run `dotnet-counters monitor` [dotnet-counters instructions](https://github.com/dotnet/diagnostics/blob/master/documentation/dotnet-counters-instructions.md)
```shell
kubectl dotnetdiag --counters demo-global-admin-processor-58dd4cbcff-fmfgw
```
# kubernetes-dotnet-diagnostics
kubectl plugin to run https://github.com/dotnet/diagnostics tools

## Installation
[![NuGet Badge](https://buildstats.info/nuget/kubernetesdotnetdiagnostics?includePreReleases=true&dWidth=0)](https://www.nuget.org/packages/KubernetesDotnetDiagnostics/)
```
dotnet tool install --global KubernetesDotnetDiagnostics --version <version>
```

## Usage
<!--USAGE_BEGIN-->
Run `dotnet-trace collect` [dotnet-trace instructions](https://github.com/dotnet/diagnostics/blob/master/documentation/dotnet-trace-instructions.md)
```shell
Usage:
  kubectl dotnetdiag trace [options] <pod>

Arguments:
  <pod>    Pod name

Options:
  -n, --namespace <n>    The namespace scope
  --args <args>          dotnet-counters arguments
  -?, -h, --help         Show help and usage information
```
Run `dotnet-counters monitor` [dotnet-counters instructions](https://github.com/dotnet/diagnostics/blob/master/documentation/dotnet-counters-instructions.md)
```shell
Usage:
  kubectl dotnetdiag counters [options] <pod> [command]

Arguments:
  <pod>    Pod name

Options:
  -n, --namespace <n>    The namespace scope
  --args <args>          dotnet-counters arguments
  -?, -h, --help         Show help and usage information

Commands:
  --monitor
  --collect
```
<!--USAGE_END-->

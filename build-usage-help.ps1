$ReadmeContent = Get-Content 'README.md' -Raw
$StartIndex = $ReadmeContent.IndexOf('<!--USAGE_BEGIN-->')
$EndIndex = $ReadmeContent.IndexOf('<!--USAGE_END-->')


$NewReadme = $ReadmeContent.Substring(0, $StartIndex)
$NewReadme += "<!--USAGE_BEGIN-->`r`n"

$NewReadme += 'Run `dotnet-trace collect` [dotnet-trace instructions](https://github.com/dotnet/diagnostics/blob/master/documentation/dotnet-trace-instructions.md)' + "`r`n"
$NewReadme += '```shell' + "`r`n"
$NewReadme += $($(dotnet run --project .\src\KubernetesDotnetDiagnostics\KubernetesDotnetDiagnostics.csproj -- --trace --help) -join "`r`n") -replace 'KubernetesDotnetDiagnostics', 'kubectl dotnetdiag'
$NewReadme += '```' + "`r`n"

$NewReadme += 'Run `dotnet-counters monitor` [dotnet-counters instructions](https://github.com/dotnet/diagnostics/blob/master/documentation/dotnet-counters-instructions.md)' + "`r`n"
$NewReadme += '```shell' + "`r`n"
$NewReadme += $($(dotnet run --project .\src\KubernetesDotnetDiagnostics\KubernetesDotnetDiagnostics.csproj -- --counters --help) -join "`r`n") -replace 'KubernetesDotnetDiagnostics', 'kubectl dotnetdiag'
$NewReadme += '```' + "`r`n"

$NewReadme += $ReadmeContent.Substring($EndIndex)
$NewReadme = $NewReadme.TrimEnd()

Set-Content -Path 'README.md' -Value $NewReadme
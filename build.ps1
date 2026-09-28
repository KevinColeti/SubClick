param(
    [string]$DotNet = 'dotnet',
    [string]$Iscc,
    [string]$PackageSource,
    [switch]$SkipInstaller
)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    $restore = @('restore', 'SubClick.slnx', '--configfile', 'NuGet.Config')
    if ($PackageSource) { $restore += @('--source', $PackageSource) }
    & $DotNet @restore
    if ($LASTEXITCODE -ne 0) { throw 'Restore failed' }
    & $DotNet build SubClick.slnx -c Release --no-restore
    if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
    & $DotNet run --project tests/SubClick.Tests -c Release --no-build
    if ($LASTEXITCODE -ne 0) { throw 'Tests failed' }
    & $DotNet run --project tests/SubClick.UiTests -c Release --no-build -- artifacts/ui
    if ($LASTEXITCODE -ne 0) { throw 'UI tests failed' }
    $publish = @('publish', 'src/SubClick.App', '-c', 'Release', '-r', 'win-x64', '--self-contained', 'true', '-o', 'artifacts/publish', '-p:RestoreConfigFile=NuGet.Config')
    if ($PackageSource) { $publish += @('--source', $PackageSource) }
    & $DotNet @publish
    if ($LASTEXITCODE -ne 0) { throw 'Publish failed' }
    if (-not $SkipInstaller) {
        if (-not $Iscc) {
            $found = Get-Command ISCC.exe -ErrorAction SilentlyContinue
            if ($found) { $Iscc = $found.Source }
            else {
                foreach ($candidate in @("${env:ProgramFiles(x86)}\Inno Setup 7\ISCC.exe", "$env:ProgramFiles\Inno Setup 7\ISCC.exe", "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe")) {
                    if (Test-Path -LiteralPath $candidate) { $Iscc = $candidate; break }
                }
            }
        }
        if (-not $Iscc -or -not (Test-Path -LiteralPath $Iscc)) { throw 'Install Inno Setup or pass -Iscc with the compiler path.' }
        $version = ([xml](Get-Content Directory.Build.props -Raw)).Project.PropertyGroup.Version
        & $Iscc "/DAppVersion=$version" installer/SubClick.iss
        if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed' }
        $installer = Join-Path $PSScriptRoot "artifacts/installer/SubClick-$version-win-x64-setup.exe"
        $hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant()
        [IO.File]::WriteAllText("$installer.sha256", "$hash  $([IO.Path]::GetFileName($installer))`n", [Text.UTF8Encoding]::new($false))
        Write-Output "Installer: $installer"
    }
}
finally { Pop-Location }

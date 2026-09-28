# Runs only in disposable CI workers: never removes a developer's installation.
param([Parameter(Mandatory)][string]$Installer)
$ErrorActionPreference = 'Stop'
if ($env:CI -ne 'true') { throw 'Installer lifecycle tests are restricted to disposable CI workers.' }
$appDir = Join-Path $env:LOCALAPPDATA 'Programs\SubClick'
$settingsDir = Join-Path $env:LOCALAPPDATA 'SubClick'
$shortcut = Join-Path $env:APPDATA 'Microsoft\Windows\Start Menu\Programs\SubClick.lnk'
$uninstallKey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\{CBDA8964-A820-4A6A-8D62-27BC7B29C45B}_is1'
if ((Test-Path -LiteralPath $appDir) -or (Test-Path -LiteralPath $settingsDir) -or (Test-Path -LiteralPath $uninstallKey)) {
    throw 'SubClick already exists on this worker; refusing to touch it.'
}
$source = Get-Content (Join-Path $PSScriptRoot '..\src\SubClick.Core\VideoFiles.cs') -Raw
$extensions = [regex]::Matches($source, '"(\.[a-z0-9]+)"') | ForEach-Object { $_.Groups[1].Value }
function Assert-That([bool]$Condition, [string]$Message) { if (-not $Condition) { throw $Message }; Write-Output "PASS $Message" }
function Run-Setup([string]$Language, [string]$Task) {
    $log = Join-Path $env:RUNNER_TEMP "subclick-install-$Language-$Task.log"
    $p = Start-Process -FilePath (Resolve-Path $Installer).Path -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',"/LANG=$Language",("/TASKS=$Task"),("/LOG=`"$log`"")) -WindowStyle Hidden -Wait -PassThru
    if ($p.ExitCode -ne 0) { Get-Content -LiteralPath $log -Tail 35; throw "Setup exit $($p.ExitCode)" }
}
function Check-Menu([bool]$Present) {
    foreach ($extension in $extensions) {
        $key = "HKCU:\Software\Classes\SystemFileAssociations\$extension\shell\SubClick"
        Assert-That ((Test-Path -LiteralPath $key) -eq $Present) "Context menu $extension present=$Present"
        if ($Present) {
            $command = (Get-Item -LiteralPath "$key\command").GetValue('')
            Assert-That ($command -eq ('"' + $appDir + '\SubClick.exe" "%1"')) "Quoted video command for $extension"
        }
    }
}
Run-Setup 'brazilianportuguese' 'contextmenu'
Assert-That (Test-Path -LiteralPath "$appDir\SubClick.exe") 'Application installed'
Assert-That (Test-Path -LiteralPath "$appDir\coreclr.dll") 'Private .NET runtime included'
Assert-That (Test-Path -LiteralPath "$appDir\System.Windows.Forms.dll") 'Private Windows Forms runtime included'
Assert-That (Test-Path -LiteralPath $shortcut) 'Start menu shortcut installed'
Assert-That (Test-Path -LiteralPath $uninstallKey) 'Uninstall entry installed'
Check-Menu $true
New-Item -ItemType Directory -Path $settingsDir | Out-Null
$settingsPath = Join-Path $settingsDir 'settings.json'
$settings = '{"SchemaVersion":1,"SubtitleLanguageId":"jpn","InterfaceLanguage":"pt-BR"}'
[IO.File]::WriteAllText($settingsPath, $settings)
Run-Setup 'english' 'contextmenu'
Assert-That ((Get-Content -LiteralPath $settingsPath -Raw) -eq $settings) 'Reinstall preserves preferences'
Check-Menu $true
Run-Setup 'english' '!contextmenu'
Check-Menu $false
Run-Setup 'english' 'contextmenu'
$uninstaller = Join-Path $appDir 'unins000.exe'
$p = Start-Process -FilePath $uninstaller -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART') -WindowStyle Hidden -Wait -PassThru
Assert-That ($p.ExitCode -eq 0) 'Uninstaller succeeds'
Check-Menu $false
Assert-That (-not (Test-Path -LiteralPath "$appDir\SubClick.exe")) 'Application removed'
Assert-That (-not (Test-Path -LiteralPath $shortcut)) 'Shortcut removed'
Assert-That (-not (Test-Path -LiteralPath $uninstallKey)) 'Uninstall entry removed'
Assert-That ((Get-Content -LiteralPath $settingsPath -Raw) -eq $settings) 'Uninstall preserves preferences'

$ErrorActionPreference = 'Stop'
$packageName = 'Pomodoro-Schedule-Notifier'
$exeName = 'PomodoroScheduleNotifier'
$WshShell = New-Object -comObject WScript.Shell
$StartupDir = $WshShell.SpecialFolders("Startup")
$ShortcutPath = "$StartupDir\PomodoroScheduleNotifier.lnk"

Get-Process | Where-Object { $_.ProcessName -eq $exeName } | Stop-Process -Force -ErrorAction SilentlyContinue

Uninstall-ChocolateyZipPackage -PackageName $packageName -ZipFileName $packageName

$installLocation = 'C:\tools\Pomodoro-Schedule-Notifier'
if (Test-Path -Path $installLocation) {
  Write-Host "Removing installation directory at '$installLocation'"
  Remove-Item -Recurse -Force $installLocation
} else {
  Write-Warning "Installation directory not found: '$installLocation'"
}

if (Test-Path -Path $ShortcutPath) {
  Write-Host "Removing shortcut at '$ShortcutPath'"
  Remove-Item -Force $ShortcutPath
} else {
  Write-Warning "Shortcut not found: '$ShortcutPath'"
}

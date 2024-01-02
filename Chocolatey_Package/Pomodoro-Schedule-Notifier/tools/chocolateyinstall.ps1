$ErrorActionPreference = 'Stop';

$packageName = 'Pomodoro-Schedule-Notifier'
$exeName = 'PomodoroScheduleNotifier'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url = 'https://github.com/mukobi/Pomodoro-Schedule-Notifier/releases/download/v1.0.0/PomodoroScheduleNotifier.v1.0.0.zip'
$installLocation = 'C:\tools\Pomodoro-Schedule-Notifier'

$packageArgs = @{
  packageName    = $packageName
  unzipLocation  = $installLocation
  url            = $url
  checksum       = 'db1fdb5eb066299ab081aa7e84f3a30e1864b0339350246660488f1c6d48795d'
  checksumType   = 'sha256'
}

Get-Process | Where-Object { $_.ProcessName -eq $exeName } | Stop-Process -Force -ErrorAction SilentlyContinue

Install-ChocolateyZipPackage @packageArgs

$WshShell = New-Object -comObject WScript.Shell
$StartupDir = $WshShell.SpecialFolders("Startup")
Install-ChocolateyShortcut -ShortcutFilePath "$StartupDir\PomodoroScheduleNotifier.lnk" -TargetPath "$installLocation\PomodoroScheduleNotifier.exe" -WorkingDirectory $installLocation

Start-Process -FilePath "$installLocation\PomodoroScheduleNotifier.exe" -WorkingDirectory $installLocation

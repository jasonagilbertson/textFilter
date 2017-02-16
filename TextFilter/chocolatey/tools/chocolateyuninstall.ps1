# chocolatey uninstall for textfilter package jagilber
# 170202

$ErrorActionPreference = 'Continue'

$destFileBaseName = $softwareName = $packageName = "textfilter"
$destFileBaseNameExe = "textfilter.exe"
$packageNameZip = "textfilter.exe.zip"

$allUsers = "$($env:ALLUSERSPROFILE)\Microsoft\Windows\Start Menu\Programs"
$currentUser = "$($env:USERPROFILE)\Start Menu\Programs"
$programDir = "$($env:ProgramFiles)\$($destFileBaseName)"
$programDirFile = "$($programDir)\$($destFileBaseNameExe)"

$error.Clear()

# unregister fta
Start-Process -FilePath "cmd.exe" -ArgumentList "/c `"$($programDirFile)`" /unregisterfta" -WorkingDirectory $programDir -NoNewWindow -Wait

$uninstalled = $false

[array]$key = Get-UninstallRegistryKey -SoftwareName $softwareName

if ($key.Count -eq 1) {
    $key | % {
    $file = "$($_.UninstallString)"

    Uninstall-ChocolateyPackage -PackageName $packageName `
                                -FileType $installerType `
                                -SilentArgs "$silentArgs" `
                                -ValidExitCodes $validExitCodes `
                                -File "$file"
    }
} elseif ($key.Count -eq 0) {
    Write-Warning "$packageName has already been uninstalled by other means."
} elseif ($key.Count -gt 1) {
    Write-Warning "$key.Count matches found!"
    Write-Warning "To prevent accidental data loss, no programs will be uninstalled."
    Write-Warning "Please alert package maintainer the following keys were matched:"
    $key | % {Write-Warning "- $_.DisplayName"}
}

Uninstall-ChocolateyZipPackage $packageName $packageNameZip 
## Remove any shortcuts you added

# cleanup old program files
if([IO.Directory]::Exists($programDir))
{
    [IO.Directory]::Delete($programDir, $true)
}

# delete shortcut in allusers start menu
if([IO.File]::Exists("$($allusers)\$($destFileBaseName).lnk"))
{
    [IO.File]::Delete("$($allusers)\$($destFileBaseName).lnk")
}

# delete shortcut in current user start menu
if([IO.File]::Exists("$($currentuser)\$($destFileBaseName).lnk"))
{
    [IO.File]::Delete("$($currentuser)\$($destFileBaseName).lnk")
}
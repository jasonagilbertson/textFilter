# IMPORTANT: Before releasing this package, copy/paste the next 2 lines into PowerShell to remove all comments from this file:
#   $f='c:\path\to\thisFile.ps1'
#   gc $f | ? {$_ -notmatch "^\s*#"} | % {$_ -replace '(^.*?)\s*?[^``]#.*','$1'} | Out-File $f+".~" -en utf8; mv -fo $f+".~" $f

## NOTE: In 80-90% of the cases (95% with licensed versions due to Package Synchronizer and other enhancements), you may 
## not need this file due to AutoUninstaller. See https://chocolatey.org/docs/commands-uninstall

## If this is an MSI, cleaning up comments is all you need.
## If this is an exe, change installerType and silentArgs
## Auto Uninstaller should be able to detect and handle registry uninstalls (if it is turned on, it is in preview for 0.9.9).
## - https://chocolatey.org/docs/helpers-uninstall-chocolatey-package

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
# Get-UninstallRegistryKey is new to 0.9.10, if supporting 0.9.9.x and below,
# take a dependency on "chocolatey-uninstall.extension" in your nuspec file.
# This is only a fuzzy search if $softwareName includes '*'. Otherwise it is 
# exact. In the case of versions in key names, we recommend removing the version
# and using '*'.
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


## OTHER HELPERS
## https://chocolatey.org/docs/helpers-reference
Uninstall-ChocolateyZipPackage $packageName $packageNameZip # Only necessary if you did not unpack to package directory - see https://chocolatey.org/docs/helpers-uninstall-chocolatey-zip-package
#Uninstall-ChocolateyEnvironmentVariable # 0.9.10+ - https://chocolatey.org/docs/helpers-uninstall-chocolatey-environment-variable 
#Uninstall-BinFile # Only needed if you used Install-BinFile - see https://chocolatey.org/docs/helpers-uninstall-bin-file
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

# chocolatey install for textfilter package jagilber

$ErrorActionPreference = 'Continue'

$url = 'https://github.com/jasonagilbertson/textFilter/releases/download/textFilter.exe.zip/textfilter.exe.zip'
#$checksum = "BD140BC42E4F5FFB13E22268BDB391B3" #optional

$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)" # c:\programdata\chocolatey\lib\textfilter\tools

$destFileNameZip = [IO.Path]::GetFileName($url) # textfilter.exe.zip
$destFile = "$($toolsDir)\$([IO.Path]::GetFileName($url))" # c:\programdata\chocolatey\lib\textfilter\tools\textfilter.exe.zip
$destFileBaseNameExe = [IO.Path]::GetFileNameWithoutExtension($destFileNameZip) # textfilter.exe
$packageName = $destFileBaseName = [IO.Path]::GetFileNameWithoutExtension($destFileBaseNameExe) # textfilter

$allUsers = "$($env:ALLUSERSPROFILE)\Microsoft\Windows\Start Menu\Programs"
$currentUser = "$($env:USERPROFILE)\Start Menu\Programs"
$programDir = "$($env:ProgramFiles)\$($destFileBaseName)" # c:\program files\textfilter
$programDirFile = "$($programDir)\$($destFileBaseNameExe)" # c:\program files\textfilter\textfilter.exe

$error.Clear()

# download url zip, extract, and install
#Get-ChocolateyWebFile -PackageName $packageName -FileFullPath $destFile -Url $url #-checksum $checksum -checksumtype "sha256"
Install-ChocolateyZipPackage -PackageName $packageName -Url $url -UnzipLocation $programDir #-checksum $checksum -checksumtype "sha256"
    
# unzip
#Get-ChocolateyUnzip $destFile $programDir

# register fta
Start-Process -FilePath "cmd.exe" -ArgumentList "/c `"$($programDirFile)`" /registerfta" -WorkingDirectory $programDir -NoNewWindow -Wait
#Install-ChocolateyExplorerMenuItem $packageName $packageName $programDirFile
#Install-ChocolateyFileAssociation -Extension ".log" $programDirFile
#Install-ChocolateyFileAssociation -Extension ".rvf" $programDirFile


$error.Clear()

# create shortcut in allusers start menu 
Install-ChocolateyShortcut `
        -ShortcutFilePath "$($allUsers)\$($destFileBaseName).lnk" `
        -TargetPath $programDirFile `
        -WorkDirectory $programDir `
        -Arguments "" `
        -IconLocation $programDirFile `
        -Description $destFileBaseName

if($error)
{
    # create shortcut in current user start menu
    Install-ChocolateyShortcut `
        -ShortcutFilePath "$($currentUser)\$($destFileBaseName).lnk" `
        -TargetPath $programDirFile `
        -WorkDirectory $programDir `
        -Arguments "" `
        -IconLocation $programDirFile `
        -Description $destFileBaseName
}

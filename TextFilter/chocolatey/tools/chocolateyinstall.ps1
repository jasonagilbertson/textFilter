# chocolatey install for textfilter package jagilber

$ErrorActionPreference = 'Continue'

$url = 'https://github.com/jasonagilbertson/textFilter/releases/download/textFilter.exe.zip/textfilter.exe.zip'
#$checksum = "BD140BC42E4F5FFB13E22268BDB391B3"

$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

$destFileNameZip = [IO.Path]::GetFileName($url)
$destFile = "$($toolsDir)\$($destFileNameZip)"
$destFileBaseNameExe = [IO.Path]::GetFileNameWithoutExtension($destFileNameZip)
$destFileBaseName = [IO.Path]::GetFileNameWithoutExtension($destFileBaseNameExe)
$packageName = $destFileBaseName
$allUsers = "$($env:ALLUSERSPROFILE)\Microsoft\Windows\Start Menu\Programs"
$currentUser = "$($env:USERPROFILE)\Start Menu\Programs"
$programDir = "$($env:ProgramFiles)\$($destFileBaseName)"
$programDirFile = "$($programDir)\$($destFileBaseNameExe)"

#-----------------------------------------------------------------------------------------
function main()
{
    $error.Clear()

    # cleanup old program files
    if([IO.Directory]::Exists($programDir))
    {
        [IO.Directory]::Delete($programDir, $true)
    }
    
    [IO.Directory]::CreateDirectory($programDir)
    
    # download url
    Get-ChocolateyWebFile -PackageName $packageName -FileFullPath $destFile -Url $url #-checksum $checksum -checksumtype "sha256"
    
    # unzip
    Get-ChocolateyUnzip $destFile $programDir

    # register fta
    Start-Process -FilePath "cmd.exe" -ArgumentList "/c `"$($programDirFile)`" /registerfta" -WorkingDirectory $programDir -NoNewWindow

    $error.Clear()

    # create shortcut in allusers start menu 
    install-shortcut -path $allUsers

    if($error)
    {
        # create shortcut in current user start menu
        install-shortcut -path $currentUser
    }
}
#-----------------------------------------------------------------------------------------

function install-shortcut($path)
{
    return (Install-ChocolateyShortcut `
          -ShortcutFilePath "$($path)\$($destFileBaseName).lnk" `
          -TargetPath $programDirFile `
          -WorkDirectory $programDir `
          -Arguments "" `
          -IconLocation $programDirFile `
          -Description $destFileBaseName)
}
#-----------------------------------------------------------------------------------------

main

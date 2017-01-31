
$ErrorActionPreference = 'Stop';

$packageName= 'textFilter'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://cdn.rawgit.com/jasonagilbertson/textFilter/releases/download/textFilter.exe.zip/textfilter.exe.zip'

$packageArgs = @{
  packageName   = $packageName
  unzipLocation = $toolsDir
  fileType      = 'EXE_MSI_OR_MSU'
  url           = $url

  softwareName  = 'textFilter*'

  checksum      = 'F3C477D83F5A915D97C63C7D2BBF25C11F97B0F0ADE17429B32A27CDCE257261'
  checksumType  = 'sha256'

  silentArgs    = "/qn /norestart /l*v `"$($env:TEMP)\$($packageName).$($env:chocolateyPackageVersion).MsiInstall.log`""
  validExitCodes= @(0, 3010, 1641)
}









Get-ChocolateyUnzip $url $toolsDir











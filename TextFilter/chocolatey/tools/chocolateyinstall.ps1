
$ErrorActionPreference = 'Stop';

$packageName= 'textFilter'
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/jasonagilbertson/textFilter/releases/download/textFilter.exe.zip/textfilter.exe.zip'

Get-ChocolateyUnzip $url $toolsDir











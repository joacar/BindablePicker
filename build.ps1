[CmdletBinding()]
#Param(
#    [string]$releaseNotes = 'None',
#    [bool]$clean = $false
#    )

# Members
$config = 'Release'
$version = '1.1.0'
$nuspec = 'nuspecs\jpc.bindablepicker.nuspec'
$assemblyPath = Join-Path (Split-Path -parent $Script:MyInvocation.MyCommand.Definition) "src\JPC.BindablePicker\bin\$config\JPC.BindablePicker.dll"

function Setup-Nuget
{
	$nugetPath = Join-Path "${env:APPDATA}" 'NuGet'
    if(-not (Test-Path $nugetPath)) 
	{
		Create-Folder $nugetPath
	}
	$nugetExe = Join-Path $nugetPath 'nuget.exe'
	if(-not (Test-Path $nugetExe)) 
	{
        $url = 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe'
		Write-Host "Downloading nuget.exe from '$url' to '$nugetPath' ..."
		Invoke-WebRequest $url -OutFile $nugetExe
	}
    $nugetExe
}

function Pack-Nuget([string]$nuspec, [string]$properties, [string]$outDir)
{
	if(-not $outDir)
	{
		# Use folder that scrip is executing from
		$outDir = Split-Path -parent $Script:MyInvocation.MyCommand.Definition
	}
    if(-not (Test-Path $outDir))
    {
        Create-Folder $outDir
    }
	Write-Host "Packaging $nuspec to $outDir with properties '$properties'"
	$params = @(
		"pack",
		"$nuspec",
		"-o",
		"$outDir",
		"-p",
		"$properties"
	)
	& $nuget $params
}

function Get-ProductVersion ([string]$assemblyPath)
{
    if(-not (Test-Path $assemblyPath))
    {
        Write-Host "Could not find file '$assemblyPath'"
        return
    }
	(Get-Item $assemblyPath).VersionInfo.ProductVersion
}

# Check for nuget.exe
$nuget = Setup-Nuget

# Run tests

# Build solution upon success
$version = Get-ProductVersion $assemblyPath

# Package nuget

$releaseNotes = "https://github.com/joacar/BindablePicker/releases/tag/v$version"
$properties = "version=$($version);releaseNotes='$($releaseNotes)';config=$($config)"
Pack-Nuget $nuspec $properties
# Sets the package identity used by this fork's releases.
# The publisher must match the subject of the signing certificate (FilesFork.cer).

param(
    [Parameter(Mandatory)] [string]$PackageManifestPath,
    [Parameter(Mandatory)] [string]$Version,
    [string]$Name = "FilesFork",
    [string]$Publisher = "CN=shore-skye",
    [string]$PublisherDisplayName = "shore-skye",
    [string]$DisplayName = "Files"
)

if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
    throw "Version '$Version' must have four numeric parts, for example 4.3.3.1"
}

[xml]$xmlDoc = Get-Content $PackageManifestPath

$xmlDoc.Package.Identity.Name = $Name
$xmlDoc.Package.Identity.Publisher = $Publisher
$xmlDoc.Package.Identity.Version = $Version
$xmlDoc.Package.Properties.DisplayName = $DisplayName
$xmlDoc.Package.Properties.PublisherDisplayName = $PublisherDisplayName
$xmlDoc.Package.Applications.Application.VisualElements.DisplayName = $DisplayName
$xmlDoc.Package.Applications.Application.VisualElements.DefaultTile.ShortName = $DisplayName

$xmlDoc.Save((Resolve-Path $PackageManifestPath))
Write-Host "Package identity: $Name $Version ($Publisher)"

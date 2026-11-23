# Installs this fork of Files from an extracted release zip.
# 1. Trusts the fork's signing certificate (needs administrator rights, asks via UAC)
# 2. Installs the Windows App Runtime framework packages that ship alongside it
# 3. Installs (or updates) the Files package

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot

# The Appx cmdlets are only reliable in Windows PowerShell 5.1
if ($PSVersionTable.PSEdition -ne 'Desktop') {
    & "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath
    exit $LASTEXITCODE
}

$cer = Join-Path $root 'FilesFork.cer'
$thumbprint = (New-Object System.Security.Cryptography.X509Certificates.X509Certificate2 $cer).Thumbprint
if (-not (Test-Path "Cert:\LocalMachine\TrustedPeople\$thumbprint")) {
    Write-Host 'Trusting the package signing certificate (a UAC prompt will appear)...'
    $import = "Import-Certificate -FilePath '$cer' -CertStoreLocation Cert:\LocalMachine\TrustedPeople | Out-Null"
    $p = Start-Process powershell.exe -Verb RunAs -Wait -PassThru -ArgumentList '-NoProfile', '-Command', $import
    if ($p.ExitCode -ne 0 -or -not (Test-Path "Cert:\LocalMachine\TrustedPeople\$thumbprint")) {
        throw 'The certificate was not imported, so Windows will refuse the package.'
    }
}

$arch = if ($env:PROCESSOR_ARCHITECTURE -eq 'ARM64') { 'arm64' } else { 'x64' }
$deps = @(Get-ChildItem (Join-Path $root "Dependencies\$arch") -Filter *.msix -ErrorAction SilentlyContinue)
Add-Type -AssemblyName System.IO.Compression.FileSystem
foreach ($dep in $deps) {
    # Skip frameworks whose same or newer version is already installed (they may be in use by other apps)
    $zip = [IO.Compression.ZipFile]::OpenRead($dep.FullName)
    try { $identity = ([xml](New-Object IO.StreamReader ($zip.GetEntry('AppxManifest.xml').Open())).ReadToEnd()).Package.Identity }
    finally { $zip.Dispose() }
    $installed = Get-AppxPackage -Name $identity.Name | Where-Object { $_.Architecture -eq $identity.ProcessorArchitecture -and [version]$_.Version -ge [version]$identity.Version }
    if ($installed) { Write-Host "Dependency $($identity.Name) $($identity.Version) is already installed."; continue }

    Write-Host "Installing dependency $($dep.Name)..."
    try { Add-AppxPackage -Path $dep.FullName }
    catch {
        # A same or newer version is already installed
        if ($_.Exception.HResult -ne 0x80073D06) { Write-Warning $_.Exception.Message }
    }
}

$package = Get-ChildItem $root -Filter *.msix | Select-Object -First 1
if (-not $package) { throw 'No .msix package found next to this script.' }

Write-Host "Installing $($package.Name)..."
$addArgs = @{ Path = $package.FullName; ForceApplicationShutdown = $true }
if ($deps.Count -gt 0) { $addArgs.DependencyPath = @($deps | ForEach-Object FullName) }
Add-AppxPackage @addArgs

Write-Host ''
Write-Host 'Files is installed. Open it from the Start menu.'
Write-Host 'To make it the default file manager: Settings > Advanced > Set Files as the default file manager.'

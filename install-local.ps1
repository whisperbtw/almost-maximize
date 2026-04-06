param(
    [string]$PackagePath,
    [string]$CertificatePath
)

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$packageRoot = Join-Path $projectRoot "AlmostMaximize\AppPackages"
$fallbackCertificate = Join-Path $projectRoot "AlmostMaximize\AlmostMaximize_TemporaryKey.cer"
$preferredArchitecture = if ($env:PROCESSOR_ARCHITECTURE)
{
    $env:PROCESSOR_ARCHITECTURE.ToLowerInvariant()
}
else
{
    "x64"
}

function Resolve-LatestPackagePath {
    param(
        [string]$SearchRoot,
        [string]$Architecture
    )

    $packages = Get-ChildItem -Path $SearchRoot -Recurse -File -Filter *.msix

    $package = $packages |
        Where-Object {
            $_.FullName -match "(^|[^a-z])$([regex]::Escape($Architecture))([^a-z]|$)"
        } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if (-not $package)
    {
        $package = $packages |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1
    }

    if (-not $package)
    {
        throw "No .msix package was found under '$SearchRoot'. Build/package the project first."
    }

    return $package.FullName
}

function Resolve-CertificatePath {
    param(
        [string]$ResolvedPackagePath,
        [string]$ExplicitCertificatePath,
        [string]$FallbackCertificatePath
    )

    if ($ExplicitCertificatePath)
    {
        return (Resolve-Path -LiteralPath $ExplicitCertificatePath).Path
    }

    $packageDirectory = Split-Path -Parent $ResolvedPackagePath
    $packageCertificate = Get-ChildItem -Path $packageDirectory -File -Filter *.cer |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1

    if ($packageCertificate)
    {
        return $packageCertificate.FullName
    }

    if (Test-Path -LiteralPath $FallbackCertificatePath)
    {
        return (Resolve-Path -LiteralPath $FallbackCertificatePath).Path
    }

    throw "No signing certificate was found next to '$ResolvedPackagePath' or at '$FallbackCertificatePath'."
}

function Ensure-CertificateInStore {
    param(
        [string]$CertificateFile,
        [string]$StorePath
    )

    $certificate = Get-PfxCertificate -FilePath $CertificateFile
    $existingCertificate = Get-ChildItem -Path $StorePath |
        Where-Object Thumbprint -eq $certificate.Thumbprint |
        Select-Object -First 1

    if (-not $existingCertificate)
    {
        Import-Certificate -FilePath $CertificateFile -CertStoreLocation $StorePath | Out-Null
    }
}

function Test-IsAdministrator {
    $currentIdentity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($currentIdentity)
    return $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Test-CertificateHasBasicConstraints {
    param(
        [string]$CertificateFile
    )

    $certificate = Get-PfxCertificate -FilePath $CertificateFile
    return $certificate.Extensions | Where-Object { $_.Oid.Value -eq "2.5.29.19" } | Select-Object -First 1
}

function Get-SideloadStatus {
    $unlockKey = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock"

    if (-not (Test-Path -LiteralPath $unlockKey))
    {
        return [pscustomobject]@{
            AllowAllTrustedApps = $null
            AllowDevelopmentWithoutDevLicense = $null
        }
    }

    $settings = Get-ItemProperty -LiteralPath $unlockKey
    return [pscustomobject]@{
        AllowAllTrustedApps = $settings.AllowAllTrustedApps
        AllowDevelopmentWithoutDevLicense = $settings.AllowDevelopmentWithoutDevLicense
    }
}

if (-not $PackagePath)
{
    $PackagePath = Resolve-LatestPackagePath -SearchRoot $packageRoot -Architecture $preferredArchitecture
}
else
{
    $PackagePath = (Resolve-Path -LiteralPath $PackagePath).Path
}

$CertificatePath = Resolve-CertificatePath -ResolvedPackagePath $PackagePath -ExplicitCertificatePath $CertificatePath -FallbackCertificatePath $fallbackCertificate

Write-Host "Installing package:"
Write-Host "  $PackagePath"
Write-Host "Using certificate:"
Write-Host "  $CertificatePath"

if (-not (Test-CertificateHasBasicConstraints -CertificateFile $CertificatePath))
{
    throw "The signing certificate is missing the Basic Constraints extension. Regenerate the developer certificate before trying to install this package."
}

$sideloadStatus = Get-SideloadStatus
if (($sideloadStatus.AllowAllTrustedApps -ne 1) -and ($sideloadStatus.AllowDevelopmentWithoutDevLicense -ne 1))
{
    Write-Warning "Windows sideloading / Developer Mode does not appear to be enabled. If installation fails with 0x80073CFF, enable Developer Mode in Windows settings and try again."
}

Ensure-CertificateInStore -CertificateFile $CertificatePath -StorePath "Cert:\CurrentUser\Root"
Ensure-CertificateInStore -CertificateFile $CertificatePath -StorePath "Cert:\CurrentUser\TrustedPeople"

if (Test-IsAdministrator)
{
    Ensure-CertificateInStore -CertificateFile $CertificatePath -StorePath "Cert:\LocalMachine\Root"
    Ensure-CertificateInStore -CertificateFile $CertificatePath -StorePath "Cert:\LocalMachine\TrustedPeople"
}

try
{
    Add-AppxPackage -Path $PackagePath -ForceUpdateFromAnyVersion -ErrorAction Stop
}
catch
{
    $message = $_.Exception.Message

    if ($message -match "0x80073CFF")
    {
        throw "Windows rejected the package because sideloading / Developer Mode is disabled. Enable Developer Mode and retry the installation."
    }

    if ($message -match "0x800B0109")
    {
        throw "Windows does not trust the package signing chain yet. Confirm the package was signed with the same certificate passed to this script and that the certificate is imported into the trusted stores for the current user."
    }

    throw
}

Get-AppxPackage |
    Where-Object { $_.Name -eq "AlmostMaximize" } |
    Select-Object Name, PackageFullName, InstallLocation

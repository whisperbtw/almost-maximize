# Local Setup

This project is meant to run locally as a PowerToys Command Palette extension.

## Requirements

- Windows
- PowerToys installed
- Command Palette enabled in PowerToys
- .NET SDK
- Developer Mode enabled in Windows

## Build

```powershell
dotnet build .\AlmostMaximize\AlmostMaximize.csproj -p:RuntimeIdentifier=win-x64
```

## Package

```powershell
dotnet publish .\AlmostMaximize\AlmostMaximize.csproj -c Release -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageSigningEnabled=false -p:AppxPackageDir=AppPackages\x64-manual\
```

## Install

Use the helper script:

```powershell
.\install-local.ps1
```

## After Reinstalling

- close Command Palette
- open it again
- if the extension still does not refresh, restart PowerToys completely

## Common Problems

### The package will not install

Check:

- Developer Mode is enabled
- the certificate is trusted
- the old package was removed before reinstalling a changed package with the same version

### The icon does not update

Restart PowerToys after reinstalling the package.

### The extension does not show up

Check logs in:

```text
%LOCALAPPDATA%\Microsoft\PowerToys\CmdPal\Logs
```

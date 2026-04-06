# Local Setup

Rode localmente como uma extensão do PowerToys Command Palette.

## Requisitos

- Windows 11
- PowerToys instalado com Command Palette ativado
- .NET SDK
- Developer Mode habilitado no Windows

## Build
```powershell
dotnet build .\AlmostMaximize\AlmostMaximize.csproj -p:RuntimeIdentifier=win-x64
```

## Publicar o pacote
```powershell
dotnet publish .\AlmostMaximize\AlmostMaximize.csproj -c Release -p:Platform=x64 -p:GenerateAppxPackageOnBuild=true -p:AppxPackageSigningEnabled=false -p:AppxPackageDir=AppPackages\x64-manual\
```

## Instalar
```powershell
.\install-local.ps1
```

Depois de reinstalar, feche e reabra o Command Palette. Se a extensão não aparecer, reinicie o PowerToys.

A interface atual usa `90%` como ação padrão. A lista secundária expõe presets por porcentagem e um campo de valor personalizado.

## Problemas comuns

**O pacote não instala**
Verifique se o Developer Mode está ativado, se o certificado é confiável, e se a versão antiga foi removida antes de reinstalar.

**O ícone não atualiza**
Reinicie o PowerToys depois de reinstalar.

**A extensão não aparece**
Veja os logs em `%LOCALAPPDATA%\Microsoft\PowerToys\CmdPal\Logs`.

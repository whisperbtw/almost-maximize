# Architecture

## Como funciona

A extensão roda como um servidor COM empacotado em MSIX.

1. O PowerToys descobre a extensão via manifesto do app extension.
2. Sobe o executável como servidor COM.
3. A extensão expõe os comandos do topo.
4. Ao executar um comando, a janela em foco é redimensionada via Win32.

## Arquivos principais

**`Program.cs`**
Inicia o servidor COM e registra a classe da extensão.

**`AlmostMaximize.cs`**
Implementa `IExtension` e retorna o provider de `ProviderType.Commands`.

**`AlmostMaximizeCommandsProvider.cs`**
Define as entradas do Command Palette: `Almost Maximize` e `Choose margin`.

**`Pages/AlmostMaximizePage.cs`**
Lista os presets de margem: 20, 30, 40, 50 e 60 px.

**`AlmostMaximizeCommand.cs`**
Executa o redimensionamento via Win32:
`GetForegroundWindow` → `MonitorFromWindow` → `GetMonitorInfo` → `ShowWindow` → `MoveWindow`

## O que o comando faz

Pega a janela ativa, restaura se estiver maximizada, lê a área útil do monitor atual, aplica a margem configurada e reposiciona a janela.

## Assets

Ícones do pacote ficam em `AlmostMaximize/Assets`. O ícone que aparece nos resultados do Command Palette é o `Square44x44Logo.targetsize-24_altform-unplated.png`.

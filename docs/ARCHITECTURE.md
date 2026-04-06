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
Define as entradas do Command Palette: `Almost Maximize` e `Choose percentage`.

**`Pages/AlmostMaximizePage.cs`**
Lista os presets por porcentagem: 90%, 80%, 70%, 60% e 50%.

**`Pages/CustomPercentagePage.cs`**
Renderiza o formulário para porcentagem personalizada.

**`AlmostMaximizeCommand.cs`**
Executa o redimensionamento via Win32:
`GetForegroundWindow` → `MonitorFromWindow` → `GetMonitorInfo` → `ShowWindow` → `MoveWindow`

## O que o comando faz

Pega a janela ativa, restaura se estiver maximizada, lê a área útil do monitor atual, calcula o tamanho final com base na porcentagem escolhida e reposiciona a janela no centro.

Janelas do shell do Windows, como taskbar e system tray, são ignoradas.

## Assets

Ícones do pacote ficam em `AlmostMaximize/Assets`. O ícone que aparece nos resultados do Command Palette é o `Square44x44Logo.targetsize-24_altform-unplated.png`.

# Almost Maximize

Extensao para o PowerToys Command Palette que redimensiona a janela ativa para quase ocupar toda a area util do monitor, mantendo uma margem configurada.

## Preview

![Preview da extensao](docs/almost-maximize-preview-realistic.png)

Fluxo esperado:

1. Buscar por `max` no Command Palette.
2. Abrir o item `Resize active window`.
3. Executar `Apply 30 px margin`.

## Estrutura

- `AlmostMaximize/AlmostMaximizeCommandsProvider.cs`: comando de topo exibido no Command Palette.
- `AlmostMaximize/Pages/AlmostMaximizePage.cs`: pagina com as acoes da extensao.
- `AlmostMaximize/AlmostMaximizeCommand.cs`: redimensionamento da janela ativa.
- `install-local.ps1`: instalacao local do pacote `.msix`.

## Instalacao local

Requisitos:

- PowerToys com Command Palette habilitado
- Modo de desenvolvedor do Windows ativado
- Certificado de desenvolvimento confiado no Windows

Depois de gerar e assinar o pacote, instale com:

```powershell
.\install-local.ps1
```

# Architecture

## Overview

The extension is a packaged COM-based Command Palette provider.

At runtime:

1. PowerToys discovers the MSIX package through the Command Palette app extension manifest.
2. PowerToys starts the packaged executable as a COM server.
3. The extension exposes top-level commands.
4. A selected command resizes the current foreground window through Win32 APIs.

## Main Files

### `AlmostMaximize/Program.cs`

Starts the COM server and registers the extension class used by PowerToys.

### `AlmostMaximize/AlmostMaximize.cs`

Implements `IExtension` and returns the provider for `ProviderType.Commands`.

### `AlmostMaximize/AlmostMaximizeCommandsProvider.cs`

Defines the top-level Command Palette entries:

- `Almost Maximize`
- `Choose margin`

### `AlmostMaximize/Pages/AlmostMaximizePage.cs`

Defines the preset list:

- 20 px
- 30 px
- 40 px
- 50 px
- 60 px

### `AlmostMaximize/AlmostMaximizeCommand.cs`

Runs the resize logic and calls Win32 APIs:

- `GetForegroundWindow`
- `MonitorFromWindow`
- `GetMonitorInfo`
- `ShowWindow`
- `MoveWindow`

## Behavior

The command:

- finds the active window
- restores it if needed
- reads the current monitor work area
- applies a symmetric margin
- resizes the window to fit the remaining space

## Assets

The package includes app icons and the Command Palette result icon under:

- `AlmostMaximize/Assets`

The most relevant result icon is:

- `Square44x44Logo.targetsize-24_altform-unplated.png`

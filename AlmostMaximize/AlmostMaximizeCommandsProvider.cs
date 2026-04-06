// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AlmostMaximize;

public partial class AlmostMaximizeCommandsProvider : CommandProvider
{
    private const string ResultIconPath = "Assets\\Square44x44Logo.targetsize-24_altform-unplated.png";

    private readonly ICommandItem[] _commands;

    public AlmostMaximizeCommandsProvider()
    {
        DisplayName = "Window Resize Tools";
        Icon = IconHelpers.FromRelativePath(ResultIconPath);
        _commands = [
            new CommandItem(new AlmostMaximizeCommand(30))
            {
                Title = "Almost Maximize",
                Icon = IconHelpers.FromRelativePath(ResultIconPath),
            },
            new CommandItem(new AlmostMaximizePage())
            {
                Title = "Choose margin",
                Icon = IconHelpers.FromRelativePath(ResultIconPath),
            },
        ];
    }

    public override ICommandItem[] TopLevelCommands()
    {
        return _commands;
    }

}

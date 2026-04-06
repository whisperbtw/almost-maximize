// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Linq;

namespace AlmostMaximize;

internal sealed partial class AlmostMaximizePage : ListPage
{
    private static readonly int[] Margins = [20, 30, 40, 50, 60];

    public AlmostMaximizePage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.targetsize-24_altform-unplated.png");
        Title = "Choose margin";
        Name = "Select a preset";
    }

    public override IListItem[] GetItems()
    {
        return [
            ..Margins.Select(margin => new ListItem(new AlmostMaximizeCommand(margin))
            {
                Title = $"Apply {margin} px margin",
                Subtitle = $"Resize the active window to fill the work area with a {margin} px border.",
            }),
        ];
    }
}

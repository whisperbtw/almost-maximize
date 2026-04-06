// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Linq;

namespace AlmostMaximize;

internal sealed partial class AlmostMaximizePage : ListPage
{
    private static readonly double[] Percentages = [90, 80, 70, 60, 50];

    public AlmostMaximizePage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.targetsize-24_altform-unplated.png");
        Title = "Choose percentage";
        Name = "Select a preset size or create your own";
    }

    public override IListItem[] GetItems()
    {
        return [
            ..Percentages.Select(percentage => new ListItem(new AlmostMaximizeCommand(percentage))
            {
                Title = $"Apply {percentage:0.#}%",
                Subtitle = $"Resize the active window to {percentage:0.#}% of the maximized size.",
            }),
            new ListItem(new CustomPercentagePage())
            {
                Title = "Custom percentage",
                Subtitle = "Enter your own size percentage.",
            },
        ];
    }
}

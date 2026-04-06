// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;
using System.Collections.Generic;
using System.Linq;

namespace AlmostMaximize;

internal sealed partial class AlmostMaximizePage : ListPage
{
    private static readonly int[] DefaultPercentages = [90, 80, 70];
    private readonly List<int> _customPercentages;

    public AlmostMaximizePage()
    {
        _customPercentages = [..CustomPercentageStore.Load()];
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.targetsize-24_altform-unplated.png");
        Title = "Choose percentage";
        Name = "Select a preset size or create your own";
    }

    public override IListItem[] GetItems()
    {
        var allPercentages = DefaultPercentages
            .Concat(_customPercentages)
            .Distinct()
            .OrderByDescending(static percentage => percentage);

        return [
            ..allPercentages.Select(CreatePercentageItem),
            new ListItem(new CustomPercentagePage(this))
            {
                Title = "Custom percentage",
                Subtitle = "Enter your own size percentage.",
            },
        ];
    }

    internal bool SaveCustomPercentage(int percentage)
    {
        if (!CustomPercentageStore.IsValid(percentage))
        {
            return false;
        }

        if (DefaultPercentages.Contains(percentage) || _customPercentages.Contains(percentage))
        {
            return false;
        }

        _customPercentages.Add(percentage);
        PersistAndRefresh();
        return true;
    }

    internal void RemoveCustomPercentage(int percentage)
    {
        if (_customPercentages.Remove(percentage))
        {
            PersistAndRefresh();
        }
    }

    private void PersistAndRefresh()
    {
        CustomPercentageStore.Save(_customPercentages);
        _customPercentages.Sort(static (left, right) => right.CompareTo(left));
        RaiseItemsChanged();
    }

    private ListItem CreatePercentageItem(int percentage)
    {
        var item = new ListItem(new AlmostMaximizeCommand(percentage))
        {
            Title = $"Apply {percentage}%",
            Subtitle = $"Resize the active window to {percentage}% of the maximized size.",
        };

        if (_customPercentages.Contains(percentage))
        {
            item.MoreCommands = [
                new CommandContextItem(new AnonymousCommand(() => RemoveCustomPercentage(percentage))
                {
                    Name = $"Delete {percentage}%",
                    Result = CommandResult.KeepOpen(),
                })
                {
                    Title = "Delete custom percentage",
                    Subtitle = $"Remove {percentage}% from the saved list.",
                    IsCritical = true,
                },
            ];
        }

        return item;
    }
}

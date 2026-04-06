// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.Json.Nodes;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace AlmostMaximize;

internal sealed partial class CustomPercentagePage : ContentPage
{
    private readonly CustomPercentageForm _form = new();

    public CustomPercentagePage()
    {
        Icon = IconHelpers.FromRelativePath("Assets\\Square44x44Logo.targetsize-24_altform-unplated.png");
        Title = "Custom percentage";
        Name = "Enter a custom size";
    }

    public override IContent[] GetContent() => [_form];
}

internal sealed partial class CustomPercentageForm : FormContent
{
    public CustomPercentageForm()
    {
        TemplateJson = $$"""
        {
          "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
          "type": "AdaptiveCard",
          "version": "1.6",
          "body": [
            {
              "type": "TextBlock",
              "size": "Medium",
              "weight": "Bolder",
              "text": "Custom margin"
            },
            {
              "type": "TextBlock",
              "wrap": true,
              "spacing": "Small",
              "text": "Use a value between {{WindowResizer.MinPercentage:0.#}} and {{WindowResizer.MaxPercentage:0.#}} percent of the maximized size."
            },
            {
              "type": "Input.Number",
              "id": "percentage",
              "label": "Window size percentage",
              "placeholder": "Example: 85",
              "min": {{WindowResizer.MinPercentage}},
              "max": {{WindowResizer.MaxPercentage}},
              "value": 90
            }
          ],
          "actions": [
            {
              "type": "Action.Submit",
              "title": "Apply"
            }
          ]
        }
        """;
    }

    public override CommandResult SubmitForm(string payload)
    {
        var percentageText = JsonNode.Parse(payload)?["percentage"]?.ToString();
        if (!AlmostMaximizeRunner.TryParsePercentage(percentageText, out var percentage))
        {
            return CommandResult.ShowToast(new ToastArgs()
            {
                Message = "Enter a valid percentage before applying.",
                Result = CommandResult.KeepOpen(),
            });
        }

        if (percentage is < WindowResizer.MinPercentage or > WindowResizer.MaxPercentage)
        {
            return CommandResult.ShowToast(new ToastArgs()
            {
                Message = $"Choose a value between {WindowResizer.MinPercentage:0.#}% and {WindowResizer.MaxPercentage:0.#}%.",
                Result = CommandResult.KeepOpen(),
            });
        }

        return AlmostMaximizeRunner.ScheduleResize(percentage);
    }
}

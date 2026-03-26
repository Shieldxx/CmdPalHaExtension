// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using CmdPalHaExtension.Helpers;
using CmdPalHaExtension.Models;
using CmdPalHaExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHaExtension.Commands;

internal sealed partial class ActivateEntityCommand : InvokableCommand
{
    private readonly HaEntityState _entity;
    private readonly string _domain;

    public ActivateEntityCommand(HaEntityState entity)
    {
        _entity = entity;
        _domain = entity.GetDomain();
        Name = _domain switch
        {
            "scene" => "Activate",
            "script" => "Run",
            "button" => "Press",
            _ => "Activate",
        };
        Icon = MdiIconProvider.GetIconInfo(entity);
    }

    public override ICommandResult Invoke()
    {
        if (_entity.State == "unavailable")
        {
            return CommandResult.ShowToast($"{_entity.GetFriendlyName()} is unavailable");
        }

        var name = _entity.GetFriendlyName();
        var service = _domain switch
        {
            "scene" => "turn_on",
            "script" => "trigger",
            "button" => "press",
            _ => "turn_on",
        };

        _ = Task.Run(async () =>
        {
            await HaClient.CallServiceAsync(_domain, service, _entity.EntityId).ConfigureAwait(false);
            await EntityCache.RefreshAsync().ConfigureAwait(false);
        });
        return CommandResult.ShowToast($"Activated {name}");
    }
}

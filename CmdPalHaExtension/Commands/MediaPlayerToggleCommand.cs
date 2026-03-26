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

internal sealed partial class MediaPlayerToggleCommand : InvokableCommand
{
    private readonly HaEntityState _entity;

    public MediaPlayerToggleCommand(HaEntityState entity)
    {
        _entity = entity;
        Name = entity.State == "playing" ? "Pause" : "Play";
        Icon = MdiIconProvider.GetIconInfo(entity);
    }

    public override ICommandResult Invoke()
    {
        if (_entity.State == "unavailable")
        {
            return CommandResult.ShowToast($"{_entity.GetFriendlyName()} is unavailable");
        }

        var name = _entity.GetFriendlyName();
        _ = Task.Run(async () =>
        {
            await HaClient.CallServiceAsync("media_player", "media_play_pause", _entity.EntityId).ConfigureAwait(false);
            await EntityCache.RefreshAsync().ConfigureAwait(false);
        });
        return CommandResult.ShowToast($"Toggled {name}");
    }
}

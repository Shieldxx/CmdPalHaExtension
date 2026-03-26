// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CmdPalHaExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHaExtension.Commands;

internal sealed partial class ToggleFavoriteCommand : InvokableCommand
{
    private readonly string _entityId;

    public ToggleFavoriteCommand(string entityId)
    {
        _entityId = entityId;
        Name = FavoritesManager.IsFavorite(entityId) ? "Remove from Dock" : "Pin to Dock";
        Icon = new IconInfo("\u2B50");
    }

    public override ICommandResult Invoke()
    {
        FavoritesManager.Toggle(_entityId);
        return CommandResult.ShowToast(
            FavoritesManager.IsFavorite(_entityId)
                ? "Pinned to Dock"
                : "Removed from Dock");
    }
}

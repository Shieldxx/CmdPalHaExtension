// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using CmdPalHaExtension.Commands;
using CmdPalHaExtension.Models;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHaExtension.Helpers;

internal static class EntityCommandFactory
{
    public static InvokableCommand CreatePrimaryCommand(HaEntityState entity)
    {
        var domain = entity.GetDomain();
        return domain switch
        {
            "light" or "switch" or "input_boolean" or "fan" or "automation" => new ToggleEntityCommand(entity),
            "scene" or "script" or "button" => new ActivateEntityCommand(entity),
            "lock" => new LockToggleCommand(entity),
            "cover" => new CoverToggleCommand(entity),
            "media_player" => new MediaPlayerToggleCommand(entity),
            _ => new ToggleEntityCommand(entity),
        };
    }

    public static readonly HashSet<string> ControllableDomains =
    [
        "light",
        "switch",
        "input_boolean",
        "fan",
        "automation",
        "scene",
        "script",
        "button",
        "lock",
        "cover",
        "media_player",
        "climate",
        "vacuum",
    ];
}

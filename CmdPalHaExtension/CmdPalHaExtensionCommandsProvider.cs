// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using CmdPalHaExtension.Dock;
using CmdPalHaExtension.Helpers;
using CmdPalHaExtension.Pages;
using CmdPalHaExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHaExtension;

public partial class CmdPalHaExtensionCommandsProvider : CommandProvider
{
    private readonly SettingsManager _settingsManager = new();
    private readonly ICommandItem[] _commands;
    private readonly HaDockBand _dockBand = new();
    private readonly ICommandItem _dockItem;

    public CmdPalHaExtensionCommandsProvider()
    {
        DisplayName = "Home Assistant";
        Icon = MdiIconProvider.GetDomainDefaultIcon("home");

        Settings = _settingsManager.Settings;

        _commands =
        [
            new CommandItem(new EntityListPage(_settingsManager))
            {
                Title = "Home Assistant",
                Subtitle = "Control your smart home",
                Icon = MdiIconProvider.GetDomainDefaultIcon("home"),
            },
        ];

        // Persistent CommandItem wrapping the DynamicListPage dock band.
        // Returning the same instance avoids a full dock re-render on each entity update.
        _dockItem = new CommandItem(_dockBand)
        {
            Title = "Home Assistant",
            Icon = MdiIconProvider.GetDomainDefaultIcon("home"),
        };

        _settingsManager.SettingsUpdated += OnSettingsUpdated;

        FavoritesManager.Load();
        // Re-query GetDockBands() only when favorites are added/removed (rare).
        // Entity state updates are handled by HaDockBand itself via RaiseItemsChanged().
        FavoritesManager.FavoritesChanged += () => RaiseItemsChanged(0);
        ConfigureClient();
    }

    private void OnSettingsUpdated()
    {
        ConfigureClient();
    }

    private void ConfigureClient()
    {
        if (_settingsManager.IsConfigured)
        {
            HaClient.Configure(_settingsManager.HaUrl, _settingsManager.HaToken);
            EntityCache.StartPeriodicRefresh(TimeSpan.FromSeconds(30));
        }
        else
        {
            EntityCache.Stop();
        }
    }

    public override ICommandItem[] TopLevelCommands() => _commands;

    public override ICommandItem[]? GetDockBands()
    {
        if (!_settingsManager.IsConfigured || FavoritesManager.GetFavorites().Count == 0)
        {
            return null;
        }

        return [_dockItem];
    }
}

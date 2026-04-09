// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using CmdPalHaExtension.Helpers;
using CmdPalHaExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHaExtension.Dock;

/// <summary>
/// A DynamicListPage used as a dock band. Command Palette renders each IListItem
/// returned from GetItems() as an individual dock button. Calling RaiseItemsChanged()
/// on this page updates only the HA dock band without re-rendering other extensions.
/// </summary>
internal sealed partial class HaDockBand : DynamicListPage
{
    public HaDockBand()
    {
        Name = "Home Assistant";
        Icon = MdiIconProvider.GetDomainDefaultIcon("home");

        // Update dock icons/subtitles in-place whenever entity states change.
        // This only re-renders this page's items, not the whole dock.
        EntityCache.EntitiesUpdated += OnEntitiesUpdated;
        FavoritesManager.FavoritesChanged += OnEntitiesUpdated;
    }

    private void OnEntitiesUpdated() => RaiseItemsChanged();

    public override void UpdateSearchText(string oldSearch, string newSearch) { }

    public override IListItem[] GetItems()
    {
        var favorites = FavoritesManager.GetFavorites();
        if (favorites.Count == 0)
        {
            return [];
        }

        var items = new List<IListItem>();
        foreach (var id in favorites)
        {
            var entity = EntityCache.GetEntity(id);
            if (entity == null)
            {
                continue;
            }

            var cmd = EntityCommandFactory.CreatePrimaryCommand(entity);
            items.Add(new ListItem(cmd)
            {
                Title = entity.GetFriendlyName(),
                Icon = MdiIconProvider.GetIconInfo(entity),
                Subtitle = entity.State,
            });
        }

        return [.. items];
    }
}

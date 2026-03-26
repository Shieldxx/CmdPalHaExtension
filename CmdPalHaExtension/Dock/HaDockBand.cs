// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using CmdPalHaExtension.Helpers;
using CmdPalHaExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHaExtension.Dock;

internal sealed partial class HaDockBand
{
    private readonly ListItem[] _emptyItems = [];

    public ICommandItem CreateDockItem()
    {
        var items = BuildItems();
        var dockItem = new WrappedDockItem(items, "com.cmdpal.ha.favorites", "Home Assistant");
        return dockItem;
    }

    public IListItem[] BuildItems()
    {
        var favorites = FavoritesManager.GetFavorites();
        if (favorites.Count == 0)
        {
            return _emptyItems;
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

        return items.ToArray();
    }
}

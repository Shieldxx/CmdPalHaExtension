// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CmdPalHaExtension.Commands;
using CmdPalHaExtension.Helpers;
using CmdPalHaExtension.Models;
using CmdPalHaExtension.Services;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHaExtension.Pages;

internal sealed partial class EntityListPage : DynamicListPage
{
    private readonly SettingsManager _settings;
    private bool _initialLoadTriggered;

    public EntityListPage(SettingsManager settings)
    {
        _settings = settings;
        Icon = MdiIconProvider.GetDomainDefaultIcon("home");
        Title = "Home Assistant";
        Name = "Open";
        PlaceholderText = "Search entities...";
        Filters = new HaEntityDomainFilters();

        EntityCache.EntitiesUpdated += OnEntitiesUpdated;
    }

    private void OnEntitiesUpdated()
    {
        RaiseItemsChanged();
    }

    public override void UpdateSearchText(string oldSearch, string newSearch)
    {
        RaiseItemsChanged();
    }

    public override IListItem[] GetItems()
    {
        if (!_settings.IsConfigured)
        {
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = "Home Assistant not configured",
                    Subtitle = "Open extension settings (gear icon) to set your HA URL and access token",
                    Icon = MdiIconProvider.GetDomainDefaultIcon("home"),
                },
            ];
        }

        var entities = EntityCache.Entities;
        if (entities.Length == 0)
        {
            if (!_initialLoadTriggered)
            {
                _initialLoadTriggered = true;
                _ = Task.Run(EntityCache.RefreshAsync);
            }

            if (HaClient.Status == ConnectionStatus.AuthFailed)
            {
                return
                [
                    new ListItem(new NoOpCommand())
                    {
                        Title = "Authentication failed",
                        Subtitle = "Invalid access token. Go to HA > Profile > Security to generate a new one.",
                    },
                ];
            }

            if (HaClient.Status == ConnectionStatus.ConnectionFailed)
            {
                return
                [
                    new ListItem(new NoOpCommand())
                    {
                        Title = "Connection failed",
                        Subtitle = $"Cannot reach Home Assistant at {_settings.HaUrl}. Check if HA is running.",
                    },
                ];
            }

            IsLoading = true;
            return
            [
                new ListItem(new NoOpCommand())
                {
                    Title = "Loading entities...",
                    Subtitle = "Connecting to Home Assistant",
                },
            ];
        }

        IsLoading = false;
        _initialLoadTriggered = true;

        var filtered = entities
            .Where(e => EntityCommandFactory.ControllableDomains.Contains(e.GetDomain()))
            .AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrEmpty(SearchText))
        {
            filtered = filtered.Where(e =>
                e.GetFriendlyName().Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                e.EntityId.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
        }

        // Apply domain filter
        var currentFilterId = (Filters as HaEntityDomainFilters)?.CurrentFilterId;
        if (!string.IsNullOrEmpty(currentFilterId) && currentFilterId != "all")
        {
            filtered = filtered.Where(e => e.GetDomain() == currentFilterId);
        }

        // Group by domain and build list items
        var grouped = filtered
            .GroupBy(e => e.GetDomain())
            .OrderBy(g => g.Key);

        var items = new List<IListItem>();
        foreach (var group in grouped)
        {
            var sectionName = MdiIconProvider.GetFriendlyDomain(group.Key);
            foreach (var entity in group.OrderBy(e => e.GetFriendlyName()))
            {
                var cmd = EntityCommandFactory.CreatePrimaryCommand(entity);
                var subtitle = FormatSubtitle(entity);
                var listItem = new ListItem(cmd)
                {
                    Title = entity.GetFriendlyName(),
                    Subtitle = subtitle,
                    Icon = MdiIconProvider.GetIconInfo(entity),
                    Section = sectionName,
                    Tags = [new Tag() { Text = entity.State }],
                    MoreCommands =
                    [
                        new CommandContextItem(new ToggleFavoriteCommand(entity.EntityId)),
                    ],
                };
                items.Add(listItem);
            }
        }

        return items.ToArray();
    }

    private static string FormatSubtitle(HaEntityState entity)
    {
        var state = entity.State;
        var domain = entity.GetDomain();

        if (state == "unavailable")
        {
            return "(unavailable)";
        }

        return domain switch
        {
            "light" when state == "on" => FormatLightState(entity),
            "climate" => FormatClimateState(entity),
            "media_player" when state == "playing" => FormatMediaState(entity),
            "cover" => state == "open" ? "Open" : "Closed",
            "lock" => state == "locked" ? "Locked" : "Unlocked",
            _ => char.ToUpperInvariant(state[0]) + state[1..],
        };
    }

    private static string FormatLightState(HaEntityState entity)
    {
        var parts = new List<string> { "On" };
        if (entity.Attributes.TryGetValue("brightness", out var br) && br.ValueKind == System.Text.Json.JsonValueKind.Number)
        {
            var pct = (int)(br.GetDouble() / 255.0 * 100);
            parts.Add($"{pct}%");
        }

        if (entity.Attributes.TryGetValue("color_temp_kelvin", out var ct) && ct.ValueKind == System.Text.Json.JsonValueKind.Number)
        {
            parts.Add($"{ct.GetInt32()}K");
        }

        return string.Join(" · ", parts);
    }

    private static string FormatClimateState(HaEntityState entity)
    {
        var parts = new List<string> { char.ToUpperInvariant(entity.State[0]) + entity.State[1..] };
        if (entity.Attributes.TryGetValue("current_temperature", out var temp) && temp.ValueKind == System.Text.Json.JsonValueKind.Number)
        {
            parts.Add($"{temp.GetDouble():F1}°");
        }

        if (entity.Attributes.TryGetValue("temperature", out var target) && target.ValueKind == System.Text.Json.JsonValueKind.Number)
        {
            parts.Add($"→ {target.GetDouble():F1}°");
        }

        return string.Join(" · ", parts);
    }

    private static string FormatMediaState(HaEntityState entity)
    {
        if (entity.Attributes.TryGetValue("media_title", out var title) && title.ValueKind == System.Text.Json.JsonValueKind.String)
        {
            return $"Playing: {title.GetString()}";
        }

        return "Playing";
    }
}

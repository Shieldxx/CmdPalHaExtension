// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CmdPalHaExtension.Models;

namespace CmdPalHaExtension.Services;

internal static class EntityCache
{
    private static HaEntityState[] _entities = [];
    private static Timer? _refreshTimer;
    private static int _refreshing;

    public static event Action? EntitiesUpdated;

    public static HaEntityState[] Entities => _entities;

    public static void StartPeriodicRefresh(TimeSpan interval)
    {
        _refreshTimer?.Dispose();
        _refreshTimer = new Timer(
            _ => _ = RefreshAsync(),
            null,
            TimeSpan.Zero,
            interval);
    }

    public static void Stop()
    {
        _refreshTimer?.Dispose();
        _refreshTimer = null;
    }

    public static async Task RefreshAsync()
    {
        if (Interlocked.CompareExchange(ref _refreshing, 1, 0) != 0)
        {
            return;
        }

        try
        {
            var states = await HaClient.GetStatesAsync().ConfigureAwait(false);
            if (states != null)
            {
                _entities = states;
                EntitiesUpdated?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"EntityCache refresh failed: {ex.Message}");
        }
        finally
        {
            Interlocked.Exchange(ref _refreshing, 0);
        }
    }

    public static HaEntityState? GetEntity(string entityId)
    {
        var entities = _entities;
        foreach (var e in entities)
        {
            if (string.Equals(e.EntityId, entityId, StringComparison.OrdinalIgnoreCase))
            {
                return e;
            }
        }

        return null;
    }
}

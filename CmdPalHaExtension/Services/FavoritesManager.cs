// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using CmdPalHaExtension.Models;

namespace CmdPalHaExtension.Services;

internal static class FavoritesManager
{
    private static readonly string FavoritesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CmdPalHaExtension",
        "favorites.json");

    private static HashSet<string> _favorites = [];

    public static event Action? FavoritesChanged;

    public static bool IsFavorite(string entityId) => _favorites.Contains(entityId);

    public static IReadOnlyCollection<string> GetFavorites() => _favorites;

    public static void Toggle(string entityId)
    {
        if (!_favorites.Remove(entityId))
        {
            _favorites.Add(entityId);
        }

        Save();
        FavoritesChanged?.Invoke();
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(FavoritesPath))
            {
                var json = File.ReadAllText(FavoritesPath);
                var arr = JsonSerializer.Deserialize(json, HaJsonContext.Default.StringArray);
                _favorites = arr != null ? new HashSet<string>(arr) : [];
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load favorites: {ex.Message}");
            _favorites = [];
        }
    }

    private static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FavoritesPath)!);
            var json = JsonSerializer.Serialize(_favorites.ToArray(), HaJsonContext.Default.StringArray);
            File.WriteAllText(FavoritesPath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save favorites: {ex.Message}");
        }
    }
}

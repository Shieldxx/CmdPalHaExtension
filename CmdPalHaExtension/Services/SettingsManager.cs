// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHaExtension.Services;

internal sealed class SettingsManager : JsonSettingsManager
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CmdPalHaExtension");

    private readonly TextSetting _haUrl;
    private readonly TextSetting _haToken;

    public string HaUrl => _haUrl.Value?.Trim() ?? string.Empty;

    public string HaToken => _haToken.Value?.Trim() ?? string.Empty;

    public bool IsConfigured => !string.IsNullOrEmpty(HaUrl) && !string.IsNullOrEmpty(HaToken);

    public event Action? SettingsUpdated;

    public SettingsManager()
    {
        Directory.CreateDirectory(SettingsDir);
        FilePath = Path.Combine(SettingsDir, "settings.json");

        _haUrl = new TextSetting(
            "haUrl",
            "Home Assistant URL",
            "The URL of your Home Assistant instance (e.g. http://homeassistant.local:8123)",
            string.Empty);

        _haToken = new TextSetting(
            "haToken",
            "Long-Lived Access Token",
            "Create in HA: Profile → Security → Long-Lived Access Tokens → Create Token",
            string.Empty);

        Settings.Add(_haUrl);
        Settings.Add(_haToken);

        LoadSettings();

        Settings.SettingsChanged += (_, _) =>
        {
            SaveSettings();
            SettingsUpdated?.Invoke();
        };
    }
}

// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using CmdPalHaExtension.Models;
using Microsoft.CommandPalette.Extensions;
using Microsoft.CommandPalette.Extensions.Toolkit;

namespace CmdPalHaExtension.Helpers;

internal static class MdiIconProvider
{
    private static readonly Dictionary<string, string> FriendlyDomainNames = new(System.StringComparer.OrdinalIgnoreCase)
    {
        ["light"] = "Lights",
        ["switch"] = "Switches",
        ["scene"] = "Scenes",
        ["climate"] = "Climate",
        ["cover"] = "Covers",
        ["fan"] = "Fans",
        ["media_player"] = "Media Players",
        ["automation"] = "Automations",
        ["script"] = "Scripts",
        ["lock"] = "Locks",
        ["vacuum"] = "Vacuums",
        ["input_boolean"] = "Input Booleans",
        ["button"] = "Buttons",
        ["sensor"] = "Sensors",
        ["binary_sensor"] = "Binary Sensors",
        ["camera"] = "Cameras",
    };

    public static string GetFriendlyDomain(string domain)
        => FriendlyDomainNames.TryGetValue(domain, out var name) ? name : domain;

    public static IconInfo GetIconInfo(HaEntityState entity)
    {
        var state = entity.State;
        var domain = entity.GetDomain();

        if (state == "unavailable" || state == "unknown")
            return Icon("unavailable");

        return domain switch
        {
            "light"        => state == "on" ? Icon("light-on") : Icon("light-off"),
            "switch"       => state == "on" ? Icon("switch-on") : Icon("switch-off"),
            "fan"          => state == "on" ? Icon("fan-on") : Icon("fan-off"),
            "scene"        => Icon("scene"),
            "script"       => Icon("script"),
            "button"       => Icon("button"),
            "climate"      => state == "off" ? Icon("climate-off") : Icon("climate"),
            "cover"        => state == "open" ? Icon("cover-open") : Icon("cover-closed"),
            "media_player" => state == "playing" ? Icon("media-playing") : Icon("media-idle"),
            "automation"   => state == "on" ? Icon("automation-on") : Icon("automation-off"),
            "lock"         => state == "locked" ? Icon("lock-locked") : Icon("lock-open"),
            "vacuum"       => state is "cleaning" or "on" ? Icon("vacuum-on") : Icon("vacuum-off"),
            "input_boolean" => state == "on" ? Icon("toggle-on") : Icon("toggle-off"),
            _              => state == "on" ? Icon("switch-on") : Icon("switch-off"),
        };
    }

    public static IconInfo GetDomainDefaultIcon(string domain)
    {
        return domain switch
        {
            "light"        => Icon("light-off"),
            "switch"       => Icon("switch-off"),
            "scene"        => Icon("scene"),
            "climate"      => Icon("climate"),
            "cover"        => Icon("cover-closed"),
            "fan"          => Icon("fan-off"),
            "media_player" => Icon("media-idle"),
            "automation"   => Icon("automation-off"),
            "script"       => Icon("script"),
            "lock"         => Icon("lock-locked"),
            "vacuum"       => Icon("vacuum-off"),
            "input_boolean" => Icon("toggle-off"),
            "button"       => Icon("button"),
            _              => Icon("home"),
        };
    }

    private static IconInfo Icon(string name)
        => IconHelpers.FromRelativePath($"Assets\\icons\\{name}.svg");
}

// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CmdPalHaExtension.Models;

public sealed class HaEntityState
{
    public string EntityId { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public Dictionary<string, JsonElement> Attributes { get; set; } = [];

    public string LastChanged { get; set; } = string.Empty;

    public string GetFriendlyName()
    {
        if (Attributes.TryGetValue("friendly_name", out var fn) && fn.ValueKind == JsonValueKind.String)
        {
            return fn.GetString() ?? EntityId;
        }

        return EntityId;
    }

    public string GetDomain()
    {
        var idx = EntityId.IndexOf('.');
        return idx > 0 ? EntityId[..idx] : EntityId;
    }

    public string? GetIcon()
    {
        if (Attributes.TryGetValue("icon", out var icon) && icon.ValueKind == JsonValueKind.String)
        {
            return icon.GetString();
        }

        return null;
    }
}

public sealed class HaApiCheckResponse
{
    public string Message { get; set; } = string.Empty;
}

[JsonSerializable(typeof(HaEntityState))]
[JsonSerializable(typeof(HaEntityState[]))]
[JsonSerializable(typeof(HaApiCheckResponse))]
[JsonSerializable(typeof(Dictionary<string, object>))]
[JsonSerializable(typeof(string[]))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = false,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class HaJsonContext : JsonSerializerContext
{
}

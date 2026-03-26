// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CmdPalHaExtension.Models;

namespace CmdPalHaExtension.Services;

internal enum ConnectionStatus
{
    NotConfigured,
    Connected,
    AuthFailed,
    ConnectionFailed,
}

internal static class HaClient
{
    private static HttpClient? _client;
    private static readonly Lock _lock = new();

    public static ConnectionStatus Status { get; private set; } = ConnectionStatus.NotConfigured;

    public static void Configure(string baseUrl, string token)
    {
        lock (_lock)
        {
            _client?.Dispose();
            var url = baseUrl.TrimEnd('/') + "/";
            _client = new HttpClient
            {
                BaseAddress = new Uri(url),
                Timeout = TimeSpan.FromSeconds(10),
            };
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Status = ConnectionStatus.NotConfigured;
        }
    }

    public static async Task<bool> TestConnectionAsync()
    {
        try
        {
            var client = GetClient();
            if (client == null)
            {
                return false;
            }

            var response = await client.GetAsync("api/").ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Status = ConnectionStatus.AuthFailed;
                return false;
            }

            response.EnsureSuccessStatusCode();
            Status = ConnectionStatus.Connected;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HA connection test failed: {ex.Message}");
            Status = ConnectionStatus.ConnectionFailed;
            return false;
        }
    }

    public static async Task<HaEntityState[]?> GetStatesAsync()
    {
        try
        {
            var client = GetClient();
            if (client == null)
            {
                return null;
            }

            var response = await client.GetAsync("api/states").ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Status = ConnectionStatus.AuthFailed;
                return null;
            }

            response.EnsureSuccessStatusCode();
            Status = ConnectionStatus.Connected;

            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync(stream, HaJsonContext.Default.HaEntityStateArray).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HA GetStates failed: {ex.Message}");
            Status = ConnectionStatus.ConnectionFailed;
            return null;
        }
    }

    public static async Task<bool> CallServiceAsync(string domain, string service, string entityId, Dictionary<string, object>? extraData = null)
    {
        try
        {
            var client = GetClient();
            if (client == null)
            {
                return false;
            }

            var payload = new Dictionary<string, object> { ["entity_id"] = entityId };
            if (extraData != null)
            {
                foreach (var kvp in extraData)
                {
                    payload[kvp.Key] = kvp.Value;
                }
            }

            var json = JsonSerializer.Serialize(payload, HaJsonContext.Default.DictionaryStringObject);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"api/services/{domain}/{service}", content).ConfigureAwait(false);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Status = ConnectionStatus.AuthFailed;
                return false;
            }

            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"HA CallService {domain}/{service} failed: {ex.Message}");
            return false;
        }
    }

    private static HttpClient? GetClient()
    {
        lock (_lock)
        {
            return _client;
        }
    }
}

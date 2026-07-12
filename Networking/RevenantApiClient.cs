using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Revenants.Properties;

namespace Revenants.Networking;

public static class RevenantApiClient
{
    //private const string BaseUrl = "https://localhost:7129/api/rev"; // BAD URL TO TEST NO CONNECTION
    //private const string BaseUrl = "https://localhost:7129/api/revenants";
    private const string BaseUrl = "https://app.neeshka-s-hoard.eu/api/revenants";

    public static IEnumerator SendRevenant(RevenantData data, Action<bool> onDone = null)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto // saves type info for abstract/derived classes
        };
        string json = JsonConvert.SerializeObject(data, Formatting.None, settings);
        RevenantDto dto = new RevenantDto
        {
            Name = data.PlayerName,
            LevelIdOfDeath = data.LevelIdOfDeath,
            TimeOfDeath = data.TimeOfDeath,
            Platform = data.Platform,
            GameMode = data.GameMode,
            DataJson = json
        };

        string dtoJson = JsonConvert.SerializeObject(dto);
        //Snippet.DebugLog(dtoJson);
        // IMPORTANT: convert DTO -> RevenantData
        UnityWebRequest req = new UnityWebRequest(BaseUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(dtoJson);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("PlayerName", ApiContext.PlayerName);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
        {
            Snippet.DebugLog(req.error, "red", debugType: Snippet.DebugType.Error);
            onDone?.Invoke(false);
            yield break;
        }

        onDone?.Invoke(true);
        //else
        //{
        //    Snippet.DebugLog($"{req.responseCode} - {req.downloadHandler.text}", "lime");
        //}
    }

    public static IEnumerator GetAllRevenants(Action<List<RevenantDto>> onDone)
    {
        string url = BaseUrl + "/" + "all";
        UnityWebRequest req = UnityWebRequest.Get(url);
        req.SetRequestHeader("PlayerName", ApiContext.PlayerName);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Snippet.DebugLog($"{req.error}", "red", debugType: Snippet.DebugType.Error);
            onDone?.Invoke(new List<RevenantDto>());
            yield break;
        }

        List<RevenantDto> result = null;

        try
        {
            result = JsonConvert.DeserializeObject<List<RevenantDto>>(req.downloadHandler.text);
        }
        catch (Exception e)
        {
            Snippet.DebugLog($"DTO list deserialize failed: {e}", debugType: Snippet.DebugType.Error);
        }

        onDone?.Invoke(result ?? new List<RevenantDto>());
    }

    public static IEnumerator GetRevenants(string levelId, Action<RevenantQueryResult> onDone)
    {
        string url = BaseUrl + "/" + levelId;
        UnityWebRequest req = UnityWebRequest.Get(url);
        // Send player id from header to the server
        req.SetRequestHeader("PlayerName", ApiContext.PlayerName);
        yield return req.SendWebRequest();
        if (req.result != UnityWebRequest.Result.Success)
        {
            Snippet.DebugLog($"{req.error}", "red", debugType: Snippet.DebugType.Error);
            onDone?.Invoke(new RevenantQueryResult
            {
                Success = false,
                Revenants = new List<RevenantDto>()
            });
            yield break;
        }

        //Snippet.DebugLog($"RAW RESPONSE: {json}");
        List<RevenantDto> result = null;

        try
        {
            result = JsonConvert.DeserializeObject<List<RevenantDto>>(req.downloadHandler.text);
        }
        catch (Exception e)
        {
            Snippet.DebugLog($"DTO list deserialize failed: {e}", debugType: Snippet.DebugType.Error);
        }

        onDone?.Invoke(new RevenantQueryResult
        {
            Success = true,
            Revenants = result ?? new List<RevenantDto>()
        });
    }
}
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Revenants.Networking;
using ThunderRoad;

namespace Revenants.Properties;

public static class Save_Load
{
    // Upload all local JSON files to the server
    public static void JsonToDatabase(string path)
    {
        string dataFolder = Path.Combine(path, "RevenantsData");

        if (!Directory.Exists(dataFolder))
        {
            Snippet.DebugLog("No local data folder found.", "yellow");
            return;
        }

        string[] files = Directory.GetFiles(dataFolder, "*.json");
        int total = 0;

        foreach (string file in files)
        {
            string levelId = Path.GetFileNameWithoutExtension(file).Replace("Revenants_", "");
            List<RevenantData> history = LoadHistoryFromJson(path, levelId);

            foreach (RevenantData data in history)
            {
                GameManager.local.StartCoroutine(
                    RevenantApiClient.SendRevenant(data, success =>
                    {
                        if (!success)
                            Snippet.DebugLog($"Failed to upload {data.PlayerName}", "yellow");
                    })
                );
            }

            total += history.Count;
        }

        Snippet.DebugLog($"Uploaded {total} revenants from local to server.", "green");
    }

    // Download all server data and save to local JSON files
    public static void DatabaseToJson(string path)
    {
        GameManager.local.StartCoroutine(FetchAllToJson(path));
    }

    private static IEnumerator FetchAllToJson(string path)
    {
        bool done = false;
        int total = 0;
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = { new RevenantDataConverter() }
        };
        // Start the request to fetch all revenants
        yield return GameManager.local.StartCoroutine(
            RevenantApiClient.GetAllRevenants(all =>
            {
                // Group by level
                IEnumerable<IGrouping<string, RevenantDto>> grouped = all.GroupBy(r => r.LevelIdOfDeath);

                foreach (IGrouping<string, RevenantDto> group in grouped)
                {
                    string levelId = group.Key;
                    List<RevenantData> local = LoadHistoryFromJson(path, levelId);

                    foreach (RevenantDto dto in group)
                    {
                        string json = dto.DataJson;

                        // Sometimes the JSON is quoted twice
                        if (json.StartsWith("\"") && json.EndsWith("\""))
                            json = JsonConvert.DeserializeObject<string>(json);

                        RevenantData data = JsonConvert.DeserializeObject<RevenantData>(json, settings);
                        if (data == null) continue;
                        if (local.Any(r =>
                                r.PlayerName == data.PlayerName &&
                                r.LevelIdOfDeath == data.LevelIdOfDeath &&
                                r.PositionOfDeathX == data.PositionOfDeathX &&
                                r.PositionOfDeathY == data.PositionOfDeathY &&
                                r.PositionOfDeathZ == data.PositionOfDeathZ)) continue;
                        local.Add(data);
                        total++;
                    }

                    SaveHistoryToJson(path, levelId, local);
                }

                done = true;
            })
        );

        while (!done)
            yield return null;

        Snippet.DebugLog($"Downloaded {total} revenants in one batch.", "green");
    }

    public static List<RevenantData> LoadHistoryFromJson(string path, string levelId)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto // saves type info for abstract/derived classes
        };
        string filePath = Path.Combine($"{path}", "RevenantsData", $"Revenants_{levelId}.json");
        if (!File.Exists(filePath))
            return new List<RevenantData>();
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<List<RevenantData>>(json, settings) ?? new List<RevenantData>();
    }

    public static void SaveHistoryToJson(string path, string levelId, List<RevenantData> data)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto // saves type info for abstract/derived classes
        };
        string filePath = Path.Combine($"{path}", "RevenantsData", $"Revenants_{levelId}.json");
        string json = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        File.WriteAllText(filePath, json);
    }

    public static RevenantPlayerSave LoadPlayerSave(string path)
    {
        string filePath = Path.Combine($"{path}", $"PlayerRevenant.json");
        if (!File.Exists(filePath))
            return new RevenantPlayerSave();
        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<RevenantPlayerSave>(json) ?? new RevenantPlayerSave();
    }

    public static void SavePlayerSave(string path, RevenantPlayerSave data)
    {
        string filePath = Path.Combine($"{path}", $"PlayerRevenant.json");
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
}
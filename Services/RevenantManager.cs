using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Revenants.Helpers;
using Revenants.Networking;
using Revenants.Options;
using Revenants.Properties;
using ThunderRoad;
using UnityEngine;
using QualityLevel = ThunderRoad.QualityLevel;
using Random = System.Random;

namespace Revenants.Services;

public class RevenantManager
{
    private readonly ModManager.ModData _modData;
    private readonly InventorySnapshot _inventorySnapshot = new InventorySnapshot();
    private readonly RevenantFactory _revenantFactory = new RevenantFactory();
    private readonly RevenantSpawner _revenantSpawner = new RevenantSpawner();
    private readonly RevenantLevelManager _revenantLevelManager = new RevenantLevelManager();
    private List<ContainerContent> _playerContainerContent = new List<ContainerContent>();
    private HashSet<string> _loadedAreas;
    private static readonly Random _random = new Random();
    
    public RevenantManager(ModManager.ModData modData)
    {
        _modData = modData;
    }
    
    public void OnLevelLoad()
    {
        _revenantLevelManager.AreaUnculled -= RevenantLevelManagerOnAreaUnculled;
        //_revenantLevelManager.AreaUnHidden -= RevenantLevelManagerOnAreaUnHidden;
        _revenantLevelManager.AreaUnculled += RevenantLevelManagerOnAreaUnculled;
        //_revenantLevelManager.AreaUnHidden += RevenantLevelManagerOnAreaUnHidden;
        _revenantLevelManager.SubscribeToAreas();
        NavMeshHelper.ClearCache();
        if (!Level.IsDungeon) return;
        if (!AreaManager.Instance) return;
        if (Level.current.gameObject.GetComponent<RevenantLevelSeedManager>() is RevenantLevelSeedManager seedManager && seedManager != null)
            seedManager.Release();
        else
            Level.current.gameObject.AddComponent<RevenantLevelSeedManager>().Init(Level.seed, this);
    }
    
    public void OnLevelUnload()
    {
        _revenantLevelManager.AreaUnculled -= RevenantLevelManagerOnAreaUnculled;
        //_revenantLevelManager.AreaUnHidden -= RevenantLevelManagerOnAreaUnHidden;
        _revenantLevelManager.UnsubscribeFromAreas();
        NavMeshHelper.ClearCache();
        if (Level.current.gameObject.GetComponent<RevenantLevelSeedManager>() is RevenantLevelSeedManager seedManager && seedManager != null)
            seedManager.Release();
    }
    
    public void ResetLevelManager()
    {
        OnLevelUnload();
        OnLevelLoad();
    }
    
    private void RevenantLevelManagerOnAreaUnculled(SpawnableArea area)
    {
        //Snippet.DebugLog($"Spawning for {area.AreaDataId}", "lime");
        if (!ModOptions.EnableMod) return;
        RequestArea(area.AreaDataId);
    }
    
    private void RevenantLevelManagerOnAreaUnHidden(SpawnableArea area)
    {
        //Snippet.DebugLog($"Spawning for {area.AreaDataId}", "lime");
        //RequestArea(area.AreaDataId);
    }
    
    public void OnPlayerPossess()
    {
        _loadedAreas = new HashSet<string>();
        _playerContainerContent = _inventorySnapshot.Capture(Player.local.creature);
        List<CatalogData> list = Catalog.GetDataList(Category.GameMode);
        foreach (CatalogData catalogData in list)
        {
            //Snippet.DebugLog($"Found GameMode : {catalogData.id}", "lime");
        }
        
        // Spawn Revenants at possession ! (for dungeons, it's only the unculled areas)
        if (!ModOptions.EnableMod) return;
        LoadData();
    }
    
    public void OnPlayerUnPossess()
    {
        _loadedAreas.Clear();
    }
    
    public void OnPlayerDeath()
    {
        if (!ModOptions.EnableMod) return;
        SaveData();
    }
    
    public void SaveData()
    {
        Creature creature = Player.local.creature;
        RevenantData data = _revenantFactory.Create(creature, ApiContext.PlayerName, _playerContainerContent);
        SaveDataToServer(data);
        SaveDataLocally(data);
    }
    
    private void SaveDataLocally(RevenantData data)
    {
        List<RevenantData> history = Save_Load.LoadHistoryFromJson(_modData.fullPath, data.LevelIdOfDeath);
        history.Add(data);
        Save_Load.SaveHistoryToJson(_modData.fullPath, data.LevelIdOfDeath, history);
    }
    
    private void SaveDataToServer(RevenantData data)
    {
        GameManager.local.StartCoroutine(
            RevenantApiClient.SendRevenant(data, success =>
            {
                if (!success)
                    Snippet.DebugLog("Server save failed", "yellow", Snippet.DebugType.Warning);
            }));
    }
    
    public void LoadData()
    {
        if (GameModeManager.instance.currentGameMode.id != ModOptions.GameModeOption &&
            ModOptions.GameModeOption != "Any") return;
        // Filter revenants at home
        if (!Level.IsDungeon && Level.current.data.id == "Home" && !ModOptions.AllowAtHome) return;
        //LoadDataLocally();
        LoadInitialAreas();
    }
    
    private bool IsAllowed(ModOptions.LevelTypeOptions option)
    {
        return option == ModOptions.LevelTypeOptions.Any ||
               (Level.IsDungeon && option == ModOptions.LevelTypeOptions.Dungeon) ||
               (!Level.IsDungeon && option == ModOptions.LevelTypeOptions.Arena);
    }
    
    private void LoadInitialAreas()
    {
        ModOptions.LevelTypeOptions option = ModOptions.LevelTypeOption;
        if (!IsAllowed(option))
            return;
        
        if (!Level.IsDungeon)
        {
            RequestArea(Level.current.data.id);
            return;
        }
        
        foreach (SpawnableArea area in AreaManager.Instance.CurrentTree)
        {
            if (!area.SpawnedArea.isCulled)
                //if (!area.SpawnedArea.isHidden)
            {
                RequestArea(area.AreaDataId);
            }
        }
    }
    
    private void RequestArea(string areaId)
    {
        if (!_loadedAreas.Add(areaId))
            return;
        LoadAreaFromServer(areaId);
        //TimeManager.Pause(true);
    }
    
    private (int min, int max) GetSpawnRange(string areaId)
    {
        int baseMin = ModOptions.MinNumberOfRevenantsPerRoom;
        int baseMax = ModOptions.MaxNumberOfRevenantsPerRoom;
        if (!Level.IsDungeon || !LevelLocationHelper.TryGetAreaBounds(areaId, out Bounds bounds))
            return (baseMin, baseMax);
        float walkableArea = NavMeshHelper.GetWalkableArea(areaId, bounds);
        // 100m² = tiny room
        // 50000m² = huge arena
        float t;
        const float areaThreshold = 1000f;
        if (walkableArea < areaThreshold)
        {
            // Small rooms : very smooth and conservative
            t = Mathf.InverseLerp(50f, areaThreshold, walkableArea);
            t = Mathf.Pow(t, 1.5f) * 0.35f;
        }
        else
        {
            // Big areas : accelerate strongly
            t = Mathf.InverseLerp(areaThreshold, 50000f, walkableArea);
            t = 0.35f + Mathf.Pow(t, 0.5f) * 0.65f;
        }
        
        int min = Mathf.Max(1, Mathf.RoundToInt(baseMin * Mathf.Lerp(1.0f, 4.0f, t)));
        int max = Mathf.Max(min, Mathf.RoundToInt(baseMax * Mathf.Lerp(1.0f, 7.5f, t)));
        max = Mathf.Min(max, QualityLevel.Android == Common.GetQualityLevel() ? 25 : 50);
        return (min, max);
    }
    
    private List<T> PickRandomSubset<T>(List<T> source, int min, int max)
    {
        if (source == null || source.Count == 0)
            return new List<T>();
        
        // Make sure the min and max don't throw impossible datas
        if (max < min)
            (min, max) = (max, min);
        
        int numberToGet = _random.Next(min, max + 1);
        numberToGet = Math.Min(numberToGet, source.Count);
        List<T> copy = new List<T>(source);
        
        // Fisher-Yates shuffle
        for (int i = copy.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (copy[i], copy[j]) = (copy[j], copy[i]);
        }
        
        return copy.Take(numberToGet).ToList();
    }
    
    private void LoadAreaLocally(string areaId)
    {
        // Load the player's history
        List<RevenantData> history = Save_Load.LoadHistoryFromJson(_modData.fullPath, areaId);
        //Snippet.DebugLog($"History count : {history.Count}", "orange");
        if (history.Count <= 0)
        {
            Snippet.DebugLog($"No data for : {areaId}", "red");
            return;
        }
        
        (int min, int max) = GetSpawnRange(areaId);
        
        List<RevenantData> selected = PickRandomSubset(
            history,
            min,
            max
        );
        SpawnRevenants(selected);
    }
    
    private void LoadAreaFromServer(string areaId)
    {
        GameManager.local.StartCoroutine(
            RevenantApiClient.GetRevenants(areaId, (result) =>
            {
                if (!result.Success)
                {
                    Snippet.DebugLog("Server unavailable, loading local history", "yellow", Snippet.DebugType.Warning);
                    LoadAreaLocally(areaId);
                    return;
                }
                
                Snippet.DebugLog($"Server history count: {result.Revenants.Count}", "orange");
                
                if (result.Revenants.Count == 0)
                {
                    Snippet.DebugLog($"No data for: {areaId}", "red");
                    return;
                }
                
                // Convert DTO → Data
                List<RevenantData> dataList = new List<RevenantData>();
                
                foreach (RevenantDto dto in result.Revenants)
                {
                    RevenantData data = Deserialize(dto);
                    
                    if (data != null)
                        dataList.Add(data);
                }
                
                if (dataList.Count == 0)
                {
                    Snippet.DebugLog($"No valid revenant data for: {areaId}", "red");
                    return;
                }
                
                // Pick random subset
                (int min, int max) = GetSpawnRange(areaId);
                
                List<RevenantData> selected = PickRandomSubset(
                    dataList,
                    min,
                    max
                );
                Snippet.DebugLog($"Spawning {selected.Count} revenants for {areaId}", "cyan");
                
                SpawnRevenants(selected);
            })
        );
    }
    
    private void SpawnRevenants(IEnumerable<RevenantData> data)
    {
        foreach (RevenantData d in data)
            _revenantSpawner.SpawnRevenant(d);
    }
    
    private RevenantData Deserialize(RevenantDto dto)
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = { new RevenantDataConverter() }
        };
        if (string.IsNullOrWhiteSpace(dto.DataJson))
            return null;
        
        string cleanJson = dto.DataJson;
        
        if (cleanJson.StartsWith("\"") && cleanJson.EndsWith("\""))
            cleanJson = JsonConvert.DeserializeObject<string>(cleanJson);
        
        try
        {
            return JsonConvert.DeserializeObject<RevenantData>(cleanJson, settings);
        }
        catch
        {
            return null;
        }
    }
}
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace Revenants.Helpers;

public static class LevelLocationHelper
{
    private static Vector3 ToAreaSpace(SpawnableArea area, Vector3 worldPosition)
    {
        Quaternion rotation = AreaRotationHelper.GetRotationQuaternionFromRotation(area.Rotation);
        
        return Quaternion.Inverse(rotation) * (worldPosition - area.Position);
    }
    
    private static Vector3 ToWorldSpace(SpawnableArea area, Vector3 localPosition)
    {
        Quaternion rotation = AreaRotationHelper.GetRotationQuaternionFromRotation(area.Rotation);
        
        return rotation * localPosition + area.Position;
    }
    
    public static Vector3 LocationInLevelToSave()
    {
        Vector3 location = Vector3.zero;
        if (Level.IsDungeon)
        {
            if (AreaManager.Instance)
            {
                location = ToAreaSpace(AreaManager.Instance.CurrentArea, Player.local.creature.transform.position);
            }
        }
        else
        {
            location = Player.local.creature.transform.position;
        }
        
        return location;
    }
    
    public static SpawnableArea GetArea(string areaId)
    {
        if (!Level.IsDungeon || !AreaManager.Instance)
            return null;
        
        foreach (SpawnableArea area in AreaManager.Instance.CurrentTree)
        {
            if (area.AreaDataId == areaId)
                return area;
        }
        
        return null;
    }
    // Outdated for some stuff !
    public static int GetFactionIDOfDungeon()
    {
        if (!Level.IsDungeon || !AreaManager.Instance)
            return 3;
        AreaManager.Instance.TryGetArea(0, out SpawnableArea area);
        if (AreaManager.Instance.CurrentTree.Count > 0 &&
            area.SpawnedArea.creatures.Count > 0)
        {
            //Snippet.DebugLog($"Area name : {area.AreaDataId}", "red");
            return area.SpawnedArea.creatures[0].factionId;
        }
        
        return 3;
    }
    
    public static int GetFactionFromArea(string areaId)
    {
        if (!Level.IsDungeon || !AreaManager.Instance)
            return 3;
        foreach (SpawnableArea area in AreaManager.Instance.CurrentTree)
        {
            SpawnableArea areaFromId = GetArea(areaId);
            if (areaFromId == null)
                return 3;
            if (areaFromId.AreaDataId != areaId) continue;
            //Snippet.DebugLog($"Area name : {area.AreaDataId}", "red");
            return area.SpawnedArea.creatures[0].factionId;
        }
        return 3;
    }
    
    
    
    public static Vector3 LocationInWorld(string areaId, Vector3 localPosition)
    {
        if (!Level.IsDungeon)
            return localPosition;
        
        SpawnableArea area = GetArea(areaId);
        
        if (area == null)
            return localPosition;
        
        return ToWorldSpace(area, localPosition);
    }
    
    public static string LevelID()
    {
        string levelId = Level.current.data.id;
        if (!Level.IsDungeon) return levelId;
        if (AreaManager.Instance)
        {
            levelId = AreaManager.Instance.CurrentArea.AreaDataId;
        }
        
        return levelId;
    }
    
    public static bool TryGetAreaBounds(string areaId, out Bounds bounds)
    {
        bounds = default;
        
        if (!Level.IsDungeon) return false;
        SpawnableArea area = GetArea(areaId);
        
        if (area == null)
            return false;
        
        bounds = area.Bounds;
        return true;
    }
}
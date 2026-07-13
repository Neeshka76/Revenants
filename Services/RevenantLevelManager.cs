using System;
using System.Collections.Generic;
using Revenants.Helpers;
using ThunderRoad;

namespace Revenants.Services;

public class RevenantLevelManager
{
    //public event Action<SpawnableArea> AreaUnHidden;
    //private Dictionary<SpawnableArea, bool> _dictAreaHidden;
    //private readonly HashSet<SpawnableArea> _unHiddenAlreadyTriggered = new HashSet<SpawnableArea>();
    public event Action<SpawnableArea> AreaUnculled;
    private Dictionary<SpawnableArea, bool> _dictAreaCulled;
    private readonly HashSet<SpawnableArea> _unCulledAlreadyTriggered = new HashSet<SpawnableArea>();
    private int _seed;
    
    public void SubscribeToAreas()
    {
        if (!Level.IsDungeon) return;
        if (!AreaManager.Instance) return;
        _seed = Level.seed;
        string listOfAreasString = "";
        _dictAreaCulled = new Dictionary<SpawnableArea, bool>();
        //_dictAreaHidden = new Dictionary<SpawnableArea, bool>();
        foreach (SpawnableArea area in AreaManager.Instance.CurrentTree)
        {
            listOfAreasString +=
                $"{area.AreaDataId} , IsCulled : {area.SpawnedArea.isCulled}, IsHidden : {area.SpawnedArea.isHidden} with a size of {area.Bounds.size} (center{area.Bounds.center} / {NavMeshHelper.GetWalkableArea(area.AreaDataId, area.Bounds, false)}\n";
            area.SpawnedArea.onCullChange -= SpawnedAreaOnonCullChange;
            //area.SpawnedArea.onHideChange -= SpawnedAreaOnonHideChange;
            area.SpawnedArea.onCullChange += SpawnedAreaOnonCullChange;
            //area.SpawnedArea.onHideChange += SpawnedAreaOnonHideChange;
            _dictAreaCulled.Add(area, area.SpawnedArea.isCulled);
            //_dictAreaHidden.Add(area, area.SpawnedArea.isHidden);
        }
        
        Snippet.DebugLog($"Tree of Areas (seed : {_seed}) : \n{listOfAreasString}", "cyan");
    }
    
    /*private void SpawnedAreaOnonHideChange(bool isHide)
    {
        SpawnableArea areaChanged = null;
        foreach (SpawnableArea area in AreaManager.Instance.CurrentTree)
        {
            if (_dictAreaHidden[area] == area.SpawnedArea.isHidden) continue;
            areaChanged = area;
            _dictAreaHidden[area] = area.SpawnedArea.isHidden;
            break;
        }
    
        if (!isHide && areaChanged != null &&  !_unHiddenAlreadyTriggered.Contains(areaChanged))
        {
            _unHiddenAlreadyTriggered.Add(areaChanged);
            //Snippet.DebugLog($"Area UnHidden : {areaChanged.AreaDataId}");
            AreaUnHidden?.Invoke(areaChanged);
        }
    }*/
    
    private void SpawnedAreaOnonCullChange(bool isCulled)
    {
        SpawnableArea areaChanged = null;
        foreach (SpawnableArea area in AreaManager.Instance.CurrentTree)
        {
            if (_dictAreaCulled[area] == area.SpawnedArea.isCulled) continue;
            areaChanged = area;
            _dictAreaCulled[area] = area.SpawnedArea.isCulled;
            break;
        }
        
        if (!isCulled && areaChanged != null && !_unCulledAlreadyTriggered.Contains(areaChanged))
        {
            _unCulledAlreadyTriggered.Add(areaChanged);
            //Snippet.DebugLog($"Area Unculled : {areaChanged.AreaDataId}");
            AreaUnculled?.Invoke(areaChanged);
        }
    }
    
    public void UnsubscribeFromAreas()
    {
        if (!Level.IsDungeon) return;
        if (!AreaManager.Instance) return;
        _dictAreaCulled?.Clear();
        //_dictAreaHidden?.Clear();
        _unCulledAlreadyTriggered?.Clear();
        //_unHiddenAlreadyTriggered?.Clear();
        foreach (SpawnableArea area in AreaManager.Instance.CurrentTree)
        {
            area?.SpawnedArea?.onCullChange -= SpawnedAreaOnonCullChange;
            //area.SpawnedArea.onHideChange -= SpawnedAreaOnonHideChange;
        }
    }
}
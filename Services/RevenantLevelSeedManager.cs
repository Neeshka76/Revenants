using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace Revenants.Services;

public class RevenantLevelSeedManager : ThunderBehaviour
{
    public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;
    private int _seed;
    private RevenantManager _revenantManager;
    
    public void Init(int seed, RevenantManager revenantManager)
    {
        _seed = seed;
        _revenantManager = revenantManager;
    }
    
    public void Release()
    {
        _revenantManager = null;
        _seed = 0;
        Destroy(this);
    }
    
    protected override void ManagedUpdate()
    {
        if (!Level.IsDungeon) return;
        if (!AreaManager.Instance) return;
        if (_seed != Level.seed)
        {
            //Snippet.DebugLog($"Found a new seed ! {_seed} to {Level.seed}; resetting !", "cyan");
            GameManager._local.StartCoroutine(IEResetManager());
            _seed = Level.seed;
        }
        
        base.ManagedUpdate();
    }
    
    IEnumerator IEResetManager()
    {
        yield return new WaitForSecondsRealtime(3f);
        _revenantManager.ResetLevelManager();
    }
}
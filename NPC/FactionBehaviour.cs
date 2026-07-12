using System.Collections;
using System.Collections.Generic;
using Revenants.Helpers;
using Revenants.Options;
using ThunderRoad;
using UnityEngine;

namespace Revenants.NPC;

public class FactionBehaviour : ThunderBehaviour
{
    public override ManagedLoops EnabledManagedLoops => ManagedLoops.Update;
    private Creature _creature;
    private bool _fightEveryone;
    private int _originalFactionId;
    private string _areaId;

    // Set the faction id on init of the behaviour
    public void Init(Creature creature, string areaId)
    {
        _creature = creature;
        _creature.OnDespawnEvent -= CreatureOnOnDespawnEvent;
        _creature.OnDespawnEvent += CreatureOnOnDespawnEvent;
        _originalFactionId = _creature.factionId;
        _areaId = areaId;
        if (!Level.IsDungeon) return;
        _fightEveryone = ModOptions.RevenantsFightAgainstEveryone;
        if (_fightEveryone) return;
        //_creature.factionId = LevelLocationHelper.GetFactionIDOfDungeon();
        _creature.factionId = LevelLocationHelper.GetFactionFromArea(_areaId);
    }

    // Check if the mod option is changing, if it is, update the faction id accordingly (by default, it's set to fight against everyone)
    protected override void ManagedUpdate()
    {
        base.ManagedUpdate();
        if (!Level.IsDungeon) return;
        if (_fightEveryone == ModOptions.RevenantsFightAgainstEveryone) return;
        _fightEveryone = ModOptions.RevenantsFightAgainstEveryone;
        StartCoroutine(IEChangeFaction());
    }

    private IEnumerator IEChangeFaction()
    {
        //Snippet.DebugLog($"FactionId BEFORE : {LevelLocationHelper.GetFactionIDOfDungeon()} / {cr.factionId} for {data.LevelIdOfDeath}", "magenta");
        //_creature.factionId = _fightEveryone ? _originalFactionId : LevelLocationHelper.GetFactionIDOfDungeon();
        _creature.factionId = _fightEveryone ? _originalFactionId : LevelLocationHelper.GetFactionFromArea(_areaId);
        //Snippet.DebugLog($"FactionId AFTER : {LevelLocationHelper.GetFactionIDOfDungeon()} / {cr.factionId} for {data.LevelIdOfDeath}", "cyan");
        // Wait for the factionId to kick in for every revenants
        yield return new WaitForSeconds(0.01f);
        // Check if the creature is targeting a friendly, if it does, wipe the target
        foreach (Creature creature in Creature.allActive)
        {
            if (creature.brain?.currentTarget == null) continue;

            Creature target = creature.brain.currentTarget;

            if (creature.factionId == target.factionId)
            {
                creature.brain.currentTarget = null;
            }
        }
    }

    // Reset the faction id on despawn
    private void CreatureOnOnDespawnEvent(EventTime eventTime)
    {
        if (eventTime == EventTime.OnEnd) return;
        _creature.factionId = _originalFactionId;
        _creature.OnDespawnEvent -= CreatureOnOnDespawnEvent;
        Destroy(this);
    }
}
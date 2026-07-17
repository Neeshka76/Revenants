using System.Collections;
using System.Collections.Generic;
using Revenants.API;
using Revenants.Helpers;
using Revenants.NPC;
using Revenants.Options;
using Revenants.Properties;
using ThunderRoad;
using ThunderRoad.Skill;
using ThunderRoad.Skill.Spell;
using UnityEngine;

namespace Revenants.Services;

public class RevenantSpawner
{
    private readonly HolderManager _holderManager = new HolderManager();
    private readonly WardRobeManager _wardRobeManager = new WardRobeManager();
    
    public void SpawnRevenant(RevenantData data, Vector3? overridePosition = null)
    {
        Creature playerCr = Player.local.creature;
        CreatureData creatureData = Catalog.GetData<CreatureData>(data.CreatureId).Clone() as CreatureData;
        //CreatureData creatureData = data.CreatureData;
        creatureData.Load(playerCr);
        creatureData.id = data.CreatureId;
        creatureData.factionId = 3;
        creatureData.brainId = "HumanHard";
        creatureData.ethnicityId = data.EthnicityId;
        Vector3 location = overridePosition ?? LevelLocationHelper.LocationInWorld(data.LevelIdOfDeath,
            new Vector3(data.PositionOfDeathX, data.PositionOfDeathY, data.PositionOfDeathZ));
        GameManager.local.StartCoroutine(creatureData.SpawnCoroutine(location,
            Random.Range(0f, 359f), null, cr => { GameManager.local.StartCoroutine(IESpawnRevenant(data, cr)); },
            false));
    }
    
    IEnumerator IESpawnRevenant(RevenantData data, Creature cr)
    {
        cr.Hide(true);
        cr.SetColor(
            new Color(data.HairColorPrimaryR, data.HairColorPrimaryG, data.HairColorPrimaryB, data.HairColorPrimaryA),
            Creature.ColorModifier.Hair);
        cr.SetColor(
            new Color(data.HairColorSecondaryR, data.HairColorSecondaryG, data.HairColorSecondaryB,
                data.HairColorSecondaryA), Creature.ColorModifier.HairSecondary);
        cr.SetColor(
            new Color(data.HairColorSpecularR, data.HairColorSpecularG, data.HairColorSpecularB,
                data.HairColorSpecularA), Creature.ColorModifier.HairSpecular);
        //cr.SetColor(new Color(data.EyesColorIrisR, data.EyesColorIrisG, data.EyesColorIrisB, data.EyesColorIrisA), Creature.ColorModifier.EyesIris);
        cr.SetColor(new Color(1f, 0.0f, 0f, 1f), Creature.ColorModifier.EyesIris);
        cr.SetColor(
            new Color(data.EyesColorScleraR, data.EyesColorScleraG, data.EyesColorScleraB, data.EyesColorScleraA),
            Creature.ColorModifier.EyesSclera);
        cr.SetColor(new Color(data.SkinColorR, data.SkinColorG, data.SkinColorB, data.SkinColorA),
            Creature.ColorModifier.Skin);
        yield return Yielders.ForSeconds(0.3f);
        //Snippet.DebugLog($"Color of Eye Iris : {cr.GetColor(Creature.ColorModifier.EyesIris)} / Player : {Player.local.creature.GetColor(Creature.ColorModifier.EyesIris)}");
        //Snippet.DebugLog($"Color of Eye Sclera : {cr.GetColor(Creature.ColorModifier.EyesSclera)} / Player : {Player.local.creature.GetColor(Creature.ColorModifier.EyesSclera)}");
        cr.GetOrAddComponent<NameDisplay>().Init(data.PlayerName);
        _holderManager.Clear(cr);
        _wardRobeManager.Clear(cr);
        yield return Yielders.ForSeconds(0.1f);
        // Allow for culling the creature inside the dungeon
        if (Level.IsDungeon)
        {
            Area spawnedArea = LevelLocationHelper.GetArea(data.LevelIdOfDeath).SpawnedArea;
            spawnedArea.RegisterCreature(cr);
            if (!spawnedArea.isHidden)
            {
                //Snippet.DebugLog($"Unhidding {cr.name} at {spawnedArea.name}", "red");
                cr.Hide(false);
            }
        }
        else
        {
            cr.Hide(false);
        }
        
        // put the revenants at the same faction if the mod option is disabled and handle toggles
        cr.GetOrAddComponent<FactionBehaviour>().Init(cr, data.LevelIdOfDeath);
        foreach (ContainerContent content1 in data.ListContainerContent)
        {
            switch (content1)
            {
                case ItemContent itemContent:
                    //Snippet.DebugLog($"Itemcontent : {itemContent.data.id}; {itemContent.data.type}");
                    switch (itemContent.state)
                    {
                        case ContentStateWorn wornState:
                            _wardRobeManager.TryEquip(cr, itemContent);
                            break;
                        //Snippet.DebugLog($"Not a wardrobe : {itemContent.state}", "lime");
                        case ContentStateHolder holderState:
                        {
                            //Snippet.DebugLog($"ItemContent 2 : {holderState.holderName}", "black");
                            foreach (Holder holder in cr.container.linkedHolders)
                            {
                                //Snippet.DebugLog($"In here {holder.name} / {holderState.holderName}", "red");
                                if (holder.name != holderState.holderName) continue;
                                //Snippet.DebugLog($"In here 2 {holder.name} / {holderState.holderName}", "yellow");
                                _holderManager.TryEquip(cr, itemContent);
                                //Snippet.DebugLog($"In here 3 {holder.name} / {holderState.holderName}", "yellow");
                                break;
                            }
                            
                            break;
                        }
                    }
                    
                    break;
                case SkillContent skillContent:
                    // Skip SkillAirDash as it's useless for npcs
                    if (Catalog.GetData<SkillAirstrike>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillAirDash as it's useless for npcs
                    if (Catalog.GetData<SkillAirDash>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillBruteForce as it's useless for npcs
                    if (Catalog.GetData<SkillBruteForce>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillCharger (Juggernaut) as it can mess with physics ?
                    if (Catalog.GetData<SkillCharger>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip Discombobulate as it's useless for npcs
                    if (Catalog.GetData<SkillDiscombobulate>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillEmergencyExit as it's useless for npcs
                    if (Catalog.GetData<SkillEmergencyExit>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillFlykick as it's useless for npcs
                    if (Catalog.GetData<SkillFlykick>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillGrappler (Wrestler) as it's useless for npcs
                    if (Catalog.GetData<SkillGrappler>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillGravityDecapitate as it's useless for npcs but give the AI one
                    if (Catalog.GetData<SkillGravityDecapitate>(skillContent.referenceID, false) != null)
                    {
                        cr.container.AddSkillContent(Catalog.GetData<SkillData>("ImpulseSpikeNPC"));
                        continue;
                    }
                    
                    // Skip SkillImprovedStealth as it's useless for npcs
                    if (Catalog.GetData<SkillImprovedStealth>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillIntimidation as it's useless for npcs but make them immune to fear
                    if (Catalog.GetData<SkillIntimidation>(skillContent.referenceID, false) != null)
                    {
                        BrainModuleFear fearModule = cr.brain.instance.GetModule<BrainModuleFear>(false);
                        fearModule.cowerDuration = -1f;
                        continue;
                    }
                    
                    // Skip SkillMightyKick as it's useless for npcs
                    if (Catalog.GetData<SkillMightyKick>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillPrecisionFocus as they spam errors for AI as they try to use it ? On ManagedUpdate, looks like it add the air helper and the game doesn't like this
                    if (Catalog.GetData<SkillPrecisionFocus>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillRicochetThrow (Boomerang) as it's useless for npcs
                    if (Catalog.GetData<SkillRicochetThrow>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillRiptide as they try to use it ? On FlowControl
                    if (Catalog.GetData<SkillRiptide>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip SkillSecondWind as it's useless for npcs but give the AI one
                    if (Catalog.GetData<SkillSecondWind>(skillContent.referenceID, false) != null)
                    {
                        cr.container.AddSkillContent(Catalog.GetData<SkillData>("SecondWindNPC"));
                        continue;
                    }
                    
                    // Skip grip cast as it's useless for npcs
                    if (Catalog.GetData<GripCastSkillData>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip spell punches as they spam errors for AI as they try to use it/select it
                    if (Catalog.GetData<SkillSpellPunch>(skillContent.referenceID, false) != null)
                        continue;
                    // Skip data unknown
                    if (skillContent.data == null)
                        continue;
                    cr.container.AddSkillContent(skillContent.data);
                    break;
                case SpellContent spellContent:
                    // Skip data unknown
                    if (spellContent.data == null)
                        continue;
                    cr.container.AddSpellContent(spellContent.data);
                    break;
            }
        }
        
        SkillManager skillManager = new SkillManager(cr);
        RevenantApi.RaiseRevenantSpawned(cr);
        //TimeManager.Pause(true);
    }
}
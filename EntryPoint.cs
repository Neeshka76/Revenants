using IngameDebugConsole;
using Revenants.Helpers;
using Revenants.Properties;
using Revenants.Services;
using ThunderRoad;
using Oculus.Platform;
using Revenants.API;
using UnityEngine;
using QualityLevel = ThunderRoad.QualityLevel;

namespace Revenants;

public class EntryPoint : ThunderScript
{
    private RevenantManager _revenantManager;
    private RevenantPlayerSave _revenantPlayerSave;
    
    public override void ScriptEnable()
    {
        base.ScriptEnable();
        EventManager.onPossess -= EventManagerOnonPossess;
        EventManager.onUnpossess -= EventManagerOnonUnpossess;
        EventManager.onCreatureKill -= EventManagerOnonCreatureKill;
        EventManager.onLevelLoad -= EventManagerOnonLevelLoad;
        EventManager.onLevelUnload -= EventManagerOnonLevelUnload;
        EventManager.onCreatureSpawn -= EventManagerOnonCreatureSpawn;
        EventManager.onPossess += EventManagerOnonPossess;
        EventManager.onUnpossess += EventManagerOnonUnpossess;
        EventManager.onCreatureKill += EventManagerOnonCreatureKill;
        EventManager.onLevelLoad += EventManagerOnonLevelLoad;
        EventManager.onLevelUnload += EventManagerOnonLevelUnload;
        EventManager.onCreatureSpawn += EventManagerOnonCreatureSpawn;
        //DebugLogConsole.AddCommand("nee.gotolevel", "Load the Data", Debug_AreaTestLevel);
        //DebugLogConsole.AddCommand("revenants.savedata", "Save the Data", SaveDataConsole);
        //DebugLogConsole.AddCommand("revenants.loaddata", "Load the Data", LoadDataConsole);
        //DebugLogConsole.AddCommand("revenants.databasetojson", "Transfer the Data of the database to jsons", DatabaseToJsonConsole);
        //DebugLogConsole.AddCommand("revenants.jsontodatabase", "Transfer the Data of jsons to the database", JsonToDatabaseConsole);
        //DebugLogConsole.AddCommand("revenants.currentlevelid", "Current level id", CurrentLevelId);
        _revenantManager = new RevenantManager(ModData);
        _revenantPlayerSave = Save_Load.LoadPlayerSave(ModData.fullPath);
        if (string.IsNullOrWhiteSpace(_revenantPlayerSave.Name))
        {
            if (QualityLevel.Android == Common.GetQualityLevel())
            {
                GameManager.platform.store.GetUserID((success, id) =>
                {
                    if (!success)
                    {
                        _revenantPlayerSave.Name = NameGenerator.Generate();
                        Snippet.DebugLog($"Getting id failed, generating a random username {_revenantPlayerSave.Name}", "yellow", Snippet.DebugType.Warning);
                        ApiContext.PlayerName = _revenantPlayerSave.Name;
                        Save_Load.SavePlayerSave(ModData.fullPath, _revenantPlayerSave);
                        return;
                    }
                    
                    Users.Get(id).OnComplete(message =>
                    {
                        if (!message.IsError && !string.IsNullOrWhiteSpace(message.Data.DisplayName))
                        {
                            Snippet.DebugLog($"Getting username success, username found : {message.Data.DisplayName}", "lime");
                            _revenantPlayerSave.Name = message.Data.DisplayName;
                        }
                        else
                        {
                            _revenantPlayerSave.Name = NameGenerator.Generate();
                            Snippet.DebugLog($"Getting username failed, generating a random username {_revenantPlayerSave.Name}", "yellow",  Snippet.DebugType.Warning);
                        }
                        
                        ApiContext.PlayerName = _revenantPlayerSave.Name;
                        Save_Load.SavePlayerSave(ModData.fullPath, _revenantPlayerSave);
                    });
                });
            }
            else
            {
                GameManager.platform.store.GetUserName((success, name) =>
                {
                    
                    if(success)
                    {
                        _revenantPlayerSave.Name = name;
                        Snippet.DebugLog($"Getting username success, username found : {_revenantPlayerSave.Name}", "lime");
                    }
                    else
                    {
                        _revenantPlayerSave.Name = NameGenerator.Generate();
                        Snippet.DebugLog($"Getting username failed, generating a random username {_revenantPlayerSave.Name}", "yellow",  Snippet.DebugType.Warning);
                    }
                    ApiContext.PlayerName = _revenantPlayerSave.Name;
                    Save_Load.SavePlayerSave(ModData.fullPath, _revenantPlayerSave);
                });
            }
        }
        else
        {
            ApiContext.PlayerName = _revenantPlayerSave.Name;
            Snippet.DebugLog($"Getting username from save, username found : {_revenantPlayerSave.Name}", "lime");
        }
        Snippet.DebugLog($"PlayerName : {ApiContext.PlayerName}", "cyan");
        RevenantApi.RevenantManager = _revenantManager;
    }
    
    public static void Debug_AreaTestLevel(string id)
    {
        ConsoleCommands.LoadLevelMode("Debug_AreaTestLevel", GameModeManager.instance.currentGameMode.name,
            "-leveloption", $"DungeonRoom={id}");
    }
    
    private void EventManagerOnonCreatureSpawn(Creature creature)
    {
        //Snippet.DebugLog($"Spawning creature {creature.name} with :", "yellow");
        //foreach (ContainerContent content in creature.container.contents)
        //{
        //    Snippet.DebugLog($"{content.referenceID}", "yellow");
        //}
    }
    
    private void EventManagerOnonLevelUnload(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
    {
        if (eventTime == EventTime.OnEnd) return;
        _revenantManager.OnLevelUnload();
    }
    
    private void EventManagerOnonLevelLoad(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
    {
        if (eventTime == EventTime.OnStart) return;
        _revenantManager.OnLevelLoad();
    }
    
    private void EventManagerOnonPossess(Creature creature, EventTime eventTime)
    {
        if (eventTime == EventTime.OnStart) return;
        //ListWardRobes(Player.local.creature, _playerContainerContent);
        //ListAllWardRobeDataOfCatalog();
        _revenantManager.OnPlayerPossess();
        //PlayerControl.local.ToggleMenu(true, false);
    }
    
    private void ListAllWardRobeDataOfCatalog()
    {
        foreach (string id in Catalog.GetAllID(Category.Item))
        {
            ItemData itemData = Catalog.GetData<ItemData>(id);
            if (!itemData.TryGetModule(out ItemModuleWardrobe module1)) continue;
            foreach (ItemModuleWardrobe.CreatureWardrobe wardrobe in module1.wardrobes)
            {
                Snippet.DebugLog($"wardrobe : {wardrobe.manikinWardrobeData.channels.Length} for {id}", "yellow");
                foreach (string channel in wardrobe.manikinWardrobeData.channels)
                {
                    Snippet.DebugLog($"wardrobe : {channel} for {id}", "cyan");
                }
            }
        }
    }
    
    private void EventManagerOnonUnpossess(Creature creature, EventTime eventTime)
    {
        if (eventTime == EventTime.OnEnd) return;
        _revenantManager.OnPlayerUnPossess();
    }
    
    private void EventManagerOnonCreatureKill(Creature creature, Player player, CollisionInstance collisionInstance,
        EventTime eventTime)
    {
        if (eventTime == EventTime.OnEnd) return;
        if (!creature.isPlayer) return;
        // I cannot find if it comes from a zone, but if kills function is triggered by a zone (which have 99999 damage and is of the energy type), I skip the save
        //Snippet.DebugLog($"Death of player by : {collisionInstance.damageStruct.damageType}", "lime");
        if (collisionInstance.damageStruct.damageType == DamageType.Energy &&
            collisionInstance.damageStruct.damage == 99999f)
        {
            //Snippet.DebugLog($"Death of player by Energy ? : {collisionInstance.damageStruct.damageType == DamageType.Energy}", "lime");
            //Snippet.DebugLog($"Death of player by : {collisionInstance.damageStruct.damage}", "red");
            return;
        }
        
        _revenantManager.OnPlayerDeath();
    }
    
    public override void ScriptDisable()
    {
        base.ScriptDisable();
        EventManager.onPossess -= EventManagerOnonPossess;
        EventManager.onUnpossess -= EventManagerOnonUnpossess;
        EventManager.onCreatureKill -= EventManagerOnonCreatureKill;
        EventManager.onLevelLoad -= EventManagerOnonLevelLoad;
        EventManager.onLevelUnload -= EventManagerOnonLevelUnload;
        EventManager.onCreatureSpawn -= EventManagerOnonCreatureSpawn;
        DebugLogConsole.RemoveCommand("revenants.savedata");
        DebugLogConsole.RemoveCommand("revenants.loaddata");
        DebugLogConsole.RemoveCommand("revenants.databasetojson");
        DebugLogConsole.RemoveCommand("revenants.jsontodatabase");
        DebugLogConsole.RemoveCommand("revenants.currentlevelid");
        RevenantApi.RevenantManager = null;
    }
    
    public void SaveDataConsole()
    {
        Snippet.DebugLog($"Saving data to the server", "yellow");
        _revenantManager.SaveData();
        Snippet.DebugLog($"Saved data to the server", "green");
    }
    
    public void DatabaseToJsonConsole()
    {
        Snippet.DebugLog($"Saving data from the server to jsons", "yellow");
        Save_Load.DatabaseToJson(ModData.fullPath);
        Snippet.DebugLog($"Saved data from the server to jsons", "green");
    }
    
    public void CurrentLevelId()
    {
        Snippet.DebugLog($"Current Level ID : {LevelLocationHelper.LevelID()}", "magenta");
    }
    
    public void JsonToDatabaseConsole()
    {
        Snippet.DebugLog($"Loading data from the json to the server", "yellow");
        Save_Load.JsonToDatabase(ModData.fullPath);
        Snippet.DebugLog($"Loaded data from the json to the server", "green");
    }
    
    public void LoadDataConsole()
    {
        Snippet.DebugLog($"Loading data to the server", "yellow");
        _revenantManager.LoadData();
        Snippet.DebugLog($"Loaded data to the server", "green");
    }
    
    public void SpawnRandomRevenant(Vector3 position)
    {
        _revenantManager.SpawnRandomRevenant(position);
    }
    
    //public void SpawnClone()
    //{
    //    Creature playerCr = Player.local.creature;
    //    Container playerCrContainer = playerCr.container;
    //    CreatureData creatureData = Catalog.GetData<CreatureData>(playerCr.creatureId).Clone() as CreatureData;
    //    creatureData.Load(playerCr);
    //    creatureData.factionId = 3;
    //    creatureData.brainId = "HumanHard";
    //    //Snippet.DebugLog($"ContainerID : {creatureData.containerID}", "black");
    //    //creatureData.containerID = creature.container.containerID;
    //    //Snippet.DebugLog($"ContainerID : {creatureData.containerID}", "lime");
    //    Snippet.DebugLog($"Spawn Nb content : {playerCrContainer.contents.Count}", "magenta");
    //    for (int i = playerCrContainer.contents.Count - 1; i >= 0; i--)
    //    {
    //        ContainerContent content = playerCrContainer.contents[i];
    //        Snippet.DebugLog($"Content : {content.GetOutput()} / {content.type}");
    //        switch (content)
    //        {
    //            case ItemContent itemContent:
    //                Snippet.DebugLog($"ItemContent : {itemContent.state} / {itemContent.type} / {itemContent.data.id}", "cyan");
    //                if (itemContent.state is ContentStateHolder holder)
    //                    Snippet.DebugLog($"ItemContent 2 : {holder.holderName}", "yellow");
    //                break;
    //            case SkillContent skillContent:
    //                Snippet.DebugLog($"skillContent : {skillContent.type} / {skillContent.data.id}", "magenta");
    //                break;
    //            case SpellContent spellContent:
    //                Snippet.DebugLog($"skillContent : {spellContent.type} / {spellContent.data.id} / {spellContent.catalogData}", "red");
    //                break;
    //        }
    //        // DOSE NOT WORK
    //        //creature.container.RemoveContent(content);
    //    }
    //    // DOSE NOT WORK
    //    //creature.container.contents = playerContent.CloneContents();
    //    
    //    creatureData.ethnicityId = playerCr.currentEthnicGroup.id;
    //    //creature.Load(creatureData);
    //    GameManager.local.StartCoroutine(creatureData.SpawnCoroutine(Spectator.local.cam.transform.position + Spectator.local.cam.transform.forward * 2f,
    //        Spectator.local.cam.transform.rotation.eulerAngles.y + 180f, null, cr =>
    //        {
    //            ClearArmor(cr);
    //            ClearWeapons(cr);
    //            foreach (ContainerContent content1 in playerCrContainer.contents)
    //            {
    //                switch (content1)
    //                {
    //                    //case SpellContent spellContent:
    //                    //    .AddSkillAndRequirements((SkillData) spellContent.data, new Action<ContainerContent>(levelModuleSurvival1.\u003CDisplayRewards\u003Eb__136_2));
    //                    //    continue;
    //                    //case SkillContent skillContent:
    //                    //    levelModuleSurvival1.AddSkillAndRequirements(skillContent.data, new Action<ContainerContent>(levelModuleSurvival1.\u003CDisplayRewards\u003Eb__136_3));
    //                    //    continue;
    //                    case ItemContent itemContent:
    //                        ItemContent content2;
    //                        if (itemContent.data.type == ItemData.Type.Wardrobe && TryEquipArmor(cr, itemContent.data, out content2))
    //                        {
    //                            continue;
    //                        }
    //                        
    //                        if (itemContent.data.type != ItemData.Type.Wardrobe)
    //                        {
    //                            Snippet.DebugLog($"Not a wardrobe : {itemContent.state}", "lime");
    //                            if (itemContent.state is ContentStateHolder holderState)
    //                            {
    //                                Snippet.DebugLog($"ItemContent 2 : {holderState.holderName}", "black");
    //                                foreach (Holder holder in cr.container.linkedHolders)
    //                                {
    //                                    Snippet.DebugLog($"In here {holder.name} / {holderState.holderName}", "red");
    //                                    if (holder.name != holderState.holderName) continue;
    //                                    Snippet.DebugLog($"In here 2 {holder.name} / {holderState.holderName}", "yellow");
    //                                    if (TryEquipHolder(cr, itemContent.data, itemContent))
    //                                    {
    //                                        Snippet.DebugLog($"In here 3 {holder.name} / {holderState.holderName}", "yellow");
    //                                        continue;
    //                                    }
    //                                    
    //                                    //itemContent.Spawn(item =>
    //                                    //{
    //                                    //    Snippet.DebugLog($"Spawning : {item.data} at {holder.name}", "green");
    //                                    //    cr.equipment.GetHolder(holder.drawSlot).Snap(item);
    //                                    //}, Item.Owner.None, false);
    //                                    break;
    //                                }
    //                            }
    //                            
    //                            break;
    //                            //cr.equipment.GetHolder();
    //                            //itemContent.Spawn(item =>
    //                            //{
    //                            //    ItemModuleWardrobe.CreatureWardrobe wardrobe = item.data.GetModule<ItemModuleWardrobe>().GetWardrobe(creature.data);
    //                            //    if (wardrobe == null)
    //                            //        return;
    //                            //    for (int index = 0; index < creature.equipment.wearableSlots.Count; ++index)
    //                            //    {
    //                            //        Wearable wearableSlot = creature.equipment.wearableSlots[index];
    //                            //        if (!wearableSlot.IsItemCompatible(wardrobe)) continue;
    //                            //        wearableSlot.EquipItem(item);
    //                            //        break;
    //                            //    }
    //                            //});
    //                        }
    //                        
    //                        continue;
    //                    default:
    //                        continue;
    //                }
    //            }
    //        }));
    //}
}
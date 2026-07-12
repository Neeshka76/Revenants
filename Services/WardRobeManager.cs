using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace Revenants.Services;

public class WardRobeManager
{
    public void Clear(Creature creature)
    {
        for (int index = creature.container.contents.Count - 1; index >= 0; index--)
        {
            ItemContent itemContent = (ItemContent)creature.container.contents[index];
            if (itemContent.data.type == ItemData.Type.Wardrobe)
                creature.equipment.UnequipWardrobe(itemContent);
            creature.container.RemoveContent(itemContent);
        }
    }

    public bool TryEquip(Creature creature, ItemContent itemContent)
    {
        if (itemContent.state is not ContentStateWorn wornState)
            return false;
        RevenantWardRobeData custom = itemContent.customDataList?
            .OfType<RevenantWardRobeData>()
            .FirstOrDefault();

        if (custom == null)
            return false;
        ItemData replacement = FindReplacement(custom);
        if (replacement == null)
            return false;
        //Snippet.DebugLog($"Try equipping : {replacement.data.id}", "lime");
        creature.equipment.EquipWardrobe(new ItemContent(replacement, new ContentStateWorn()));
        return true;
    }

    /*private void ListWardRobes(Creature creature, List<ContainerContent> contents)
    {
        Snippet.DebugLog($"Content data at snapshot : {contents.Count}", "green");
        foreach (ContainerContent content in contents)
        {
            Snippet.DebugLog($"Content data at snapshot : {content.referenceID}", "lime");
            switch (content)
            {
                case ItemContent itemContent:
                    //Snippet.DebugLog($"Itemcontent : {itemContent.data.id}; {itemContent.data.type}");
                    switch (itemContent.state)
                    {
                        case ContentStateWorn wornState:
                            if (!itemContent.data.TryGetModule(out ItemModuleWardrobe module1))
                            {
                                Snippet.DebugLog($"Cannot wear {itemContent.referenceID} because it doesn't have an itemModuleWardrobe", "red", Snippet.DebugType.Error);
                            }
                            else
                            {
                                if (!module1.TryGetWardrobe(creature, out ItemModuleWardrobe.CreatureWardrobe wardrobe) || wardrobe.manikinWardrobeData == null)
                                    return;
                                Snippet.DebugLog($"Wardrobe data ({itemContent.referenceID}) nb Channels {wardrobe.manikinWardrobeData.channels.Length}", "cyan");
                                foreach (string channel in wardrobe.manikinWardrobeData.channels)
                                {
                                    Snippet.DebugLog($"Wardrobe data ({itemContent.referenceID}) channel : {channel}", "yellow");
                                }
    
                                //Snippet.DebugLog($"Wardrobe data ({itemContent.referenceID}) nb Layers {wardrobe.manikinWardrobeData.layers.Length}", "blue");
                                //foreach(int layer in wardrobe.manikinWardrobeData.layers)
                                //{
                                //    Snippet.DebugLog($"Wardrobe data ({itemContent.referenceID}) layer : {layer}", "orange");
                                //}
                                break;
                            }
    
                            continue;
                        default:
                            continue;
                    }
    
                    break;
            }
        }
    
        HashSet<string> idsWithTorso = new HashSet<string>();
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
                    if (channel != "Torso") continue;
                    idsWithTorso.Add(id);
                }
            }
        }
    
        foreach (string id in idsWithTorso)
        {
            Snippet.DebugLog($"Ids with Torso : {id}", "lime");
        }
    
    }*/


    private ItemData FindReplacement(RevenantWardRobeData data)
    {
        // Try original first
        if (!string.IsNullOrEmpty(data.Id))
        {
            Catalog.TryGetData(data.Id, out ItemData original, false);
            if (original != null)
                return original;
        }

        // Fallback list based on the data of the candidate
        List<ItemData> candidates = new List<ItemData>();
        foreach (string id in Catalog.GetAllID(Category.Item))
        {
            ItemData itemData = Catalog.GetData<ItemData>(id);
            if (itemData.tier != data.Tier) continue;
            if (!itemData.TryGetModule(out ItemModuleWardrobe module1)) continue;
            foreach (ItemModuleWardrobe.CreatureWardrobe wardrobe in module1.wardrobes)
            {
                //Snippet.DebugLog($"wardrobe : {wardrobe.manikinWardrobeData.channels.Length} for {id}", "yellow");
                foreach (string channel in wardrobe.manikinWardrobeData.channels)
                {
                    //Snippet.DebugLog($"wardrobe : {channel} for {id}", "cyan");
                    if (channel != data.Channel) continue;
                    candidates.Add(itemData);
                }
            }
        }

        return candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : null;
    }
}
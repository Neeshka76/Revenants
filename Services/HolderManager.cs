using System.Collections.Generic;
using System.Linq;
using ThunderRoad;
using UnityEngine;

namespace Revenants.Services;

public class HolderManager
{
    public void Clear(Creature creature)
    {
        List<Item> list = creature.equipment.GetHolsterWeapons();
        //Snippet.DebugLog($"Despawning : {list.Count}", "lime");
        for (int i = list.Count - 1; i >= 0; i--)
        {
            //Snippet.DebugLog($"Despawning : {list[i].itemId}", "lime");
            Item item = list[i];
            item.Despawn();
        }
    }

    public bool TryEquip(Creature creature, ItemContent itemContent)
    {
        if (itemContent.state is not ContentStateHolder holderState)
            return false;
        RevenantItemData custom = itemContent.customDataList?
            .OfType<RevenantItemData>()
            .FirstOrDefault();

        if (custom == null)
            return false;
        //Snippet.DebugLog($"Original holder data : {custom.Id} at {holderState.holderName}", "lime");
        ItemData replacement = FindReplacement(custom);
        if (replacement == null)
            return false;
        // Find the holder then spawn replacement
        foreach (Holder holder in creature.container.linkedHolders)
        {
            if (holder.name != holderState.holderName) continue;

            for (int i = holder.items.Count - 1; i >= 0; i--)
            {
                Item item = holder.items[i];
                item.Despawn();
            }

            replacement.SpawnAsync(item =>
            {
                //Snippet.DebugLog($"Spawning : {item.itemId} at {holder.name}", "lime");
                holder.UnSnapAll();
                holder.Snap(item, true);
            }, holder.expectedPosition, Quaternion.identity);
            return true;
        }

        return false;
    }

    private ItemData FindReplacement(RevenantItemData data)
    {
        // Try original first
        if (!string.IsNullOrEmpty(data.Id))
        {
            Catalog.TryGetData(data.Id, out ItemData original, false);
            if (original != null)
            {
                //Snippet.DebugLog($"Original returned : {original.data.id}", "cyan");
                return original;
            }
        }

        // Fallback list based on the data of the
        List<ItemData> candidates = new List<ItemData>();

        foreach (string id in Catalog.GetAllID(Category.Item))
        {
            ItemData itemData = Catalog.GetData<ItemData>(id);

            if (itemData.type != data.Type) continue;
            if (itemData.slot != data.Slot) continue;
            if (itemData.tier != data.Tier) continue;

            candidates.Add(itemData);
        }

        return candidates.Count > 0
            ? candidates[Random.Range(0, candidates.Count)]
            : null;
    }
}
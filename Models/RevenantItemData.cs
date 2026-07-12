using System;
using ThunderRoad;

namespace Revenants;

[Serializable]
public class RevenantItemData : ContentCustomData
{
    public RevenantItemData()
    {
    }

    public RevenantItemData(string id, int tier, string slot, ItemData.Type type, string category)
    {
        Id = id;
        Tier = tier;
        Slot = slot;
        Type = type;
        Category = category;
    }

    public string Id;
    public int Tier;
    public string Slot;
    public ItemData.Type Type;
    public string Category;
}
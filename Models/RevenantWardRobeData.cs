using System;
using ThunderRoad;

namespace Revenants;

[Serializable]
public class RevenantWardRobeData : ContentCustomData
{
    public RevenantWardRobeData()
    {
    }

    public RevenantWardRobeData(string id, int tier, string channel)
    {
        Id = id;
        Tier = tier;
        Channel = channel;
    }

    public string Id;
    public int Tier;
    public string Channel;
}
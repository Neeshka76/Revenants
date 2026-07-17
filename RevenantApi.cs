using System;
using Revenants.Services;
using ThunderRoad;
using UnityEngine;

namespace Revenants.API;

public static class RevenantApi
{
    internal static RevenantManager RevenantManager;
    public static bool IsAvailable => RevenantManager != null;
    public static event Action<Creature> OnRevenantSpawned;
    internal static void RaiseRevenantSpawned(Creature creature) => OnRevenantSpawned?.Invoke(creature);
    public static void SpawnRandomRevenant(Vector3 position) => RevenantManager?.SpawnRandomRevenant(position);
}
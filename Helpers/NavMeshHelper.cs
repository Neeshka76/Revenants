using UnityEngine;
using UnityEngine.AI;

namespace Revenants.Helpers;

public static class NavMeshHelper
{
    public static float GetNavMeshReachability(Bounds bounds, int samples = 200)
    {
        int validSeeds = 0;
        
        for (int i = 0; i < samples; i++)
        {
            Vector3 random = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.center.y,
                Random.Range(bounds.min.z, bounds.max.z)
            );
            
            if (NavMesh.SamplePosition(random, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                validSeeds++;
            }
        }
        
        return validSeeds / (float)samples;
    }
}
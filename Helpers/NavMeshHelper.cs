using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Revenants.Helpers;

public static class NavMeshHelper
{
    private struct Triangle
    {
        public Bounds Bounds;
        public float Area;
    }
    
    private static List<Triangle> s_triangles;
    private static readonly Dictionary<string, float> s_roomAreaCache = new();
    
    public static void ClearCache()
    {
        s_triangles = null;
        s_roomAreaCache.Clear();
    }
    
    private static void BuildTriangleCache()
    {
        if (s_triangles != null)
            return;
        
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        
        s_triangles = new List<Triangle>(triangulation.indices.Length / 3);
        
        for (int i = 0; i < triangulation.indices.Length; i += 3)
        {
            Vector3 a = triangulation.vertices[triangulation.indices[i]];
            Vector3 b = triangulation.vertices[triangulation.indices[i + 1]];
            Vector3 c = triangulation.vertices[triangulation.indices[i + 2]];
            
            float area = Vector3.Cross(b - a, c - a).magnitude * 0.5f;
            
            // Ignore invalid triangles
            if (area <= Mathf.Epsilon)
                continue;
            
            Bounds triBounds = new Bounds(a, Vector3.zero);
            triBounds.Encapsulate(b);
            triBounds.Encapsulate(c);
            
            s_triangles.Add(new Triangle
            {
                Bounds = triBounds,
                Area = area
            });
        }
    }
    
    public static float GetWalkableArea(string areaId, Bounds bounds, bool useCache = true)
    {
        if (useCache && s_roomAreaCache.TryGetValue(areaId, out float cached))
            return cached;
        
        BuildTriangleCache();
        
        float totalArea = 0f;
        
        foreach (Triangle tri in s_triangles)
        {
            if (tri.Bounds.Intersects(bounds))
                totalArea += tri.Area;
        }
        
        // Only store results when requested
        if (useCache)
            s_roomAreaCache[areaId] = totalArea;
        
        return totalArea;
    }
    
    private static bool ContainsXZ(Bounds bounds, Vector3 point)
    {
        return point.x >= bounds.min.x &&
               point.x <= bounds.max.x &&
               point.z >= bounds.min.z &&
               point.z <= bounds.max.z;
    }
}
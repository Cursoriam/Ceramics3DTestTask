using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MathFunctions : MonoBehaviour
{
    public static bool NearlyEqual(float a, float b)
    {
        var difference = Math.Abs(a * .0001f);

        return Math.Abs(a - b) <= difference;
    }
    
    public static List<Vector3> GetClockwiseSortedVertices(List<Vector3> vertices)
    {
        var lowerLeftVertex = GetLowerLeftVertex(vertices);

        var tmpVertices = vertices.ToArray();
        
        Array.Sort(tmpVertices, new ClockwiseComparer(lowerLeftVertex));

        var sortedVertices = new List<Vector3> {lowerLeftVertex};

        foreach (var vertex in tmpVertices)
        {
            if (lowerLeftVertex != vertex)
                sortedVertices.Add(vertex);
        }

        sortedVertices = sortedVertices.Distinct().ToList();
        sortedVertices.Reverse();

        return sortedVertices;
    }

    private static Vector3 GetLowerLeftVertex(List<Vector3> vertices)
    {
        var lowerLeftVertex = Vector3.positiveInfinity;
        foreach (var vertex in vertices.Where(vertex => vertex.y < lowerLeftVertex.y ||
                                                        Math.Abs(vertex.y - lowerLeftVertex.y) < 0.001f
                                                        && vertex.x < lowerLeftVertex.x))
        {
            lowerLeftVertex = vertex;
        }

        return lowerLeftVertex;
    }
}

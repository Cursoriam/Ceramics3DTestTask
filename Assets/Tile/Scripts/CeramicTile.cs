using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CeramicTile
{
    private readonly Mesh _mesh;
    private readonly Material _material;
    private readonly string _name;

    public GameObject CeramicTileGameObject;
    
    public CeramicTile(List<Vector3> vertices, Vector2 tileSize, Material material, string name)
    {
        _mesh = new Mesh
        {
            name = "Ceramic Tile Mesh",
            vertices = vertices.ToArray(),
            triangles = GetTriangles(vertices.ToArray()),
            uv = GetUV(vertices.ToArray(), tileSize)
        };
        _material = material;
        _name = name;
        CreateGameObject();
    }
    
    private int[] GetTriangles(Vector3[] vertices)
    {
        return TriangulateConvexPolygon(vertices.ToList());
    }

    private Vector2[] GetUV(Vector3[] vertices, Vector2 tileSize)
    {
        var uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            uv[i] = new Vector2(vertices[i].x/tileSize.x, vertices[i].y/tileSize.y);

        return uv;
    }

    private void CreateGameObject()
    {
         CeramicTileGameObject = new GameObject()
        {
            name = _name
        };
        CeramicTileGameObject.AddComponent<MeshFilter>().mesh = _mesh;
        CeramicTileGameObject.AddComponent<MeshRenderer>().material = _material;
    }

    private static int[] TriangulateConvexPolygon(List<Vector3> convexHullpoints)
    {
        List<int> triangles = new List<int>();

        for (int i = 2; i < convexHullpoints.Count; i++)
        {
            triangles.Add(0);
            triangles.Add(i - 1);
            triangles.Add(i);
        }
        
        return triangles.ToArray();
    }
}

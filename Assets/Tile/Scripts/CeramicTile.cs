using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.WSA;

public class CeramicTile
{
    private Mesh _mesh;
    private Material _material;
    private string _name;

    public GameObject CeramicTileGameObject;
    
    public CeramicTile(List<Vector3> vertices, Vector2 tileSize, Material material, string name)
    {
        _mesh = new Mesh();
        _mesh.name = "Ceramic Tile Mesh";
        _mesh.vertices = vertices.ToArray();
        _mesh.triangles = GetTriangles(vertices.ToArray());
        _mesh.uv = GetUV(vertices.ToArray(), tileSize);
        _material = material;
        _name = name;
        CreateGameObject();
    }

    private Vector3[] GetVertices(Vector2 rightUpperAngle)
    {
        //Добавить дополнительные точки при пересечении с границей стены
        var vertices = new List<Vector3>();
        vertices.Add(new Vector3(0.0f, 0.0f));
        vertices.Add(new Vector3(0.0f, rightUpperAngle.y));
        vertices.Add(new Vector3(rightUpperAngle.x, rightUpperAngle.y));
        vertices.Add(new Vector3(rightUpperAngle.x, 0.0f));
        return vertices.ToArray();
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
    
    public static int[] TriangulateConvexPolygon(List<Vector3> convexHullpoints)
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

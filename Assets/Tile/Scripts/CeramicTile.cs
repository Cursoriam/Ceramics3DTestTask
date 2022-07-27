using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class CeramicTile
{
    private Mesh _mesh;
    private Material _material;
    private string _name;

    public GameObject CeramicTileGameObject;
    
    public CeramicTile(Vector2 tileSize, Material material, string name)
    {
        _mesh = new Mesh();
        _mesh.name = "Ceramic Tile Mesh";
        _mesh.vertices = GetVertices(tileSize);
        _mesh.triangles = GetTriangles(tileSize);
        _mesh.uv = GetUV(tileSize);
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

    private int[] GetTriangles(Vector2 tileSize)
    {
        var triangulator = new Triangulator(GetVertices(tileSize));
        return triangulator.Triangulate();
    }

    private Vector2[] GetUV(Vector2 tileSize)
    {
        var vertices = GetVertices(tileSize);
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
}

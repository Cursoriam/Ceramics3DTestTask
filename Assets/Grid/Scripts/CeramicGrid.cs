using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CeramicGrid : MonoBehaviour
{
    [SerializeField] private int xSize;
    [SerializeField] private int ySize;
    
    [SerializeField] private Vector3 initialPoint;
    [SerializeField] private Vector2 tileSize;
    
    [SerializeField] private float seam;
    [SerializeField] private float bias;
    public Material material;
    public int angle;
    private List<GameObject> _tiles;

    [SerializeField] private Vector2 wallSize;
    private Vector3 _rotationPivot;

    private Vector3 _leftLowerWallAngle;
    private Vector3 _leftUpperWallAngle;
    private Vector3 _rightUpperWallAngle;
    private Vector3 _rightLowerWallAngle;
    private List<Vector3> alreadyVisited;



    private void Awake()
    {
        _rotationPivot = transform.position;;
        _leftLowerWallAngle = initialPoint;
        _leftUpperWallAngle = new Vector3(initialPoint.x, wallSize.y);
        _rightUpperWallAngle = new Vector3(wallSize.x, wallSize.y);
        _rightLowerWallAngle = new Vector3(wallSize.x, initialPoint.y);
        alreadyVisited = new List<Vector3>();
        _tiles = new List<GameObject>();
        GenerateTiles(initialPoint);
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, -angle);
    }
    
    private void GenerateTiles(Vector2 spawnPoint)
    {
        alreadyVisited.Add(spawnPoint);
        var ceramicTile = new CeramicTile(tileSize, material, "Tile (" + spawnPoint.x + ", " + spawnPoint.y + ")");
        ceramicTile.CeramicTileGameObject.transform.position = new Vector3(spawnPoint.x, spawnPoint.y);
        ceramicTile.CeramicTileGameObject.transform.SetParent(transform);
        _tiles.Add(ceramicTile.CeramicTileGameObject);
        var vertices = ceramicTile.CeramicTileGameObject.GetComponent<MeshFilter>().mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            var direction = (vertices[i] - _rotationPivot);
            direction = Quaternion.Euler(0.0f, 0.0f, -angle) * direction;
            vertices[i] = direction + _rotationPivot;
            vertices[i] = ceramicTile.CeramicTileGameObject.transform.TransformPoint(vertices[i]);
        }
        
        
        //CheckLeft
        if ((TileInsideWalls(new Vector2(spawnPoint.x - (tileSize.x + seam), spawnPoint.y), spawnPoint)) &&
            !PointAlreadyVisited(new Vector2(spawnPoint.x - (tileSize.x + seam), spawnPoint.y)))
        {
            GenerateTiles(new Vector2(spawnPoint.x - (tileSize.x + seam), spawnPoint.y));
        }

        //CheckUp
        if ((TileInsideWalls(new Vector2(spawnPoint.x + bias, spawnPoint.y + (tileSize.y + seam)), spawnPoint)) &&
            !PointAlreadyVisited(new Vector2(spawnPoint.x + bias, spawnPoint.y + (tileSize.y + seam))))
        {
            GenerateTiles(new Vector2(spawnPoint.x + bias, spawnPoint.y + (tileSize.y + seam)));
        }

        //CheckRight
        if ((TileInsideWalls(new Vector2(spawnPoint.x + (tileSize.x + seam), spawnPoint.y), spawnPoint)) &&
            !PointAlreadyVisited(new Vector2(spawnPoint.x + (tileSize.x + seam), spawnPoint.y)))
        {
            GenerateTiles(new Vector2(spawnPoint.x + (tileSize.x + seam), spawnPoint.y));
        }

        //CheckDown
        if ((TileInsideWalls(new Vector2(spawnPoint.x - bias, spawnPoint.y - (tileSize.y + seam)), spawnPoint)) &&
            !PointAlreadyVisited(new Vector2(spawnPoint.x - bias, spawnPoint.y - (tileSize.y + seam))))
        {
            GenerateTiles(new Vector2(spawnPoint.x - bias, spawnPoint.y - (tileSize.y + seam)));
        }
    }

    private bool IsIntersectingWithWall(Vector2 leftLowerVertex, Vector2 leftUpperVertex, Vector2 rightUpperVertex,
        Vector2 rightLowerVertex, Vector2 wallStart, Vector2 wallEnd)
    {
        if (GeometryUtils.IsSegmentIntersecting(wallStart,
            wallEnd, leftLowerVertex, leftUpperVertex))
            return true;
        
        if (GeometryUtils.IsSegmentIntersecting(wallStart,
            wallEnd, leftUpperVertex, rightUpperVertex))
            return true;
        
        if (GeometryUtils.IsSegmentIntersecting(wallStart,
            wallEnd, rightUpperVertex, rightLowerVertex))
            return true;
        
        if (GeometryUtils.IsSegmentIntersecting(wallStart,
            wallEnd, rightLowerVertex, leftLowerVertex))
            return true;

        return false;
    }
    
    private bool PointInsideWalls(Vector3 point)
    {
        var inside = false;
        var rect = new Rect(_leftLowerWallAngle.x, _leftLowerWallAngle.y, _rightUpperWallAngle.x,
            _rightUpperWallAngle.y);
        if (rect.Contains(point))
            inside = true;
        

        return inside;
    }

    private bool PointAlreadyVisited(Vector3 point)
    {
        var visited = false;
        foreach (var visitedPoint in alreadyVisited)
        {
            if (NearlyEqual(point.x, visitedPoint.x) &&
                NearlyEqual(point.y, visitedPoint.y))
            {
                visited = true;
            }
        }
        return visited;
    }

    private bool TileInsideWalls(Vector2 spawnPoint, Vector2 basePoint)
    {
        var inside = false;
        var vertices = new Vector3[4];
        vertices[0] = new Vector3(spawnPoint.x, spawnPoint.y);
        vertices[1] = new Vector3(spawnPoint.x, spawnPoint.y + tileSize.y);
        vertices[2] = new Vector3(spawnPoint.x + tileSize.x, spawnPoint.y + tileSize.y);
        vertices[3] = new Vector3(spawnPoint.x + tileSize.x, spawnPoint.y);
        
        for (int i = 0; i < vertices.Length; i++)
        {
            var direction = (vertices[i] - _rotationPivot);
            direction = Quaternion.Euler(0.0f, 0.0f, -angle) * direction;
            vertices[i] = direction + _rotationPivot;
        }
        
        if (PointInsideWalls(vertices[0]) || PointInsideWalls(vertices[1]) ||
              PointInsideWalls(vertices[2]) || PointInsideWalls(vertices[3]))
        {
            inside = true;
        }
        
        return inside;
    }
    
    private bool NearlyEqual(float a, float b)
    {
        var difference = Math.Abs(a * .0001f);

        return Math.Abs(a - b) <= difference;
    }
}

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

struct LineSegment
{
    public Vector2 start;
    public Vector2 end;
}

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
        _leftUpperWallAngle = new Vector3(initialPoint.x, initialPoint.y + wallSize.y);
        _rightUpperWallAngle = new Vector3(initialPoint.x + wallSize.x, initialPoint.y + wallSize.y);
        _rightLowerWallAngle = new Vector3(initialPoint.x + wallSize.x, initialPoint.y);
        alreadyVisited = new List<Vector3>();
        _tiles = new List<GameObject>();
        GenerateTiles(initialPoint);
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, -angle);
        Debug.Log(_rightUpperWallAngle);
        
        foreach (var tile in _tiles)
        {
            IsIntersectingWithWalls(tile);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(mousepos);
        }
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

    private void IsIntersectingWithWalls(GameObject tile)
    {
        var vertices = tile.GetComponent<MeshFilter>().mesh.vertices;
        var tileSides = new LineSegment[4];
        tileSides[0].start = tile.transform.TransformPoint(vertices[0]);
        tileSides[0].end = tile.transform.TransformPoint(vertices[1]);
        tileSides[1].start = tile.transform.TransformPoint(vertices[1]);
        tileSides[1].end = tile.transform.TransformPoint(vertices[2]);
        tileSides[2].start = tile.transform.TransformPoint(vertices[2]);
        tileSides[2].end = tile.transform.TransformPoint(vertices[3]);
        tileSides[3].start = tile.transform.TransformPoint(vertices[3]);
        tileSides[3].end = tile.transform.TransformPoint(vertices[0]);

        Debug.Log(tile.name);
        
        
        var leftWallIntersects = false;
        var upperWallIntersects = false;
        var rightWallIntersects = false;
        var lowerWallIntersects = false;

        var intersection = new Vector2();
        var found = false;
        
        for (int i = 0; i < tileSides.Length; i++)
        {
            if (GeometryUtils.IntersectLineSegments2D(tileSides[i].start, tileSides[i].end,
                _leftLowerWallAngle, _leftUpperWallAngle, out intersection))
                leftWallIntersects = true;
            
            if (GeometryUtils.IntersectLineSegments2D(tileSides[i].start, tileSides[i].end,
                _leftUpperWallAngle, _rightUpperWallAngle, out intersection))
                upperWallIntersects = true;

            
            if (GeometryUtils.IntersectLineSegments2D(tileSides[i].start, tileSides[i].end,
                _rightUpperWallAngle, _rightLowerWallAngle, out intersection))
                rightWallIntersects = true;
            
            if (GeometryUtils.IntersectLineSegments2D(tileSides[i].start, tileSides[i].end,
                _rightLowerWallAngle, _leftLowerWallAngle, out intersection))
                lowerWallIntersects = true;
        }

        if(leftWallIntersects)
            Debug.Log("Left Wall Intersection");
        
        if(upperWallIntersects)
            Debug.Log("Upper Wall Intersection");
        
        if(rightWallIntersects)
            Debug.Log("Right Wall Intersection");
        
        if(lowerWallIntersects)
            Debug.Log("Lower Wall Intersection");
    }
    
    private bool PointInsideWalls(Vector3 point)
    {
        var inside = false;
        var rect = new Rect(initialPoint.x, initialPoint.y, wallSize.x,
            wallSize.y);
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

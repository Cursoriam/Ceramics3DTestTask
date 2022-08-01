using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CeramicGrid
{
    private readonly Vector3 _initialPoint;
    private readonly Vector2 _tileSize;
    
    private readonly float _seam;
    private readonly float _bias;
    private readonly int _angle;
    private readonly Material _material;
    public  List<GameObject> Tiles;

    private readonly Vector2 _wallSize;
    private  Vector3 _rotationPivot;
    
    private  List<Vector3> _alreadyVisited;
    public float Square;
    private readonly Transform _parent;
    private  Vector3[] _tileCorners;
    private  Vector3[] _wallCorners;
    
    public CeramicGrid(float seam, int angle, float bias, Vector2 tileSize, Material material, Transform parent,
        Vector2 wallSize, Vector3 initialPoint)
    {
        _seam = seam;
        _angle = angle;
        _bias = bias;

        _tileSize = tileSize;
        _material = material;
        _parent = parent;

        _wallSize = wallSize;
        _initialPoint = initialPoint;
        
        InitializeVariables();
        
        var startPoint = RotatePointAroundPivot(initialPoint, _angle);
        GenerateTiles(startPoint);
        _parent.rotation = Quaternion.Euler(0.0f, 0.0f, -_angle);
    }

    private Vector3 RotatePointAroundPivot(Vector3 point, int angle)
    {
        var direction = (point - _rotationPivot);
        direction = Quaternion.Euler(0.0f, 0.0f, angle) * direction;
        point = direction + _rotationPivot;
        return point;
    }

    private Vector3[] GetWallCorners()
    {
        var wallCorners = new Vector3[4];
        wallCorners[0] = _initialPoint;
        wallCorners[1] = new Vector3(_initialPoint.x, _initialPoint.y + _wallSize.y);
        wallCorners[2] = new Vector3(_initialPoint.x + _wallSize.x, _initialPoint.y + _wallSize.y);
        wallCorners[3] = new Vector3(_initialPoint.x + _wallSize.x, _initialPoint.y);
        return wallCorners;
    }

    private Vector3[] GetTileCorners()
    {
        var tileCorners = new Vector3[4];
        tileCorners[0] = new Vector3(0.0f, 0.0f);
        tileCorners[1] = new Vector3(0.0f, _tileSize.y);
        tileCorners[2] = new Vector3(_tileSize.x, _tileSize.y);
        tileCorners[3] = new Vector3(_tileSize.x, 0.0f);
        return tileCorners;
    }

    private void InitializeVariables()
    {
        Tiles = new List<GameObject>();
        _rotationPivot = _parent.position;
        _alreadyVisited = new List<Vector3>();
        _wallCorners = GetWallCorners();
        _tileCorners = GetTileCorners();
    }

    //Method for generating tiles
    private void GenerateTiles(Vector3 spawnPoint)
    {
        //Round spawn point coordinates
        spawnPoint = new Vector3(Mathf.Round(spawnPoint.x * 100f) / 100f,
            Mathf.Round(spawnPoint.y * 100f) / 100f);

        //Add point to already visited points to avoid stack overflow
        _alreadyVisited.Add(spawnPoint);

        //Check that tile corners are inside wall
        var vertices = _tileCorners.Where(corner =>
            PointInsideWalls(GetWorldPoint(corner, spawnPoint))).ToList();

        //Add intersections with walls to vertices to cut mesh by wall sides
        vertices.AddRange(GetTileSideIntersectionsWithWalls(_tileCorners, spawnPoint));

        
        //If mesh cut by wall corner, then add wall corners to vertices
        vertices.AddRange(from wallCorner in _wallCorners where WallCornerInsideTile(wallCorner, spawnPoint)
            select GetLocalTilePoint(wallCorner, spawnPoint));
        
        //Sort vertices in clockwise order
        var sortedVertices = MathFunctions.GetClockwiseSortedVertices(vertices);
        
        //Generate mesh based on vertices
        GenerateTile(sortedVertices.ToList(), spawnPoint);

        //To calculate sum we need to get vertices in reverse order to triangulate them in a proper way
        sortedVertices.Reverse();
        Square += GeometryUtils.CalculatePolygonSquare(sortedVertices);

        //Check all neighbor spawn points and if tiles based on this points are inside wall, then generate tiles
        //recursively
        var neighborPoints = GetNeighborPoints(spawnPoint);
        foreach (var neighborPoint in neighborPoints.Where(neighborPoint => 
            TileVerticesInsideWalls(neighborPoint) && !PointAlreadyVisited(neighborPoint)))
        {
            GenerateTiles(neighborPoint);
        }
    }

    private List<Vector3> GetNeighborPoints(Vector3 spawnPoint)
    {
        var neighborPoints = new List<Vector3>
        {
            new Vector2(Mathf.Round((spawnPoint.x - (_tileSize.x + _seam)) * 100f) / 100f,
                Mathf.Round(spawnPoint.y * 100f) / 100f), //Left neighbor point
            new Vector2(Mathf.Round((spawnPoint.x + _bias) * 100f) / 100f,
                Mathf.Round((spawnPoint.y + (_tileSize.y + _seam)) * 100f) / 100f), //Upper neighbor point
            new Vector2(Mathf.Round((spawnPoint.x + (_tileSize.x + _seam)) * 100f) / 100f,
                Mathf.Round(spawnPoint.y * 100f) / 100f), //Right neighbor point
            new Vector2(Mathf.Round((spawnPoint.x - _bias) * 100f) / 100f,
                Mathf.Round((spawnPoint.y - (_tileSize.y + _seam)) * 100f) / 100f) //Lower neighbor point
        };
        return neighborPoints;
    }

    private void GenerateTile(List<Vector3> vertices, Vector3 spawnPoint)
    {
        var ceramicTile = new CeramicTile(vertices.ToList(), _tileSize, _material,
            "Tile (" + spawnPoint.x + ", " + spawnPoint.y + ")");
        ceramicTile.CeramicTileGameObject.transform.position = new Vector3(spawnPoint.x, spawnPoint.y);
        ceramicTile.CeramicTileGameObject.transform.rotation = Quaternion.identity;
        ceramicTile.CeramicTileGameObject.transform.SetParent(_parent);
        ceramicTile.CeramicTileGameObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        Tiles.Add(ceramicTile.CeramicTileGameObject);
    }
    
    private Vector3 GetLocalTilePoint(Vector3 point, Vector3 spawnPoint)
    {
        var direction = (point - _rotationPivot);
        direction = Quaternion.Euler(0.0f, 0.0f, _angle) * direction;
        point = direction + _rotationPivot;
        point -= spawnPoint;
        return point;
    }

    private bool WallCornerInsideTile(Vector3 corner, Vector3 spawnPoint)
    {
        var rect = new Rect(0.0f, 0.0f, _tileSize.x, _tileSize.y);
        
        return rect.Contains(GetLocalTilePoint(corner, spawnPoint));
    }

    private Vector2 GetWorldPoint(Vector3 point, Vector3 spawnPoint)
    {
        point += spawnPoint;
        var direction = (point - _rotationPivot);
        direction = Quaternion.Euler(0.0f, 0.0f, -_angle) * direction;
        point = direction + _rotationPivot;
        return point;
    }

    private List<Vector3> GetTileSideIntersectionsWithWalls(Vector3[] corners, Vector3 spawnPoint)
    {
        var intersections = new List<Vector3>();

        var tileSides = new LineSegment[4];
        
        tileSides[0].start = GetWorldPoint(corners[0], spawnPoint);
        tileSides[0].end = GetWorldPoint(corners[1], spawnPoint);
        tileSides[1].start = GetWorldPoint(corners[1], spawnPoint);
        tileSides[1].end = GetWorldPoint(corners[2], spawnPoint);
        tileSides[2].start = GetWorldPoint(corners[2], spawnPoint);
        tileSides[2].end = GetWorldPoint(corners[3], spawnPoint);
        tileSides[3].start = GetWorldPoint(corners[3], spawnPoint);
        tileSides[3].end = GetWorldPoint(corners[0], spawnPoint);

        var wallSides = new LineSegment[4];
        wallSides[0].start = _wallCorners[0];
        wallSides[0].end = _wallCorners[1];
        wallSides[1].start = _wallCorners[1];
        wallSides[1].end = _wallCorners[2];
        wallSides[2].start = _wallCorners[2];
        wallSides[2].end = _wallCorners[3];
        wallSides[3].start = _wallCorners[3];
        wallSides[3].end = _wallCorners[0];

        foreach (var tileSide in tileSides)
        {
            foreach (var wallSide in wallSides)
            {
                if (GeometryUtils.IntersectLineSegments2D(tileSide.start, tileSide.end, wallSide.start,
                    wallSide.end, out var intersection))
                    intersections.Add(GetLocalTilePoint(intersection, spawnPoint));
            }
        }
        
        return intersections;
    }
    
    private bool PointInsideWalls(Vector3 point)
    {
        var inside = false;
        var rect = new Rect(_initialPoint.x, _initialPoint.y, _wallSize.x,
            _wallSize.y);
        if (rect.Contains(point))
            inside = true;
        

        return inside;
    }

    private bool PointAlreadyVisited(Vector3 point)
    {
        var visited = false;
        foreach (var visitedPoint in _alreadyVisited)
        {
            if (MathFunctions.NearlyEqual(point.x , visitedPoint.x) &&
                MathFunctions.NearlyEqual(point.y, visitedPoint.y))
            {
                visited = true;
            }
        }
        return visited;
    }

    private bool TileVerticesInsideWalls(Vector2 spawnPoint)
    {
        var inside = false;
        var vertices = new Vector3[4];
        vertices[0] = new Vector3(spawnPoint.x, spawnPoint.y);
        vertices[1] = new Vector3(spawnPoint.x, spawnPoint.y + _tileSize.y);
        vertices[2] = new Vector3(spawnPoint.x + _tileSize.x, spawnPoint.y + _tileSize.y);
        vertices[3] = new Vector3(spawnPoint.x + _tileSize.x, spawnPoint.y);
        
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = RotatePointAroundPivot(vertices[i], -_angle);
        }
        
        if (PointInsideWalls(vertices[0]) || PointInsideWalls(vertices[1]) ||
              PointInsideWalls(vertices[2]) || PointInsideWalls(vertices[3]))
        {
            inside = true;
        }
        
        return inside;
    }
}

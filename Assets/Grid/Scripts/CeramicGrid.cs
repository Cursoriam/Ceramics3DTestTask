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

public class ClockwiseComparer : IComparer
    {
        private Vector2 m_Origin;
 
        #region Properties
 
        /// <summary>
        ///     Gets or sets the origin.
        /// </summary>
        /// <value>The origin.</value>
        public Vector2 origin { get { return m_Origin; } set { m_Origin = value; } }
 
        #endregion
 
        /// <summary>
        ///     Initializes a new instance of the ClockwiseComparer class.
        /// </summary>
        /// <param name="origin">Origin.</param>
        public ClockwiseComparer(Vector2 origin)
        {
            m_Origin = origin;
        }
 
        #region IComparer Methods
 
        /// <summary>
        ///     Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        public int Compare(object first, object second)
        {
            Vector3 pointA = (Vector3)first;
            Vector3 pointB = (Vector3)second;
            
            return IsClockwise(pointA, pointB, m_Origin);
        }
 
        #endregion
 
        /// <summary>
        ///     Returns 1 if first comes before second in clockwise order.
        ///     Returns -1 if second comes before first.
        ///     Returns 0 if the points are identical.
        /// </summary>
        /// <param name="first">First.</param>
        /// <param name="second">Second.</param>
        /// <param name="origin">Origin.</param>
        public static int IsClockwise(Vector3 first, Vector3 second, Vector3 origin)
        {
            if (first == second)
                return 0;
 
            Vector3 firstOffset = first - origin;
            Vector3 secondOffset = second - origin;
 
            float angle1 = Mathf.Atan2(firstOffset.x, firstOffset.y);
            float angle2 = Mathf.Atan2(secondOffset.x, secondOffset.y);
            
            if (angle1 < angle2)
                return 1;
 
            if (angle1 > angle2)
                return -1;
 
            // Check to see which point is closest
            return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? 1 : -1;
        }
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
    private List<Vector3> _vertices;


    private void Awake()
    {
        _rotationPivot = transform.position;;
        _leftLowerWallAngle = initialPoint;
        _leftUpperWallAngle = new Vector3(initialPoint.x, initialPoint.y + wallSize.y);
        _rightUpperWallAngle = new Vector3(initialPoint.x + wallSize.x, initialPoint.y + wallSize.y);
        _rightLowerWallAngle = new Vector3(initialPoint.x + wallSize.x, initialPoint.y);
        alreadyVisited = new List<Vector3>();
        _tiles = new List<GameObject>();
        _vertices = new List<Vector3>();
        GenerateTiles(initialPoint);
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, -angle);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void GenerateTiles(Vector3 spawnPoint)
    {
        alreadyVisited.Add(spawnPoint);
        
        var tileCorners = new Vector3[4];
        tileCorners[0] = new Vector3(0.0f, 0.0f);
        tileCorners[1] = new Vector3(0.0f, tileSize.y);
        tileCorners[2] = new Vector3(tileSize.x, tileSize.y);
        tileCorners[3] = new Vector3(tileSize.x, 0.0f);
        

        var vertices = tileCorners.Where(corner => 
            PointInsideWalls(GetRealPoint(corner, spawnPoint))).ToList();

        Debug.Log("Tile (" + spawnPoint.x + ", " + spawnPoint.y + ")");
        
        vertices.AddRange(TileInterSectionPointsWithWalls(tileCorners, spawnPoint));

        var lowerLeftVertex = Vector3.positiveInfinity;
        
        if (WallCornerInsideTile(_leftLowerWallAngle, spawnPoint))
        {
            var corner = _leftLowerWallAngle;
            var direction = (corner - _rotationPivot);
            direction = Quaternion.Euler(0.0f, 0.0f, angle) * direction;
            corner = direction + _rotationPivot;
            corner -= spawnPoint;
            vertices.Add(corner);
        }
        
        if (WallCornerInsideTile(_leftUpperWallAngle, spawnPoint))
        {
            var corner = _leftUpperWallAngle;
            var direction = (corner - _rotationPivot);
            direction = Quaternion.Euler(0.0f, 0.0f, angle) * direction;
            corner = direction + _rotationPivot;
            corner -= spawnPoint;
            vertices.Add(corner);
        }
        
        if (WallCornerInsideTile(_rightUpperWallAngle, spawnPoint))
        {
            var corner = _rightUpperWallAngle;
            var direction = (corner - _rotationPivot);
            direction = Quaternion.Euler(0.0f, 0.0f, angle) * direction;
            corner = direction + _rotationPivot;
            corner -= spawnPoint;
            vertices.Add(corner);
        }
        
        if (WallCornerInsideTile(_rightLowerWallAngle, spawnPoint))
        {
            var corner = _rightLowerWallAngle;
            var direction = (corner - _rotationPivot);
            direction = Quaternion.Euler(0.0f, 0.0f, angle) * direction;
            corner = direction + _rotationPivot;
            corner -= spawnPoint;
            vertices.Add(corner);
        }
        
        foreach (var vertex in vertices)
        {
            if (vertex.y < lowerLeftVertex.y || (vertex.y == lowerLeftVertex.y && vertex.x < lowerLeftVertex.x))
                lowerLeftVertex = vertex;
        }
        
        var tmpVertices = vertices.ToArray();
        
        
        Array.Sort(tmpVertices, new ClockwiseComparer(lowerLeftVertex));

        var sortedVertices = new List<Vector3>();
        
        sortedVertices.Add(lowerLeftVertex);
        foreach (var vertex in tmpVertices)
        {
            if(lowerLeftVertex != vertex)
                sortedVertices.Add(vertex);
        }

        sortedVertices.Reverse();
        
        _vertices.AddRange(sortedVertices);
        var ceramicTile = new CeramicTile(sortedVertices.ToList(), tileSize, material,
            "Tile (" + spawnPoint.x + ", " + spawnPoint.y + ")");
        ceramicTile.CeramicTileGameObject.transform.position = new Vector3(spawnPoint.x, spawnPoint.y);
        ceramicTile.CeramicTileGameObject.transform.SetParent(transform);
        _tiles.Add(ceramicTile.CeramicTileGameObject);
        


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

    private bool WallCornerInsideTile(Vector3 corner, Vector3 spawnPoint)
    {
        var rect = new Rect(0.0f, 0.0f, tileSize.x, tileSize.y);

        var direction = (corner - _rotationPivot);
        direction = Quaternion.Euler(0.0f, 0.0f, angle) * direction;
        corner = direction + _rotationPivot;
        corner -= spawnPoint;
        if (rect.Contains(corner))
            return true;
        return false;
    }

    private Vector2 GetRealPoint(Vector3 point, Vector3 spawnPoint)
    {
        point += spawnPoint;
        var direction = (point - _rotationPivot);
        direction = Quaternion.Euler(0.0f, 0.0f, -angle) * direction;
        point = direction + _rotationPivot;
        return point;
    }

    private List<Vector3> TileInterSectionPointsWithWalls(Vector3[] corners, Vector3 spawnPoint)
    {
        var intersections = new List<Vector3>();

        var tileSides = new LineSegment[4];
        
        tileSides[0].start = GetRealPoint(corners[0], spawnPoint);
        tileSides[0].end = GetRealPoint(corners[1], spawnPoint);
        tileSides[1].start = GetRealPoint(corners[1], spawnPoint);
        tileSides[1].end = GetRealPoint(corners[2], spawnPoint);
        tileSides[2].start = GetRealPoint(corners[2], spawnPoint);
        tileSides[2].end = GetRealPoint(corners[3], spawnPoint);
        tileSides[3].start = GetRealPoint(corners[3], spawnPoint);
        tileSides[3].end = GetRealPoint(corners[0], spawnPoint);
        
        var intersection = new Vector3();
        
        for (int i = 0; i < tileSides.Length; i++)
        {
            if (GeometryUtils.IntersectLineSegments2D(tileSides[i].start, tileSides[i].end,
                _leftLowerWallAngle, _leftUpperWallAngle, out intersection))
            {
                var direction = (intersection - _rotationPivot);
                direction = Quaternion.Euler(0.0f, 0.0f, angle) * direction;
                intersection = direction + _rotationPivot;
                intersection -= spawnPoint;
                intersections.Add(intersection);
            }

            if (GeometryUtils.IntersectLineSegments2D(tileSides[i].start, tileSides[i].end,
                _leftUpperWallAngle, _rightUpperWallAngle, out intersection))
            {
                var direction = (intersection - _rotationPivot);
                direction = Quaternion.Euler(0.0f, 0.0f, angle) * direction;
                intersection = direction + _rotationPivot;
                intersection -= spawnPoint;
                intersections.Add(intersection);
            }


            if (GeometryUtils.IntersectLineSegments2D(tileSides[i].start, tileSides[i].end,
                _rightUpperWallAngle, _rightLowerWallAngle, out intersection))
            {
                var direction = (intersection - _rotationPivot);
                direction = Quaternion.Euler(0.0f, 0.0f, angle) * direction;
                intersection = direction + _rotationPivot;
                intersection -= spawnPoint;
                intersections.Add(intersection);
            }

            if (GeometryUtils.IntersectLineSegments2D(tileSides[i].start, tileSides[i].end,
                _rightLowerWallAngle, _leftLowerWallAngle, out intersection))
            {
                var direction = (intersection - _rotationPivot);
                direction = Quaternion.Euler(0.0f, 0.0f, angle) * direction;
                intersection = direction + _rotationPivot;
                intersection -= spawnPoint;
                intersections.Add(intersection);
            }
        }

        return intersections;
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
    
    private void OnDrawGizmos () {
        if(_vertices == null)
            return;
        
        Gizmos.color = Color.black;
        for (int i = 0; i < _vertices.Count; i++) {
            Gizmos.DrawSphere(_vertices[i], 0.1f);
        }
    }
}

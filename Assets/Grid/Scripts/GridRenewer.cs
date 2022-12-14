using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GridRenewer : MonoBehaviour
{
    [SerializeField] private InputField seam;
    [SerializeField] private InputField angle;
    [SerializeField] private InputField bias;
    [SerializeField] private Text squareText;
    
    [SerializeField]
    private Vector2 initialPoint;

    [SerializeField] private Vector2 tileSize;

    [SerializeField]
    private Material material;

    [SerializeField] private Transform parent;

    [SerializeField]
    private Vector2 wallSize;

    [SerializeField] private float squareOfUnitsPerMeters;

    private CeramicGrid _grid;

    private void Start()
    {
        _grid = new CeramicGrid(0, 0, 0, tileSize, material, parent, wallSize,
            initialPoint);
        squareText.text = "Площадь, кв/м " + Math.Round((_grid.Square * squareOfUnitsPerMeters) * Constants.Precision) / 
            Constants.Precision;
    }

    public void RenewGrid()
    {

        foreach (var tile in _grid.Tiles)
        {
            Destroy(tile);
        }

        parent.rotation = Quaternion.identity;

        int tileSeam;
        int tileAngle;
        int tileBias;
        int.TryParse(seam.text, out tileSeam);
        int.TryParse(angle.text, out tileAngle);
        int.TryParse(bias.text, out tileBias);

        _grid = new CeramicGrid(tileSeam / Constants.Precision, tileAngle, tileBias / Constants.Precision,
            tileSize, material, parent, wallSize, initialPoint);
        squareText.text = "Площадь, кв/м " + Math.Round((_grid.Square * squareOfUnitsPerMeters) * Constants.Precision) / 
            Constants.Precision;
    }
}

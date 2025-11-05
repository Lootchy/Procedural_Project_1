using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;




[CreateAssetMenu(menuName = "Procedural Generation Method/CellularAutomataOptimize")]
public class CellularAutomataOptimize : ProceduralGenerationMethod
{
    public float groundWeight = 0.5f;
    [SerializeField] private int groundCount = 4;

    private GridObjectTemplate groundTemplate;
    private GridObjectTemplate waterTemplate;
    private List<Cell> allCells = new List<Cell>();

    private readonly Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),   // Droite
        new Vector2Int(-1, 0),  // Gauche
        new Vector2Int(0, 1),   // Haut
        new Vector2Int(0, -1),  // Bas
        new Vector2Int(1, 1),   // Haut-droite
        new Vector2Int(1, -1),  // Bas-droite
        new Vector2Int(-1, 1),  // Haut-gauche
        new Vector2Int(-1, -1)  // Bas-gauche
    };

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
        waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water");
        PlaceRandomCell();


        
        for (int i = 0; i < _maxSteps; i++)
        {
            SmoothGrid();
        }
        cancellationToken.ThrowIfCancellationRequested();
    }

    private void PlaceRandomCell()
    {
        allCells.Clear();
        int totalCells = Grid.Width * Grid.Lenght;
        allCells.Capacity = totalCells;

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                Grid.GetCellByCoordinates(x, z, out Cell chosenCell);

                GridObjectTemplate templateToPlace = RandomService.Chance(groundWeight) ? groundTemplate : waterTemplate;
                GridGenerator.AddGridObjectToCell(chosenCell, templateToPlace, false);
                allCells.Add(chosenCell);
            }
        }
    }

    private void SmoothGrid()
    {
        List<(Cell cell, GridObjectTemplate template)> cellsToUpdate = new List<(Cell, GridObjectTemplate)>();
        string groundName = groundTemplate.Name;

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                if (!Grid.TryGetCellByCoordinates(x, z, out Cell currentCell))
                    continue;

                int groundNeighbors = 0;

                foreach (Vector2Int dir in directions)
                {
                    int checkX = x + dir.x;
                    int checkZ = z + dir.y;

                    if (Grid.TryGetCellByCoordinates(checkX, checkZ, out Cell neighborCell) &&
                     neighborCell.GridObject?.Template.Name == groundName)
                    {
                        groundNeighbors++;
                    }
                }

                GridObjectTemplate newTemplate = groundNeighbors >= groundCount ? groundTemplate : waterTemplate;

                if (currentCell.GridObject?.Template != newTemplate)
                {
                    cellsToUpdate.Add((currentCell, newTemplate));
                }
            }
        }

        foreach (var (cell, template) in cellsToUpdate)
        {
            GridGenerator.AddGridObjectToCell(cell, template, true);
        }
    }
}

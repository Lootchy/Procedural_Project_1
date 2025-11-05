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


public struct CellType
{
    public Cell cell;
    public GridObjectTemplate type;

    public CellType(Cell cell, GridObjectTemplate type)
    {
        this.cell = cell;
        this.type = type;
    }
}


[CreateAssetMenu(menuName = "Procedural Generation Method/CellularAutomata")]
public class CellularAutomata : ProceduralGenerationMethod
{
    public float groundWeight = 0.5f;
    [SerializeField] private int groundCount = 4;

    private GridObjectTemplate groundTemplate;
    private GridObjectTemplate waterTemplate;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        for(int i = 0; i < _maxSteps; i++)
        {
            groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
            waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Water");

            PlaceRandomCell();
            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            SmoothGrid();
            //await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
        }
    }

    private void PlaceRandomCell()
    {
      

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                {
                    Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                    continue;
                }
                GridObjectTemplate templateToPlace = RandomService.Chance(groundWeight) ? groundTemplate : waterTemplate;
                GridGenerator.AddGridObjectToCell(chosenCell, templateToPlace, false);
            }
        }
    }

    private void SmoothGrid()
    {
        Vector2Int[] directions = new Vector2Int[]
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

        Dictionary<Cell, GridObjectTemplate> cellsToUpdate = new Dictionary<Cell, GridObjectTemplate>();

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                if (!Grid.TryGetCellByCoordinates(x, z, out Cell currentCell))
                    continue;

                int groundNeighbors = 0;
                int totalNeighbors = 0;

                foreach (Vector2Int dir in directions)
                {
                    int checkX = x + dir.x;
                    int checkZ = z + dir.y;

                    if (Grid.TryGetCellByCoordinates(checkX, checkZ, out Cell neighborCell))
                    {
                        if (neighborCell.GridObject != null)
                        {
                            totalNeighbors++;
                            if (neighborCell.GridObject.Template.Name == groundTemplate.Name)
                            {
                                groundNeighbors++;
                            }
                        }
                    }
                }

                if (groundNeighbors >= groundCount)
                {
                    cellsToUpdate[currentCell] = groundTemplate;
                }
                else
                {
                    cellsToUpdate[currentCell] = waterTemplate;
                }
            }
        }


        foreach (var kvp in cellsToUpdate)
        {
            GridGenerator.AddGridObjectToCell(kvp.Key, kvp.Value, true);
        }
    }
}

using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using Microsoft.Unity.VisualStudio.Editor;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/TestBSP")]
public class TestBSP : ProceduralGenerationMethod
{
    [NonSerialized]public List<TestNode> nodeList = new List<TestNode>();
    [NonSerialized] private List<RectInt> roomList = new List<RectInt>();
    public int maxNodeSize = 4;
    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {

        TestNode root = new TestNode(new RectInt(0, 0, Grid.Width, Grid.Lenght), this, RandomService);
        int i = 0;
        foreach(TestNode node in nodeList)
        {
            Debug.Log(
                "Node " + i +
                " is " + node.IsLeaf() +
                " because child1: " + (node.Child1 == null) +
                " and child2: " + (node.Child2 == null)
            );
            i += 1;
            if (node.IsLeaf())
            {
                PlacedRoom(node.Bound);
            }
        }
        BuildGround();
    }



    private void BuildGround()
    {
        var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");

        // Instantiate ground blocks
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                {
                    Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                    continue;
                }

                GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
            }
        }
    }

    public void PlacedRoom(RectInt room)
    {

        for (int x = room.xMin - 1; x <= room.xMax + 1; x++)
        {
            for (int y = room.yMin - 1; y <= room.yMax + 1; y++)
            {
                if (Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                {
                    if (cell.ContainObject)
                        return;
                }
            }
        }


        for (int x = room.xMin; x <= room.xMax; x++)
        {
            for (int y = room.yMin; y <= room.yMax; y++)
            {
                if (Grid.TryGetCellByCoordinates(x, y, out Cell cell))
                {
                    if (cell.ContainObject) return;
                    AddTileToCell(cell, ROOM_TILE_NAME, true);
                }
            }
        }
        roomList.Add(room);
    }
}

public class TestNode
{
    private  RectInt _bound;

    public RectInt Bound
    {
        get { return _bound; }
    }

    public TestNode Child1
    {
        get { return _child1; }
    }
    public TestNode Child2
    {
        get { return _child2; }
    }

    private RandomService _randomService;
    private TestNode _child1, _child2;
    private TestBSP BSP;
    private Vector2Int _roomMinSize = new(5, 5);
    public TestNode(RectInt bound, TestBSP bSP, RandomService random)
    {
        _bound = bound;
        BSP = bSP;
        _randomService = random;
        BSP.nodeList.Add(this);
        if (Bound.width > _roomMinSize.x && Bound.height > _roomMinSize.y)
        {
            CreateNewNode();
        }
    }

    public bool IsLeaf()
    {
        return _child1 == null && _child2 == null;
    }

    public void CreateNewNode()
    {
        bool splitHorizontally = _randomService.Chance(0.5f);

        if (!splitHorizontally)
        {
            RectInt splitBoundsBottom = new RectInt(_bound.xMin, _bound.yMin, _bound.width, _bound.height / 2);
            RectInt splitBoundsTop = new RectInt(_bound.xMin, _bound.yMin + _bound.height / 2, _bound.width, _bound.height / 2);

            _child1 = new TestNode(splitBoundsBottom, BSP, _randomService);
            _child2 = new TestNode(splitBoundsTop, BSP, _randomService);
        }
        else
        {
            RectInt splitBoundsLeft = new RectInt(_bound.xMin, _bound.yMin, _bound.width / 2, _bound.height);
            RectInt splitBoundsRight = new RectInt(_bound.xMin + _bound.width / 2, _bound.yMin, _bound.width / 2, _bound.height);

            _child1 = new TestNode(splitBoundsLeft, BSP, _randomService);
            _child2 = new TestNode(splitBoundsRight, BSP, _randomService);
        }
    }

}

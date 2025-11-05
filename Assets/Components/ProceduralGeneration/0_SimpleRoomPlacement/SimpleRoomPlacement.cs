using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using System.Collections.Generic;
using VTools.Utility;
using Unity.VisualScripting;
using System;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        [NonSerialized]private List<RectInt> roomList = new List<RectInt>();
        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            int minRoomSize = 2;


            for (int i = 0; i < _maxSteps; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();

                PlacedRoom();
                PlaceCorridors();
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);

            }
            
            // Final ground building.
            BuildGround();
        }

        private void PlacedRoom()
        {
            RectInt room = GetRandomRoom();

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


        private void PlaceCorridors()
        {
            for(int i = 0; i < roomList.Count - 1; i++){
                Vector2Int roomStart = roomList[i].GetCenter();
                Vector2Int roomTarget = roomList[i + 1].GetCenter();

                int xDir = roomTarget.x > roomStart.x ? 1 : -1;
                int yDir = roomTarget.y > roomStart.y ? 1 : -1;

                while (roomStart.x != roomTarget.x)
                {
                    if(Grid.TryGetCellByCoordinates(roomStart.x, roomStart.y, out Cell cell))
                    {
                        AddTileToCell(cell, CORRIDOR_TILE_NAME, false);
                    }
                    roomStart.x += xDir;
                }
                while (roomStart.y != roomTarget.y)
                {
                    if (Grid.TryGetCellByCoordinates(roomStart.x, roomStart.y, out Cell cell))
                    {
                        AddTileToCell(cell, CORRIDOR_TILE_NAME, false);
                    }
                    roomStart.y += yDir;
                }
            }
        }

        private RectInt GetRandomRoom()
        {
            int xPos = RandomService.Range(0, Grid.Width);
            int yPos = RandomService.Range(0, Grid.Lenght);

            int sizeX = RandomService.Range(2, 10);
            int sizeY = RandomService.Range(2, 10);

            RectInt room = new RectInt(xPos, yPos, sizeX, sizeY);
            return room;
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
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//RandomWalkParameter Value use only SmallDungeon or SmallRoom 

public class CorridorFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField] int corridorLength = 14, corridorCount = 5;
    [SerializeField][Range(0.1f, 1)] private float roomPercent = 1f;

    // private Dictionary<Vector2Int, HashSet<Vector2Int>> roomsDictionary
    //     = new Dictionary<Vector2Int, HashSet<Vector2Int>>();

    // private HashSet<Vector2Int> floorPositions, corridorPositions;

    // private List<Color> roomColors = new List<Color>();
    // [SerializeField]
    // private bool showRoomGizmo = false, showCorridorsGizmo;


    protected override void RunProceduralGeneration()
    {
        CorridorFirstGeneration();
    }

    private void CorridorFirstGeneration()
    {
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();
        CreateCorridors(floorPositions, potentialRoomPositions);
        HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);

        List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);

        CreateRoomsAtDeadEnd(deadEnds, roomPositions);
        //Draw Rooms
        floorPositions.UnionWith(roomPositions);

        tilemapVisualizer.PaintFloorTiles(floorPositions);
        //Draw Walls
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);

        //PlayerController.instance.transform.position = (Vector3Int)floorPositions.First();
    }

    private void CreateRoomsAtDeadEnd(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomFloors)
    {
        var deadEndsCount = deadEnds.Count;
        for (int i = 0; i < deadEndsCount; i++)
        {
            Vector2Int position = deadEnds[i];
            if (roomFloors.Contains(position) == false)
            {
                var room = RunRandomWalk(randomWalkParameters, position);
                roomFloors.UnionWith(room);
            }
        }
    }

    private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
    {
        List<Vector2Int> deadEnds = new List<Vector2Int>();
        foreach (var position in floorPositions)
        {
            int neighborsCount = 0;
            var cardinalDirectionsListCount = Direction2D.cardinalDirectionsList.Count;
            for (int i = 0; i < cardinalDirectionsListCount; i++)
            {
                Vector2Int direction = Direction2D.cardinalDirectionsList[i];
                if (floorPositions.Contains(position + direction))
                    neighborsCount++;
            }
            if (neighborsCount == 1)
                deadEnds.Add(position);
        }
        return deadEnds;
    }

    private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
    {
        HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();
        int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);

        List<Vector2Int> roomsToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();
        // ClearRoomData();
        // foreach (var roomPosition in roomsToCreate)
        // {
        //     var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);

        //     SaveRoomData(roomPosition, roomFloor);
        //     roomPositions.UnionWith(roomFloor);
        // }
        // return roomPositions;
        var roomsToCreateCount = roomsToCreate.Count;
        for (int i = 0; i < roomsToCreateCount; i++)
        {
            Vector2Int roomPosition = roomsToCreate[i];
            var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);
            roomPositions.UnionWith(roomFloor);
        }
        return roomPositions;
    }
    // private void ClearRoomData()
    // {
    //     roomsDictionary.Clear();
    //     roomColors.Clear();
    // }
    // private void SaveRoomData(Vector2Int roomPosition, HashSet<Vector2Int> roomFloor)
    // {
    //     roomsDictionary[roomPosition] = roomFloor;
    //     roomColors.Add(UnityEngine.Random.ColorHSV());
    // }

    private void CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> potentialRoomPositions)
    {
        var currentPosition = startPosition;
        potentialRoomPositions.Add(currentPosition);

        for (int i = 0; i < corridorCount; i++)
        {
            var corridor = ProceduralGenerationAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);
            currentPosition = corridor[corridor.Count - 1];
            potentialRoomPositions.Add(currentPosition);
            floorPositions.UnionWith(corridor);
            //Debug.Log(corridor.First());
            // ObjectPooler.SpawnFromPool("SpotLight2D", ((Vector3Int)corridor[i]));
        }
        //corridorPositions = new HashSet<Vector2Int>(floorPositions);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimsAlgorithm : MazeLogic
{
    // Start is called before the first frame update

    public class MapLocation
    {
        public int x;
        public int z;

        public MapLocation(int _x, int _z)
        {
            x = _x;
            z = _z;

        }
    }

    
    public class Mazelogic : MonoBehaviour
    {
        public int width = 30;
        public int depth = 30;
        public List<GameObject> Cube;
        public byte[,] map;
    }

    public override void GenerateMaps()
    {
        int x = 2;
        int z = 2;
        map[x, z] = 0;

        List<MapLocation> walls = new List<MapLocation>();
        walls.Add(new MapLocation(x + 1, z));
        walls.Add(new MapLocation(x - 1, z));
        walls.Add(new MapLocation(x, z + 1));
        walls.Add(new MapLocation(x, z - 1));

        int countloops = 0;
        while (walls.Count > 0 && countloops < 5000)
        {
            int rwall = Random.Range(0, walls.Count);
            x = walls[rwall].x;
            z = walls[rwall].z;
            walls.RemoveAt(rwall);
            if (CountSquareNeighbours(x, z) == 1)
            {
                map[x, z] = 0;
                walls.Add(new MapLocation(x + 1, z));
                walls.Add(new MapLocation(x - 1, z));
                walls.Add(new MapLocation(x, z + 1));
                walls.Add(new MapLocation(x, z - 1));
            }

            countloops++;
        }
    }
   
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

public class MazeLogic : MonoBehaviour
{
    public static MazeLogic instance;
    public int width = 30;
    public int depth = 30;
    public int scale = 6;
    public byte[,] map;
    public GameObject Character;
    public GameObject Enemy;
    public GameObject Bom;
    public GameObject Car;
    public GameObject Road;
    public int RoadCount = 3;
    public int CarCount = 3;
    public int BombCount = 5;
    public int EnemyCount = 3;
    public int RoomCount = 3;
    public int RoomMinSize = 6;
    public int RoomMaxSize = 10;

    // Konstanta atau variabel statis untuk jenis sel
    public const byte EmptyCell = 0;
    public const byte WallCell = 1;
    public const byte RoomCell = 2;
    public const byte HoleCell = 3;

    GameObject[,] BuildingList;
    public NavMeshSurface surface;
    public List<GameObject> Cube;

    // Start is called before the first frame update
    void Start()
    {

        InitialiseMap();
        GenerateMaps();
        AddRooms(RoomCount, RoomMinSize, RoomMaxSize);
        DrawMaps();
        PlaceCharacter();
        PlaceEnemy();
        PlaceBomb();
        PlaceCar();
        Placebrokenroad();
        surface.BuildNavMesh();
        instance = this;
    }

    void InitialiseMap()
    {
        map = new byte[width, depth];
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                map[x, z] = 1;
            }
        }
    }

    public virtual void GenerateMaps()
    {
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int randomValue = Random.Range(0, 100);

                // Modifikasi logika sesuai kebutuhan
                if (randomValue < 50)
                {
                    map[x, z] = MazeLogic.EmptyCell;
                }
                else if (randomValue < 80)
                {
                    map[x, z] = MazeLogic.WallCell;
                }
                else
                {
                    map[x, z] = MazeLogic.HoleCell;
                }
            }
        }
    }

    void DrawMaps()
    {
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                if (map[x, z] == 1)
                {
                    Vector3 pos = new Vector3(x * scale, 0, z * scale);
                    GameObject wall = Instantiate(Cube[Random.Range(0, Cube.Count)], pos, Quaternion.identity);
                    wall.transform.localScale = new Vector3(scale, scale, scale);
                    wall.transform.position = pos;
                }
            }
        }
    }

    public int CountSquareNeighbours(int x, int z)
    {
        int count = 0;
        if (x <= 0 || x >= width - 1 || z <= 0 || z >= depth - 1) return 5;
        if (map[x - 1, z] == 0) count++;
        if (map[x + 1, z] == 0) count++;
        if (map[x, z - 1] == 0) count++;
        if (map[x, z + 1] == 0) count++;
        return count;
    }

    public bool IsInBounds(int x, int z)
    {
        return x >= 0 && x < width && z >= 0 && z < depth;
    }

    public byte GetCellType(int x, int z)
    {
        if (IsInBounds(x, z))
        {
            return map[x, z];
        }
        return 0;
    }

    public virtual void PlaceCharacter()
    {
        bool PlayerSet = false;
        for (int i = 0; i < depth; i++)
        {
            int x = Random.Range(0, width);
            int z = Random.Range(0, depth);

            if (map[x, z] == 0 && !PlayerSet)
            {
                // Debug.Log("placing character");
                PlayerSet = true;
                Character.transform.position = new Vector3(x * scale, 0, z * scale);
            }
            else if (PlayerSet)
            {
                // Debug.Log("already Placing character");
                return;
            }
        }
    }

    public virtual void PlaceEnemy()
    {
        int EnemySet = 0;
        for (int i = 0; i < depth; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int x = Random.Range(0, width);
                int z = Random.Range(0, depth);

                if (map[x, z] == 0 && EnemySet != EnemyCount)
                {
                    // Debug.Log("placing Enemy");
                    EnemySet++;
                    GameObject enemy = Instantiate(Enemy, new Vector3(x * scale, 0, z * scale), Quaternion.identity);
                }
                else if (EnemySet == EnemyCount)
                {
                    // Debug.Log("already Placing All the Enemy");
                    return;
                }
            }
        }
    }

    public virtual void PlaceBomb()
    {
        int BombSet = 0;
        for (int i = 0; i < depth; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int x = Random.Range(0, width);
                int z = Random.Range(0, depth);
                if (map[x, z] == 0 && BombSet != BombCount)
                {
                    // Debug.Log("placing Bomb");
                    BombSet++;
                    Instantiate(Bom, new Vector3(x * scale, -3, z * scale), Quaternion.identity);
                }
                else if (BombSet == BombCount)
                {
                    // Debug.Log("already Placing All the Bombs");
                    return;
                }
            }
        }
    }

    public virtual void PlaceCar()
    {
        int CarSet = 0;
        for (int i = 0; i < depth; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int x = Random.Range(0, width);
                int z = Random.Range(0, depth);

                if (map[x, z] == 0 && CarSet != CarCount)
                {
                    // Debug.Log("placing Enemy");
                    CarSet++;
                    Instantiate(Car, new Vector3(x * scale, 0, z * scale), Quaternion.identity);
                }
                else if (CarSet == CarCount)
                {
                    // Debug.Log("already Placing All the Enemy");
                    return;
                }
            }
        }
    }

    public virtual void Placebrokenroad()
    {
        int RoadSet = 0;
        for (int i = 0; i < depth; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int x = Random.Range(0, width);
                int z = Random.Range(0, depth);

                if (map[x, z] == 0 && RoadSet != RoadCount)
                {
                    // Debug.Log("placing Enemy");
                    RoadSet++;
                    Instantiate(Road, new Vector3(x * scale, -3.21f, z * scale), Quaternion.Euler(-87.146f, 0f, 0f));
                }
                else if (RoadSet == RoadCount)
                {
                    // Debug.Log("already Placing All the Enemy");
                    return;
                }
            }
        }
    }

    public virtual void AddRooms(int Count, int minSize, int maxSize)
    {
        for (int c = 0; c < Count; c++)
        {
            int startX = Random.Range(3, width - 3);
            int startZ = Random.Range(3, depth - 3);
            int roomWidth = Random.Range(minSize, maxSize);
            int roomDepth = Random.Range(minSize, maxSize);

            for (int x = startX; x < width - 3 && x < startX + roomWidth; x++)
            {
                for (int z = startZ; z < depth - 3 && z < startZ + roomDepth; z++)
                {
                    map[x, z] = 2;
                }
            }
        }
    }
}

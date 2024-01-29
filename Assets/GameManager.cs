using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject roadPrefab;
    public GameObject lamboPrefab;
    public List<GameObject> buildingPrefabs;
    public MeshRenderer plane;

    public GameObject pantsConfettiPrefab;
    public AttackSchema pantsingAttack;

    Dictionary<(int, int), GameObject> map = new();
    public static int gridSize = 10;
    public static float buildingSize = 2.1f;
    float buildiingScale = 5;

    int buildingsPerBlock = 4;
    int roadWidth = 2;

    float roadLength = 20f;

    int roadStart = -20+2;

    int RotationForBuildingPosition(int i, int j) {
        var x = (i%(roadWidth+buildingsPerBlock)+(roadWidth+buildingsPerBlock))%(roadWidth+buildingsPerBlock);
        var z = (j%(roadWidth+buildingsPerBlock)+(roadWidth+buildingsPerBlock))%(roadWidth+buildingsPerBlock);
        List<int> choices = new();
        if (x == roadWidth) {
            choices.Add(-90);
        } else if (x == roadWidth+buildingsPerBlock-1) {
            choices.Add(90);
        }
        if (z == roadWidth) {
            choices.Add(180);
        } else if (z == roadWidth+buildingsPerBlock-1) {
            choices.Add(0);
        }
        if (choices.Count == 0) choices.Add(0);
        return choices[(int) Mathf.Floor(Random.value*choices.Count)];
    }

    void Start()
    {
        instance = this;
        for (int i = -gridSize; i < gridSize; i++) {
            for (int j = -gridSize; j < gridSize; j++) {
                if (
                    (j%(roadWidth+buildingsPerBlock)+(roadWidth+buildingsPerBlock))%(roadWidth+buildingsPerBlock) < roadWidth || 
                    (i%(roadWidth+buildingsPerBlock)+(roadWidth+buildingsPerBlock))%(roadWidth+buildingsPerBlock)  < roadWidth
                ) 
                continue;
                
                var sample = Random.value;
                sample = Mathf.Pow(sample, (Mathf.Abs(i+j)%15)/4+1); 
                map[(i, j)] = buildingPrefabs[(int) Mathf.Floor(sample*buildingPrefabs.Count)];
            }            
        }

        GenerateBuildings();
        GenerateRoads();
    }

    void GenerateBuildings() {
        foreach (var pair in map) {
            var go= GameObject.Instantiate(pair.Value, new Vector3(pair.Key.Item1*buildingSize, 0, pair.Key.Item2*buildingSize), Quaternion.Euler(-90,RotationForBuildingPosition(pair.Key.Item1,pair.Key.Item2),0));
        }
    }
    void GenerateRoads() {
        for (int i = roadStart; i < gridSize; i += roadWidth+buildingsPerBlock) {
            var x = (i+ (roadWidth-1)/2f)*buildingSize;
            for (float z = roadLength/2 - Mathf.Round(gridSize*buildingSize/roadLength)*roadLength; z < gridSize*buildingSize; z += roadLength) {
                var road = GameObject.Instantiate(roadPrefab, new Vector3(z, 0, x), Quaternion.identity);
                if (Random.value < 0.5) {
                    var lambo = GameObject.Instantiate(lamboPrefab, road.transform.position + new Vector3(0, 0, roadWidth*gridSize/4), Quaternion.identity);
                }
            }
        }
        for (int i = roadStart; i < gridSize; i += roadWidth+buildingsPerBlock) {
            var x = (i+ (roadWidth-1)/2f)*buildingSize;
            for (float z = roadLength/2 - Mathf.Round(gridSize*buildingSize/roadLength)*roadLength; z < gridSize*buildingSize; z += roadLength) {
                GameObject.Instantiate(roadPrefab, new Vector3(x, 0.0001f, z), Quaternion.Euler(0, 90, 0));
            }
        }       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CanWalk(float x, float z) {
        return !map.ContainsKey(((int) Mathf.Round(x/buildingSize), (int) Mathf.Round(z/buildingSize)));
    }
    public bool CanWalk(Vector3 v) => CanWalk(v.x, v.z);
    public bool IsOnRoad(float x, float z) {
        x /= buildingSize;
        x -= (roadWidth-1)/2f;
        z /= buildingSize;
        z -= (roadWidth-1)/2f;
        var xDiff = Mathf.Abs(x - Mathf.Round(x));
        var zDiff = Mathf.Abs(z - Mathf.Round(z));
        return ((Mathf.Round(x) - roadStart)%(roadWidth+buildingsPerBlock) == 0 && xDiff < roadWidth*0.5f*1.2f) ||
            ((Mathf.Round(z) - roadStart)%(roadWidth+buildingsPerBlock) == 0 && zDiff < roadWidth*0.5f*1.2f);
    }
    public bool IsOnRoad(Vector3 v) => IsOnRoad(v.x, v.z);
    public bool IsOnSidewalk(float x, float z) => !IsOnRoad(x, z) && CanWalk(x, z);
    public bool IsOnSidewalk(Vector3 v) => IsOnSidewalk(v.x, v.z);

    public delegate bool SampleFunction(Vector3 v);

    public Vector3 SampleMap(SampleFunction func) {
        Vector3 pos;
        do {
            pos = new Vector3(Random.value*2-1, 0, Random.value*2-1)*gridSize*buildingSize;
        } while (!func(pos));
        return pos;
    }
}

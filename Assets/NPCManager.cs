using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public CameraController cam;
    public GameObject baseGuyPrefab;
    public GameObject baseGirlPrefab;
    public GameObject copPrefab;

    public List<GameObject> girlPants;
    public List<GameObject> pants;

    public List<GameObject> girlTops;
    public List<GameObject> tops;

    public List<GameObject> girlHair;

    public static Dictionary<string, GameObject> thoughtPrefabs = new();
    public static NPCManager instance;

    public NPCPool pool;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        NPCPool.baseGuyPrefab = baseGuyPrefab;
        NPCPool.baseGirlPrefab = baseGirlPrefab;
        NPCPool.copPrefab = copPrefab;
        
        NPCPool.tops = tops;
        NPCPool.pants = pants;
        
        NPCPool.girlTops = girlTops;        
        NPCPool.girlPants = girlPants;

        NPCPool.girlHair = girlHair;

        pool = new NPCPool();

        thoughtPrefabs["Alarm"] = Resources.Load<GameObject>("Thoughts/Alarm");
        thoughtPrefabs["Angry"] = Resources.Load<GameObject>("Thoughts/Angry");
        thoughtPrefabs["Approve"] = Resources.Load<GameObject>("Thoughts/Approve");
        thoughtPrefabs["Awesome"] = Resources.Load<GameObject>("Thoughts/Awesome");
        thoughtPrefabs["Confusion"] = Resources.Load<GameObject>("Thoughts/Confusion");
        thoughtPrefabs["Impact"] = Resources.Load<GameObject>("Thoughts/Impact");
        thoughtPrefabs["Laughing"] = Resources.Load<GameObject>("Thoughts/Laughing");
        thoughtPrefabs["Love"] = Resources.Load<GameObject>("Thoughts/Love");
        thoughtPrefabs["Punch"] = Resources.Load<GameObject>("Thoughts/Punch");
        thoughtPrefabs["Swear"] = Resources.Load<GameObject>("Thoughts/Swear");
        thoughtPrefabs["Why"] = Resources.Load<GameObject>("Thoughts/Why");
        thoughtPrefabs["WTF"] = Resources.Load<GameObject>("Thoughts/WTF");

        cam.player = NPCPool.NewPlayer().GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        while (pool.activeNPCs < 157) {
            var npc = Random.value > 0.1 ? pool.Get() : pool.GetCop();
            npc.transform.position = GameManager.instance.SampleMap(GameManager.instance.IsOnSidewalk);
        }
    }
    public delegate float CostFunction(Vector3 start, Vector3 end);
    public static float NormalDistance(Vector3 start, Vector3 end) => (start-end).magnitude;
    public static float NPCWalkCost(Vector3 start, Vector3 end) {
        var dir = start-end;
        if (GameManager.instance.IsOnRoad(end) && dir.x != 0 && dir.z != 0){
            return 1000;
        } else return NormalDistance(start, end);
    }

    public delegate bool ValidFunction(Vector3 v);

    public static List<Vector3> GetNeighbors(Vector3 v, float step) {
        var res = new List<Vector3>();
        for (int i = -1; i <=1; i++) {
            for (int j = -1; j <=1; j++) {
                if (i == 0 && j == 0) continue;
                res.Add(new Vector3(i, 0, j)*step + v);
            }   
        }
        return res;
    }
    public static List<Vector3> GetPath(Vector3 start, Vector3 end, CostFunction cost = null, ValidFunction valid = null, float step = 0.25f, List<(Vector3, float, List<int>)> viable = null, List<Vector3> dead = null) {
        if (viable == null) viable = new(){(start, 0, new())};
        if (dead == null) dead = new();
        if (cost == null) cost = NormalDistance;
        if (valid == null) valid = GameManager.instance.CanWalk;

        // sort most viable, return path if reached within step
        viable.Sort((v1, v2) => (int) Mathf.Sign(
            (v1.Item2 + cost(v1.Item1, end)) - (v2.Item2 + cost(v2.Item1, end))
        ));
        if (NormalDistance(viable[0].Item1, end) < step) {
            var res = viable[0].Item3.ConvertAll(i => dead[i]);
            res.Add(end);
            return res;
        }


        var neighbors = GetNeighbors(viable[0].Item1, step);
        var currentCost = viable[0].Item2;
        dead.Add(viable[0].Item1);


        foreach (var n in neighbors) {
            if (valid(n)) {
                if (dead.Contains(n)) continue;
                if (viable.Find(x => x.Item1 == n) != default) continue;
                viable.Add((
                    n, 
                    currentCost + cost(n, viable[0].Item1),
                    new List<int>(viable[0].Item3)
                ));
                viable[viable.Count-1].Item3.Add(dead.Count-1);
            }
            else dead.Add(n);
        }
        viable.RemoveAt(0);

        if (viable.Count == 0) return null;
        return GetPath(start, end, cost, valid, step, viable, dead);
    }
}

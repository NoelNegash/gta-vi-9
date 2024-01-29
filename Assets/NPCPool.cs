using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCPool
{
    public static GameObject baseGuyPrefab, baseGirlPrefab;
    public static GameObject copPrefab;
    public static List<GameObject> girlPants;
    public static List<GameObject> pants;

    public static List<GameObject> girlTops;
    public static List<GameObject> tops;

    public static List<GameObject> girlHair;


    public int activeNPCs = 0;
    List<GameObject> npcs = new();

    public static GameObject NewPlayer() {
        var player = NewModel();
        player.AddComponent<Player>();
        return player;
    }
    public static GameObject NewNPC() {
        var npc = NewModel();
        npc.AddComponent<NPCController>();
        return npc;
    }
    public static GameObject NewCop() {
        var npc = GameObject.Instantiate(copPrefab);
        npc.AddComponent<NPCController>().cop = true;
        return npc;
    }

    public static GameObject NewModel() {
        var woman = Random.value < 0.69;
        var npc =  GameObject.Instantiate(!woman ? baseGuyPrefab : baseGirlPrefab);


        
        var p = GameObject.Instantiate(pants[(int) Mathf.Floor(Random.value*pants.Count)]);
        var top = (int) Mathf.Floor(Random.value*tops.Count);
        var t = GameObject.Instantiate(tops[top]);
        
        if (woman) {
            if (Random.value > 0.69) {
                var hair = girlHair[(int) Mathf.Floor(Random.value*girlHair.Count)];
                MergeMesh(
                    npc.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>(),
                    hair.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>()
                );

            }
            if (true || Random.value > 0.5) {
                GameObject.Destroy(t);
                t = GameObject.Instantiate(girlTops[(int) Mathf.Floor(Random.value*girlTops.Count)]);
            }
            if (true || Random.value > 0.5) {
                GameObject.Destroy(p);
                p = GameObject.Instantiate(girlPants[(int) Mathf.Floor(Random.value*girlPants.Count)]);
            }

        }

        MergeMesh(
            npc.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>(),
            t.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>()
        );
        var (nakedMesh, nakedMaterials) = MergeMesh(
            npc.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>(),
            p.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>()
        );

        var naked = npc.AddComponent<NakedInfo>();
        naked.mesh = nakedMesh;
        naked.materials = nakedMaterials;

        GameObject.Destroy(p);
        GameObject.Destroy(t);
        return npc;
    }



    public List<GameObject> GetAttackables(Vector3 pos, Vector3 direction, bool melee, float dist, float angle) {
        var res = npcs.FindAll(n => n.active && (pos-n.transform.position).magnitude < dist && (melee || Vector3.Angle(n.transform.position-pos, direction) < angle/2));
        res.Sort(( n1, n2 ) => (int) Mathf.Sign((pos-n1.transform.position).magnitude - (pos-n2.transform.position).magnitude));
        return res;
    }



    static (Mesh, List<Material>) MergeMesh(SkinnedMeshRenderer r1, SkinnedMeshRenderer r2) {
        Mesh m1 = r1.sharedMesh;
        Mesh m2 = r2.sharedMesh;
        Mesh m = new Mesh();
        m.Clear();
        MergeMesh(m, m1);
        MergeMesh(m, m2);


        var oldMaterials = new List<Material>(r1.sharedMaterials);
        var materials = new List<Material>(oldMaterials);
        foreach(var mat in r2.sharedMaterials) materials.Add(mat);

        r1.sharedMesh = m;

        r1.sharedMaterials = materials.ToArray();

        return (m1, oldMaterials);
    }
    static void MergeMesh(Mesh m1, Mesh m2) {
        var m1Vertices = new List<Vector3>(m1.vertices);
        var m2Vertices = new List<Vector3>(m2.vertices);
        var initialVertices = m1Vertices.Count;

        var m1UVs = new List<Vector2>(m1.uv);
        var m2UVs = new List<Vector2>(m2.uv);

        var m1AllBoneWeights = new List<BoneWeight>(m1.boneWeights);

        foreach (var v in m2Vertices) m1Vertices.Add(v);
        m1.vertices = m1Vertices.ToArray();

        foreach (var v in m2UVs) m1UVs.Add(v);
        m1.uv = m1UVs.ToArray();

        var offset = initialVertices == 0 ? -1 : 0;
        var subMeshCount = m1.subMeshCount+offset;
        m1.subMeshCount += m2.subMeshCount+offset;
        for (int i = 0; i < m2.subMeshCount; i++) {
            var triangles = m2.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j++) {
                triangles[j] += initialVertices;
            }
            m1.SetTriangles(triangles, i+subMeshCount);
        }

        foreach (var b in m2.boneWeights) m1AllBoneWeights.Add(b);
        m1.boneWeights = m1AllBoneWeights.ToArray();
        m1.bindposes = m2.bindposes;

        /*
        m1.GetAllBoneWeights(m1AllBoneWeights);
        var m2AllBoneWeights = new List<BoneWeight>();
        m2.GetAllBoneWeights(m2AllBoneWeights);
        foreach (var b in m2AllBoneWeights) m1AllBoneWeights.Add(b);
        var m1BonesPerVertex = new List<byte>(m1.GetBonesPerVertex());
        var m2BonesPerVertex = m2.GetBonesPerVertex();
        foreach (var b in m2BonesPerVertex) m1BonesPerVertex.Add(b);
        m1.SetBoneWeights(m1AllBoneWeights.ToArray());
        m1.SetBonesPerVertex(m1BonesPerVertex);*/
    }

    public GameObject Get() {
        GameObject npc;
        npc = npcs.Find(n => !n.active);
        if (npc == null) {
            npc = NewNPC();
            npc.GetComponent<NPCController>().Restart();
            npcs.Add(npc);
        }

        npc.SetActive(true);
        activeNPCs++;
        return npc;
    }
    public GameObject GetCop() {
        GameObject npc;
        npc = npcs.Find(n => !n.active && n.GetComponent<NPCController>().cop);
        if (npc == null) {
            npc = NewCop();
            npc.GetComponent<NPCController>().Restart();
            npcs.Add(npc);
        }

        npc.SetActive(true);
        activeNPCs++;
        return npc;
    }

    public void Release(GameObject npc) {
        npc.SetActive(false);
        activeNPCs--;
        // todo handle any dependencies down the line
    }
}

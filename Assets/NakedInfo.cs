using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NakedInfo : MonoBehaviour
{
    public Mesh mesh;
    public List<Material> materials;

    public void GetNaked() {
        var skin = transform.GetChild(1).GetComponent<SkinnedMeshRenderer>();
        skin.sharedMesh = mesh;
        skin.sharedMaterials = materials.ToArray();
    }
}

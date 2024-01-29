using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dies : MonoBehaviour
{
    public float time;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Die());
    }

    IEnumerator Die() {
        yield return new WaitForSeconds(time);
        GameObject.Destroy(gameObject);
    }
}

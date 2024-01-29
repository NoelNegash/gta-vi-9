using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Player player;
    Vector3 anchor = new Vector3(0, 0.7f, -1f);
    float lookUp = 0;
    float lookRange = 45;
    void Start()
    {
        
    }

    void FixedUpdate()
    {
        lookUp -= Input.GetAxis("Mouse Y")*2.5f;
        lookUp = Mathf.Clamp(lookUp, -lookRange, lookRange);


        var newPosition = Vector3.Lerp(transform.position, player.transform.rotation * anchor + player.transform.position, 0.046f);
        if (GameManager.instance.CanWalk(newPosition-transform.right/3) && GameManager.instance.CanWalk(newPosition+transform.right/3)) transform.position = newPosition;
        else {
            transform.position = Vector3.Lerp(transform.position, newPosition, 0.03f);
        }
        transform.rotation = Quaternion.LookRotation(player.transform.position-transform.position + Vector3.up/2, Vector3.up) * Quaternion.Euler(lookUp, 0, 0);
    }
}

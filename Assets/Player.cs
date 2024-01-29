using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BaseEntity
{
    static float WALK_SPEED = .7f * 1.5f;
    static float RUN_SPEED = .7f * 1.5f * 2.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void FixedUpdate()
    {

        if (GameManager.instance.IsOnRoad(transform.position)) {
            GameManager.instance.plane.material.color = new Color(255,0,0);
        } else {
            GameManager.instance.plane.material.color = new Color(255,255,255);
        }

        animator = GetComponent<Animator>();
        var m_CurrentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
        var clipName = m_CurrentClipInfo[0].clip.name;

        if (!inControl || currentAttack != null) {

        } else {

            var speed = Input.GetKey(KeyCode.LeftShift) ? RUN_SPEED : WALK_SPEED;

            if (Input.GetKeyDown(KeyCode.Space)) {
                StartAttack(GameManager.instance.pantsingAttack);
            } else {

                Quaternion heading;
                var input = new Vector3(Input.GetAxis("Horizontal"), 0, Mathf.Max(0,Input.GetAxis("Vertical")));
                if (input.magnitude != 0) {
                    heading = transform.rotation*Quaternion.LookRotation(input, Vector3.up);
                } else heading = transform.rotation;
                transform.rotation = Quaternion.Euler(0, Input.GetAxis("Mouse X")*3, 0) * Quaternion.Lerp(transform.rotation, heading, 0.02f);
                if (input.magnitude != 0) {
                    var newPosition = transform.position + transform.rotation * Vector3.forward * speed*Time.deltaTime;
                    if (CanWalk(newPosition)) {
                        transform.position = newPosition;

                        if (Input.GetKey(KeyCode.LeftShift)) animator.SetTrigger("Fast Run");
                        else animator.SetTrigger("Walk");
                    } else {
                        animator.SetTrigger("Walk");
                    }
                } else {
                    animator.SetTrigger("Idle");
                }

            }

        }
    }


    List<Collider> colliders = new();

    void OnTriggerEnter(Collider other) {
        CapsuleCollider capsuleCollider = other as CapsuleCollider;
        if (!colliders.Contains(other) && capsuleCollider != null)
        {
            float radius = capsuleCollider.radius;
            if (radius < 1) {
                // inner trigger
                //Debug.Log(1);
            } else {
                // outer trigger
                //Debug.Log(2);
            }   
        }

        colliders.Add(other);
    }

    void OnTriggerExit(Collider c) {
        colliders.Remove(c);
    }
}

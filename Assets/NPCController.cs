using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCState {
    CivilainWander, CivilainPhone, CivilianPantsed, Chasing, Dancing
}


public class NPCController : BaseEntity
{
    static float WALK_SPEED = .7f * 0.5f;
    static float RUN_SPEED = .7f * 0.5f * 2.5f;
    public bool cop;

    NPCState state;
    List<Vector3> path;


    static float THOUGHT_LENGTH = 2f;
    float lastThought = 0;

    public void Think(string thought, bool force = false) {
        if (!force && (Time.time-lastThought < THOUGHT_LENGTH || !NPCManager.thoughtPrefabs.ContainsKey(thought))) return;
        lastThought = Time.time;

        var go = GameObject.Instantiate(NPCManager.thoughtPrefabs[thought]);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.up*2.2f;
        go.AddComponent<Dies>().time = THOUGHT_LENGTH;
    }

    void Start()
    {
        Restart();
    }
    public void Restart() {
        state = NPCState.CivilainWander;
        path = new();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var m_CurrentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
        var clipName = m_CurrentClipInfo[0].clip.name;

        if (!inControl || currentAttack != null) {

        } else {
            switch(state) {
                case NPCState.CivilainWander:
                    animator.SetTrigger("Walk");
                    if (path != null && path.Count == 0) {
                        // find new place to walk to 
                        if (Random.value > 0.1) {
                            try {
                                path = NPCManager.GetPath(
                                    transform.position, 
                                    GameManager.instance.SampleMap((v) => GameManager.instance.IsOnSidewalk(v) && (v-transform.position).magnitude < 8),
                                    NPCManager.NPCWalkCost,
                                    CanWalk);
                            } catch {}   
                        } else TalkOnPhone();
                    } else FollowPath();
                    break;

                case NPCState.CivilainPhone:
                    if (clipName != "Phone") state = NPCState.CivilainWander;
                    break;
                case NPCState.CivilianPantsed:
                    if (clipName != "Pantsed") {
                        if (enemy == null) {
                            state = NPCState.CivilainWander;
                        } else {
                            state = NPCState.Chasing;
                            actuallyChasing = false;
                            chaseStarted = Time.time;
                        }
                    }
                    break;
                case NPCState.Chasing:
                    if (actuallyChasing) {
                        animator.SetTrigger("Fast Run");
                        Think(Random.value > 0.5 ? "Angry" : "Punch");
                        if (path == null || path.Count == 0 || Time.time - lastEnemyCheck > ENEMY_PATH_REGEN_TIME) {
                            lastEnemyCheck = Time.time;

                            try {
                                path = NPCManager.GetPath(
                                    transform.position, 
                                    enemy.transform.position,
                                    NPCManager.NormalDistance,
                                    CanWalk);
                                Debug.Log(path.Count);

                            } catch {} 
                        } else if ((transform.position-enemy.transform.position).magnitude > 0.6f) {
                            FollowPath();
                        } else if (!Input.GetKey(KeyCode.LeftShift)) {
                            animator.SetTrigger("Dancing");
                            path = null;
                            state = NPCState.Dancing;
                        }
                        break;

                    } else {
                        if (Time.time-chaseStarted > 2f) {
                            Debug.Log("Actually chasing");
                            Debug.Log((transform.position-enemy.transform.position).magnitude);
                            actuallyChasing = true;
                        }
                    }
                    break;
                case NPCState.Dancing:
                    if (clipName != "Dancing") state = NPCState.CivilainWander;
                    break;
                default:
                    break;
            }

        }

    }
    List<Collider> colliders = new();

    void OnTriggerEnter(Collider other) {
        CapsuleCollider capsuleCollider = other as CapsuleCollider;
        if (!colliders.Contains(other) && capsuleCollider != null)
        {
            var entity = other.GetComponent<NPCController>();
            if (entity == null) return;

            float radius = capsuleCollider.radius;
            if (radius < 1) {
                // inner trigger
                //Debug.Log(1);
            } else {
                if (cop && state != NPCState.Chasing && entity.state == NPCState.Chasing) {
                    var chase = Random.value > 0.5;
                    if (chase) {
                        state = NPCState.Chasing;
                        actuallyChasing = false;
                        chaseStarted = Time.time + 1000;
                        Think(Random.value > 0.5?"Swear":"Confusion");
                    } else {
                        Think(Random.value > 0.5?"Laughing":"Approve");
                    }
                }
            }   
        }

        colliders.Add(other);
    }

    void OnTriggerExit(Collider c) {
        colliders.Remove(c);
    }

    float chaseStarted;
    bool actuallyChasing = false;

    void TalkOnPhone() {
        Think("Impact");
        animator.SetTrigger("Phone");
        state = NPCState.CivilainPhone;
    }


    public void StartChase(BaseEntity e) {
        enemy = e;
        state = NPCState.Chasing;
        actuallyChasing = true;
    }

    BaseEntity enemy;
    float lastEnemyCheck;
    static float ENEMY_PATH_REGEN_TIME = 3f;
    public void Pants(BaseEntity e) {
        var loved = Random.value > 0.69;

        if (!loved) enemy = e;

        RegainControl();
        Think(loved ? "Love" : (Random.value > 0.5 ? "Alarm" : "WTF"));
        GetComponent<NakedInfo>().GetNaked();
        animator.SetTrigger("Pantsed");
        state = NPCState.CivilianPantsed;
    }

    void FollowPath(float dist = -1) {
        if (dist < 0) dist = (state == NPCState.Chasing ? RUN_SPEED : WALK_SPEED)*Time.deltaTime;
        if (dist == 0) return;
        if (path == null || path.Count == 0) return;

        var change = (transform.position-path[0]).magnitude - dist;
        if (change <= 0) {
            transform.position = path[0];
            path.RemoveAt(0);
            FollowPath(-change);
        } else {
            var heading = (path[0]-transform.position).normalized*dist;
            if (heading == Vector3.zero) {
                path.RemoveAt(0);
                return;
            }
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(heading, Vector3.up), 0.055f);
            transform.position += heading;
        }
    }
}

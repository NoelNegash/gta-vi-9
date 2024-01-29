using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    public Animator animator;
    public Attack currentAttack = null;
    public bool inControl = true;


    void Update()
    {
        if (currentAttack != null) {

        }

        transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
    }

    public bool CanWalk(Vector3 v) => GameManager.instance.CanWalk(v + transform.forward/3);

    public void StartAttack(AttackSchema s) {
        animator.ResetTrigger("Walk");
        animator.ResetTrigger("Idle");
        animator.SetTrigger(s.attackerTrigger);

        currentAttack = new Attack();
        var options = NPCManager.instance.pool.GetAttackables(transform.position, transform.forward, s.melee, s.range, s.autoAimAngle);

        if (options.Count == 0) {
            currentAttack.failed = true;
            StartCoroutine(FailAfterAlign(s));
        } else {
            currentAttack.victim = options[0].GetComponent<BaseEntity>();
            currentAttack.victim.LoseControl();
            StartCoroutine(AlignVictim(s));
        }


    }

    public IEnumerator TriggerAttack(AttackSchema s) {

        // confetti
        GameObject.Instantiate(
            GameManager.instance.pantsConfettiPrefab,
            animator.GetBoneTransform(HumanBodyBones.LeftHand).position,
            transform.rotation
        );

        var locals = NPCManager.instance.pool.GetAttackables(transform.position, transform.forward, true, 4f, 360);
        locals.Remove(currentAttack.victim.gameObject);

        foreach(var l in locals) {
            var npc = l.GetComponent<NPCController>();
            if (npc.cop) {
                npc.StartChase(this);
            } else {
                npc.Think(Random.value > 0.5 ? "Laughing" : "Approve");
            }
        }

        // todo refactor 
        (currentAttack.victim as NPCController).Pants(this);
        currentAttack.victim.RegainControl();
        currentAttack = null;
        animator.SetTrigger("Idle");
        yield return null;
    }
    public IEnumerator FailAfterAlign(AttackSchema s) {
        yield return new WaitForSeconds(s.alignmentTime);
        currentAttack = null;
    }
    public IEnumerator AlignVictim(AttackSchema s) {
        float time = s.alignmentTime;

        Vector3 victimPosition = currentAttack.victim.transform.position;
        Quaternion victimRotation = currentAttack.victim.transform.rotation;
        var currentRotation = transform.rotation;

        Quaternion endRotation = Quaternion.LookRotation(victimPosition-transform.position, Vector3.up);

        var victimEndRotation = Quaternion.Euler(0, s.victimAlignment, 0) * endRotation;
        Vector3 victimEndPosition = transform.position + endRotation*Vector3.forward*s.distance;

        while (time > 0) {

            currentAttack.victim.transform.position = Vector3.Lerp(victimEndPosition, victimPosition, time/s.alignmentTime);
            currentAttack.victim.transform.rotation = Quaternion.Lerp(victimEndRotation, victimRotation, time/s.alignmentTime);

            transform.rotation = Quaternion.Lerp(endRotation, currentRotation, time/s.alignmentTime);
            time -= Time.deltaTime;
            yield return null;
        }

        StartCoroutine(TriggerAttack(s));
    }

    public void GetNaked() {
        GetComponent<NakedInfo>().GetNaked();
    }


    public void LoseControl() {
        inControl = false;
        ReleaseControl();
    }

    public void ReleaseControl() {

    }

    public void RegainControl() {
        inControl = true;
    }
}

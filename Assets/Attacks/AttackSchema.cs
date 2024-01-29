using UnityEngine;

[CreateAssetMenu(fileName = "AttackSchema", menuName = "ScriptableObjects/AttackSchema", order = 1)]
public class AttackSchema : ScriptableObject
{
    public float range;
    public float autoAimAngle;

    public float alignmentTime;

    public float attackerAlignment;
    public float victimAlignment;
    public float distance;

    public string attackerTrigger;
    public string victimTrigger;
    public int priority;
    public bool optional;
    public bool melee;
}

public class Attack {
    public AttackSchema schema;
    public Transform target;
    public float time = 0;

    public bool failed = false;
    public BaseEntity victim;
}
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "AttackData", menuName = "Planeto/Attack Data")]
public class AttackData : SerializedScriptableObject
{
    public enum AttackType { PROJECTILE, AURA, DROP, SPAWN };

    [Header("Attack Properties")]
    public AttackType attackType;
    public float cooldown;            // Cooldown tra un'istanza e l'altra
    public float damage;              // Danno inflitto
    public GameObject prefab;
    public int initialPool = 10;

    [Header("Projectile Settings")]
    [ShowIf("@attackType == AttackType.PROJECTILE")]public int projectileNumber;      // Numero di proiettili sparati
    [ShowIf("@attackType == AttackType.PROJECTILE")] public float projectileAngle;     // Angolo di distribuzione
    [ShowIf("@attackType == AttackType.PROJECTILE")] public bool shootBackwards;       // Spara al contrario
    [ShowIf("@attackType == AttackType.PROJECTILE")] public float speed;               // Velocità dei proiettili
    [ShowIf("@attackType == AttackType.PROJECTILE")] public bool hasLifeTime;          // Se hanno un tempo di vita limitato
    [ShowIf("@attackType == AttackType.PROJECTILE")] public float lifeTime;            // Tempo di vita dei proiettili

    [Header("Aura Settings")]
    [ShowIf("@attackType == AttackType.AURA")] public float auraRadius;          // Raggio dell'aura per le zone di danno

    [Header("Drop/Spawn Settings")]
    [ShowIf("@attackType == AttackType.PROJECTILE || attackType == AttackType.SPAWN")] public bool hasActivationEffect;  // Effetto speciale alla fine del ciclo vitale
    [ShowIf("@attackType == AttackType.PROJECTILE || attackType == AttackType.SPAWN")] public float lifeTimeDrop;        // Tempo di vita degli oggetti drop/spawn
}

using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Planeto/Upgrade Data")]
public class UpgradeData : SerializedScriptableObject
{
    public AttackData originalAttack;

    public float cooldownModification = 10f;
    public float damageModification * .01f) = 10f;
    public int projectilesToAdd = 0;
    public int angleModification = 0;
    public float speedIncrease = 10f;
    public float sizeIncrease = 0f;
}

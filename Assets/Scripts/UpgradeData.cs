using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Planeto/Upgrade Data")]
public class UpgradeData : SerializedScriptableObject
{
    public AttackData originalAttack;

    public float cooldownReduction = 10f;
    public float damageIncrease = 10f;
    public int projectilesToAdd = 0;
    public int angleIncrease = 0;
    public float speedIncrease = 10f;
    public float sizeIncrease = 0f;
}

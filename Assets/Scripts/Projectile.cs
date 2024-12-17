using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private EntityType sourceType;

    private void OnTriggerEnter(Collider other)
    {
        var entity = other.GetComponent<Entity>();
        if (entity != null)
        {
            DamageManager.Instance.RegisterDamage(new DamageEvent
            {
                TargetEntityId = entity.EntityId,
                DamageAmount = damage,
                SourceType = sourceType
            });

            gameObject.SetActive(false); // Ricicla o disattiva il proiettile.
        }
    }
}


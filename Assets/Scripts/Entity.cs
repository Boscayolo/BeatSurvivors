using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Pathfinding;
using System.Collections;

public class Entity : MonoBehaviour
{
    public int EntityId { get; private set; }
    public EntityType type;

    [SerializeField] private float health;
    [SerializeField, ShowIf("@type == EntityType.ENEMY")] private float contactDamage; // Danno inflitto al contatto (solo per ENEMY).
    [SerializeField, ShowIf("@type == EntityType.ENEMY")] private AIDestinationSetter destinationSetter;

    // Eventi per gestione dinamica del target
    public static Action<AIDestinationSetter> OnEnemyActivation;
    public Action OnEntityDisabled;

    private void Awake()
    {
        EntityId = GetInstanceID();

        StartCoroutine(RegisterEntity());

        if (type == EntityType.PLAYER)
        {
            // Il player registra l'evento per assegnare il proprio transform ai nemici
            OnEnemyActivation += RegisterToEnemies;
        }
    }

    IEnumerator RegisterEntity()
    {
        yield return new WaitForEndOfFrame();
        EntityManager.Instance.RegisterEntity(this);
    }

    private void OnEnable()
    {
        if (type == EntityType.ENEMY && destinationSetter != null && destinationSetter.target == null)
        {
            // Notifica al sistema che un nuovo nemico è stato attivato
            OnEnemyActivation?.Invoke(destinationSetter);
        }

        StartCoroutine(RegisterEntity());
    }

    private void OnDisable()
    {
        if (type == EntityType.ENEMY)
        {
            // Notifica disattivazione del nemico
            OnEntityDisabled?.Invoke();
        }

        if (type == EntityType.PLAYER)
        {
            // Rimuove il player dall'evento quando è disattivato
            OnEnemyActivation -= RegisterToEnemies;
        }
    }

    private void RegisterToEnemies(AIDestinationSetter targetSetter)
    {
        // Assegna il Transform del player come target per i nemici
        targetSetter.target = this.transform;
        Debug.Log($"Player registered as target for {targetSetter.gameObject.name}");
    }

    private float damageCooldown = 1.0f;
    private float lastDamageTime;

    private void OnTriggerEnter(Collider other)
    {
        if (type == EntityType.ENEMY && Time.time > lastDamageTime + damageCooldown)
        {
            lastDamageTime = Time.time;
            var targetEntity = other.GetComponent<Entity>();
            if (targetEntity != null && targetEntity.type == EntityType.PLAYER)
            {
                DamageManager.Instance.RegisterDamage(new DamageEvent(
                    targetEntityId: targetEntity.EntityId,
                    damageAmount: contactDamage,
                    sourceType: type
                ));
            }
        }
    }

    public void ApplyDamage(DamageEvent damageEvent)
    {
        if (!IsDamageValid(damageEvent.SourceType))
        {
            return; // Ignora danni non validi.
        }

        Debug.Log($"{gameObject.name} received {damageEvent.DamageAmount}");

        health -= damageEvent.DamageAmount;
        if (health <= 0)
        {
            OnDeath();
        }
    }

    private bool IsDamageValid(EntityType sourceType)
    {
        return (type == EntityType.PLAYER && sourceType == EntityType.ENEMY) ||
               (type == EntityType.ENEMY && sourceType == EntityType.PLAYER);
    }

    private void OnDeath()
    {
        gameObject.SetActive(false);
        // Logica aggiuntiva per la morte, es. drop o animazioni.
    }
}

public enum EntityType { PLAYER, ENEMY, COLLECTABLE };

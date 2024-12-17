using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerShooter : SerializedMonoBehaviour
{
    [Header("Attack Configuration")]
    [SerializeField] private List<AttackData> attackDataList;

    [Header("Pooling System")]
    [SerializeField]private Dictionary<GameObject, Queue<GameObject>> projectilePool = new Dictionary<GameObject, Queue<GameObject>>();

    private Dictionary<AttackData, Coroutine> activeCoroutines = new Dictionary<AttackData, Coroutine>();

    private void Start()
    {
        // Avvia le coroutines per ogni abilità
        foreach (var attackData in attackDataList)
        {
            if (!activeCoroutines.ContainsKey(attackData))
            {
                Coroutine coroutine = StartCoroutine(HandleAttackCooldown(attackData));
                activeCoroutines.Add(attackData, coroutine);
            }
        }

        foreach(AttackData attackData in attackDataList) 
        {
            AddAttack(attackData);
        }
    }

    private IEnumerator HandleAttackCooldown(AttackData attackData)
    {
        while (true)
        {
            yield return new WaitForSeconds(attackData.cooldown);

            switch (attackData.attackType)
            {
                case AttackData.AttackType.PROJECTILE:
                    ShootProjectiles(attackData);
                    break;
                case AttackData.AttackType.AURA:
                    TriggerAuraDamage(attackData);
                    break;
                case AttackData.AttackType.DROP:
                    SpawnDrop(attackData);
                    break;
                case AttackData.AttackType.SPAWN:
                    SpawnObjectAroundPlayer(attackData);
                    break;
            }
        }
    }

    private void ShootProjectiles(AttackData attackData)
    {
        float angleStep = attackData.projectileAngle / attackData.projectileNumber;
        float startAngle = attackData.projectileAngle / -2f;

        // Calcola il vettore radiale dal centro della sfera alla posizione del giocatore
        Vector3 toCenter = (transform.position - Attractor.instance.transform.position).normalized;

        // Calcola la direzione tangente locale iniziale
        Vector3 forwardDirection = Vector3.Cross(toCenter, transform.right).normalized;

        // Se shootBackwards è true, inverti la direzione
        if (attackData.shootBackwards)
            forwardDirection = -forwardDirection;

        for (int i = 0; i < attackData.projectileNumber; i++)
        {
            // Calcola l'angolo per ogni proiettile
            float angle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.AngleAxis(angle, toCenter); // Ruota intorno al vettore radiale

            // Calcola la direzione del proiettile ruotando la forwardDirection
            Vector3 projectileDirection = rotation * forwardDirection;

            // Ottieni il proiettile dal pool
            GameObject projectile = GetProjectileFromPool(attackData.prefab);
            projectile.transform.position = transform.position;
            projectile.transform.rotation = Quaternion.LookRotation(projectileDirection);

            // Inizializza il movimento del proiettile
            projectile.GetComponent<Projectile>().InitializeProjectile(
                attackData.prefab,
                projectileDirection,
                attackData.damage,
                attackData.speed,
                attackData.lifeTime
            );
        }
    }

    void AddAttack(AttackData attackData)
    {
        projectilePool.TryAdd(attackData.prefab, new Queue<GameObject>());

        for (int i = 0; i < attackData.initialPool * attackData.projectileNumber; i++)
        {
            GameObject instantiatedProj = Instantiate(attackData.prefab); // Usa il prefab corretto
            projectilePool[attackData.prefab].Enqueue(instantiatedProj);
            instantiatedProj.SetActive(false);
        }
    }


    private void TriggerAuraDamage(AttackData attackData)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackData.auraRadius);
        foreach (var hitCollider in hitColliders)
        {
            var entity = hitCollider.GetComponent<Entity>();
            if (entity != null && entity.type == EntityType.ENEMY)
            {
                DamageManager.Instance.RegisterDamage(new DamageEvent(
                    targetEntityId: entity.EntityId,
                    damageAmount: attackData.damage,
                    sourceType: EntityType.PLAYER
                ));
            }
        }
    }

    private void SpawnDrop(AttackData attackData)
    {
        // Ottieni il prefab specificato nell'AttackData
        GameObject drop = GetProjectileFromPool(attackData.prefab);
        drop.transform.position = transform.position; // Spawn alla posizione del giocatore

        // Configura il ciclo di vita del drop (se definito)
        if (attackData.hasLifeTime)
        {
            drop.GetComponent<Projectile>().InitializeProjectile(attackData.prefab, Vector3.zero, attackData.damage, 0, attackData.lifeTimeDrop);
        }
    }

    float orbitRadius = -1;

    private void SpawnObjectAroundPlayer(AttackData attackData)
    {
        int numObjects = attackData.projectileNumber; // Numero di oggetti da spawnare

        if(orbitRadius == -1) orbitRadius = Vector3.Distance(transform.position, Attractor.instance.transform.position); // Distanza fissa (configurabile)
        float angleStep = 360f / numObjects; // Distribuzione uniforme a 360 gradi

        for (int i = 0; i < numObjects; i++)
        {
            float angle = i * angleStep;
            Vector3 spawnPosition = GetOrbitPosition(transform.position, angle, orbitRadius);

            GameObject orbitObject = GetProjectileFromPool(attackData.prefab);
            orbitObject.transform.position = spawnPosition;
            orbitObject.transform.parent = transform; // Imposta il giocatore come parent
            orbitObject.GetComponent<Projectile>()?.InitializeOrbiting(attackData.prefab, orbitRadius, angle, attackData.speed);
        }
    }

    private Vector3 GetOrbitPosition(Vector3 center, float angleDegrees, float radius)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        return new Vector3(
            center.x + Mathf.Cos(angleRadians) * radius,
            center.y,
            center.z + Mathf.Sin(angleRadians) * radius
        );
    }


    private GameObject GetProjectileFromPool(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is null! Make sure all AttackData assets have a valid prefab assigned.");
            return null; // Restituisce null per evitare il crash
        }

        if (projectilePool[prefab].Count > 0)
        {
            GameObject pooledObject = projectilePool[prefab].Dequeue();
            pooledObject.SetActive(true);
            return pooledObject;
        }
        else
        {
            GameObject toPoolObject = Instantiate(prefab);
            projectilePool.TryAdd(prefab, new Queue<GameObject>());
            projectilePool[prefab].Enqueue(toPoolObject);

            return toPoolObject;
        }
    }

    public void ReturnProjectileToPool(GameObject projectile, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("Attempted to return projectile with null prefab!");
            return;
        }

        if (!projectilePool.ContainsKey(prefab))
        {
            projectilePool[prefab] = new Queue<GameObject>();
        }

        projectilePool[prefab].Enqueue(projectile);
        projectile.SetActive(false);
    }
}

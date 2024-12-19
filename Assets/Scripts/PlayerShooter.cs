using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class PlayerShooter : SerializedMonoBehaviour
{
    [Header("Attack Configuration")]
    [SerializeField] private List<AttackData> startingAttacks;
    private List<Attack> registeredAttacks = new List<Attack>();
    private Dictionary<AttackData, Attack> attackDictionary = new Dictionary<AttackData, Attack>();

    [Header("Pooling System")]
    [SerializeField]private Dictionary<GameObject, Queue<GameObject>> projectilePool = new Dictionary<GameObject, Queue<GameObject>>();

    private Dictionary<Attack, Coroutine> activeCoroutines = new Dictionary<Attack, Coroutine>();

    private void Start()
    {
        for (int i = 0; i < startingAttacks.Count; i++)
        {
            Attack attackInList = startingAttacks[i].attack;
            registeredAttacks.Add(attackInList);
            attackDictionary.TryAdd(startingAttacks[i], attackInList);
        }

        // Avvia le coroutines per ogni abilità
        foreach (var attack in registeredAttacks)
        {
            if (!activeCoroutines.ContainsKey(attack))
            {
                Coroutine coroutine = StartCoroutine(HandleAttackCooldown(attack));
                activeCoroutines.Add(attack, coroutine);
            }
        }

        foreach(AttackData attackData in startingAttacks) 
        {
            AddAttack(attackData);
        }
    }

    private IEnumerator HandleAttackCooldown(Attack attackData)
    {
        while (true)
        {
            yield return new WaitForSeconds(attackData.cooldown);

            switch (attackData.attackType)
            {
                case Attack.AttackType.PROJECTILE:
                    ShootProjectiles(attackData);
                    break;
                case Attack.AttackType.AURA:
                    TriggerAuraDamage(attackData);
                    break;
                case Attack.AttackType.DROP:
                    SpawnDrop(attackData);
                    break;
                case Attack.AttackType.SPAWN:
                    SpawnObjectAroundPlayer(attackData);
                    break;
            }
        }
    }

    private void ShootProjectiles(Attack attack)
    {
        float angleStep = attack.projectileAngle / attack.projectileNumber;
        float startAngle = attack.projectileAngle / -2f;

        // Calcola il vettore radiale dal centro della sfera alla posizione del giocatore
        Vector3 toCenter = (transform.position - Attractor.instance.transform.position).normalized;

        // Calcola la direzione tangente locale frontale
        Vector3 forwardDirection = Vector3.Cross(toCenter, Vector3.Cross(transform.forward, toCenter)).normalized;

        // Se shootBackwards è true, inverti la direzione
        if (attack.shootBackwards)
            forwardDirection = -forwardDirection;

        for (int i = 0; i < attack.projectileNumber; i++)
        {
            // Calcola l'angolo per ogni proiettile
            float angle = startAngle + (angleStep * i);
            Quaternion rotation = Quaternion.AngleAxis(angle, toCenter); // Ruota intorno al vettore radiale

            // Calcola la direzione del proiettile ruotando la forwardDirection
            Vector3 projectileDirection = rotation * forwardDirection;

            // Ottieni il proiettile dal pool
            GameObject projectile = GetProjectileFromPool(attack.prefab);
            projectile.transform.position = transform.position;
            projectile.transform.rotation = Quaternion.LookRotation(projectileDirection);

            // Inizializza il movimento del proiettile
            projectile.GetComponent<Projectile>().InitializeProjectile(
                attack.prefab,
                projectileDirection,
                attack.damage,
                attack.speed,
                attack.lifeTime
            );
        }
    }

    void AddAttack(AttackData attackData)
    {
        Attack nAttack = attackData.attack;
        registeredAttacks.Add(nAttack);
        attackDictionary.TryAdd(attackData, nAttack);

        projectilePool.TryAdd(nAttack.prefab, new Queue<GameObject>());

        for (int i = 0; i < nAttack.initialPool * nAttack.projectileNumber; i++)
        {
            GameObject instantiatedProj = Instantiate(nAttack.prefab); // Usa il prefab corretto
            projectilePool[nAttack.prefab].Enqueue(instantiatedProj);
            instantiatedProj.SetActive(false);
        }
    }

    public void AddAttackUpgrade(UpgradeData upgradeData)
    {
        AttackData attackToModify = upgradeData.originalAttack;
        Attack modifyingAttack = attackDictionary[attackToModify];

        modifyingAttack.cooldown += modifyingAttack.cooldown * (upgradeData.cooldownModification * .01f);
        modifyingAttack.damage += modifyingAttack.damage * (upgradeData.damageModification * .01f);
        modifyingAttack.projectileNumber += upgradeData.projectilesToAdd;
        modifyingAttack.projectileAngle += upgradeData.angleModification;
        modifyingAttack.speed += modifyingAttack.speed * (upgradeData.speedIncrease * .01f);
        modifyingAttack.size += modifyingAttack.size * (upgradeData.sizeIncrease * .01f);
    }


    private void TriggerAuraDamage(Attack attack)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attack.auraRadius);
        foreach (var hitCollider in hitColliders)
        {
            var entity = hitCollider.GetComponent<Entity>();
            if (entity != null && entity.type == EntityType.ENEMY)
            {
                DamageManager.Instance.RegisterDamage(new DamageEvent(
                    targetEntityId: entity.EntityId,
                    damageAmount: attack.damage,
                    sourceType: EntityType.PLAYER
                ));
            }
        }
    }

    private void SpawnDrop(Attack attack)
    {
        // Ottieni il prefab specificato nell'AttackData
        GameObject drop = GetProjectileFromPool(attack.prefab);
        drop.transform.position = transform.position; // Spawn alla posizione del giocatore

        // Configura il ciclo di vita del drop (se definito)
        if (attack.hasLifeTime)
        {
            drop.GetComponent<Projectile>().InitializeProjectile(attack.prefab, Vector3.zero, attack.damage, 0, attack.lifeTimeDrop);
        }
    }

    float orbitRadius = -1;

    private void SpawnObjectAroundPlayer(Attack attack)
    {
        int numObjects = attack.projectileNumber; // Numero di oggetti da spawnare

        if(orbitRadius == -1) orbitRadius = Vector3.Distance(transform.position, Attractor.instance.transform.position); // Distanza fissa (configurabile)
        float angleStep = 360f / numObjects; // Distribuzione uniforme a 360 gradi

        for (int i = 0; i < numObjects; i++)
        {
            float angle = i * angleStep;
            Vector3 spawnPosition = GetOrbitPosition(transform.position, angle, orbitRadius);

            GameObject orbitObject = GetProjectileFromPool(attack.prefab);
            orbitObject.transform.position = spawnPosition;
            orbitObject.transform.parent = transform; // Imposta il giocatore come parent
            orbitObject.GetComponent<Projectile>()?.InitializeOrbiting(attack.prefab, orbitRadius, angle, attack.speed);
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

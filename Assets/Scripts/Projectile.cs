using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class Projectile : SerializedMonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private EntityType sourceType;

    [SerializeField] bool isOrbiting;

    private Vector3 direction;
    private float speed;
    private float lifeTime;

    private Transform planetCenter;
    GameObject prefab;

    public void InitializeProjectile(GameObject prefab, Vector3 direction, float damage, float speed, float lifeTime)
    {
        this.prefab = prefab; // Assegna il prefab
        this.direction = direction.normalized;
        this.damage = damage;
        this.speed = speed;
        this.lifeTime = lifeTime;

        if (!planetCenter) planetCenter = Attractor.instance.transform; // Riferimento al centro del pianeta
        StartCoroutine(LifeCycle());
    }

    private void Update()
    {
        if(isOrbiting)
        {
            Orbit();
        }
        else
        {
            Move();
        }
    }

    private void Move()
    {
        // Calcola il vettore dal centro della sfera alla posizione corrente
        Vector3 toCenter = (transform.position - planetCenter.position).normalized;

        // Calcola il vettore tangente alla superficie della sfera
        Vector3 tangentDirection = Vector3.Cross(toCenter, direction).normalized;

        // Sposta il proiettile lungo la direzione tangente
        transform.position += tangentDirection * speed * Time.deltaTime;

        // Mantieni la distanza costante dal centro della sfera
        transform.position = planetCenter.position + (transform.position - planetCenter.position).normalized * Vector3.Distance(transform.position, planetCenter.position);

        // Ruota il proiettile per puntare nella direzione di movimento
        transform.rotation = Quaternion.LookRotation(tangentDirection, toCenter);
    }


    ///ORBIT BULLET IMPLEMENTATION
    private float orbitRadius;
    private float orbitSpeed;
    private float currentAngle;

    private Transform playerTransform;

    public void InitializeOrbiting(GameObject prefab, float radius, float startAngle, float speed)
    {
        this.prefab = prefab; // Assegna il prefab
        orbitRadius = radius;
        orbitSpeed = speed;
        currentAngle = startAngle;
        playerTransform = transform.parent; // Il giocatore è il parent
        isOrbiting = true;
    }

    private void Orbit()
    {
        if (playerTransform == null) return;

        // Aggiorna l'angolo basato sulla velocità
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle > 360f) currentAngle -= 360f;

        // Calcola la nuova posizione orbitale
        Vector3 orbitPosition = GetOrbitPosition(playerTransform.position, currentAngle, orbitRadius);
        transform.position = orbitPosition;
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

    private IEnumerator LifeCycle()
    {
        yield return new WaitForSeconds(lifeTime);
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        FindObjectOfType<PlayerShooter>().ReturnProjectileToPool(gameObject, prefab); // Passa il prefab
    }
    private void OnTriggerEnter(Collider other)
    {
        var entity = other.GetComponent<Entity>();
        if (entity != null && entity.type != sourceType)
        {
            DamageManager.Instance.RegisterDamage(new DamageEvent
            {
                TargetEntityId = entity.EntityId,
                DamageAmount = damage,
                SourceType = sourceType
            });

            ReturnToPool();
            gameObject.SetActive(false); // Ricicla o disattiva il proiettile.
        }
    }
}


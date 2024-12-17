using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance;

    private Dictionary<int, Entity> entityMap = new Dictionary<int, Entity>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void RegisterEntity(Entity entity)
    {
        if (!entityMap.ContainsKey(entity.EntityId))
        {
            entityMap.Add(entity.EntityId, entity);
        }
    }

    public void UnregisterEntity(Entity entity)
    {
        if (entityMap.ContainsKey(entity.EntityId))
        {
            entityMap.Remove(entity.EntityId);
        }
    }

    public Entity GetEntityById(int entityId)
    {
        entityMap.TryGetValue(entityId, out var entity);
        return entity;
    }
}

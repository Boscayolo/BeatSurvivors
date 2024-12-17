using System.Collections.Generic;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    public static DamageManager Instance;

    private NativeList<DamageEvent> damageEvents;
    private JobHandle damageJobHandle;

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

        damageEvents = new NativeList<DamageEvent>(Allocator.Persistent);
    }

    private void OnDestroy()
    {
        if (damageJobHandle.IsCompleted)
        {
            damageJobHandle.Complete();
        }
        damageEvents.Dispose();
    }

    public void RegisterDamage(DamageEvent damageEvent)
    {
        damageEvents.Add(damageEvent);
    }

    private void LateUpdate()
    {
        if (damageEvents.Length > 0)
        {
            ProcessDamageEvents();
        }
    }

    private void ProcessDamageEvents()
    {
        var damageJob = new DamageProcessingJob
        {
            DamageEvents = damageEvents,
        };

        damageJobHandle = damageJob.Schedule(damageEvents.Length, 64);
        damageJobHandle.Complete();

        // Dispatch damage to entities.
        foreach (var damageEvent in damageEvents)
        {
            var entity = EntityManager.Instance.GetEntityById(damageEvent.TargetEntityId);
            if (entity != null)
            {
                entity.ApplyDamage(damageEvent);
            }
        }

        damageEvents.Clear();
    }
}

public struct DamageEvent
{
    public int TargetEntityId;        // ID dell'entità che riceve il danno
    public float DamageAmount;        // Quantità di danno inflitto
    public EntityType SourceType; // Tipo dell'entità che infligge il danno

    // Costruttore
    public DamageEvent(int targetEntityId, float damageAmount, EntityType sourceType)
    {
        TargetEntityId = targetEntityId;
        DamageAmount = damageAmount;
        SourceType = sourceType;
    }
}

public struct DamageProcessingJob : IJobParallelFor
{
    [NativeDisableParallelForRestriction]
    public NativeList<DamageEvent> DamageEvents;

    public void Execute(int index)
    {
        var damageEvent = DamageEvents[index];

        // (Optional) Modifica o valida il danno qui.
        damageEvent.DamageAmount = Mathf.Max(damageEvent.DamageAmount, 0);
        DamageEvents[index] = damageEvent;
    }
}
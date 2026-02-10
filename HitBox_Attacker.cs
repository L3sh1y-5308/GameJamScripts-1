using UnityEngine;
using System.Collections.Generic;

public class HitBox_Attacker : HitBox_Active
{
    [Header("Attacker Settings")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private bool debugDrawCollider = true;

    private bool isAttacking = false;
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    protected override void Start()
    {
        base.Start();
        
        // Убедитесь, что коллайдер настроен правильно
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("HitBox_Attacker нуждается в Collider компоненте!");
        }
    }

    public override bool IsHitBoxColides()
    {
        return hitBoxData.hasHit;
    }

    protected override void PerformHitBoxDetection()
    {
        // Не используется в trigger-based подходе
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (!isAttacking) return;

        // Проверяем LayerMask
        if (((1 << other.gameObject.layer) & detectionMask) == 0) return;

        // Избегаем повторного урона одной цели за атаку
        if (hitTargets.Contains(other.gameObject)) return;

        // Записываем данные столкновения
        hitBoxData.hasHit = true;
        hitBoxData.hitObject = other.gameObject;
        hitBoxData.hitTag = other.tag;
        hitBoxData.hitPoint = other.ClosestPoint(transform.position);
        hitBoxData.hitNormal = (transform.position - hitBoxData.hitPoint).normalized;
        hitBoxData.distance = Vector3.Distance(transform.position, hitBoxData.hitPoint);

        hitTargets.Add(other.gameObject);

        // Применяем урон
        HitBox_Receiver receiver = other.GetComponent<HitBox_Receiver>();
        if (receiver != null)
        {
            receiver.TakeDamage(attackDamage, transform.position);
        }

        Debug.Log($"Атака попала в: {other.gameObject.name}");
    }

    public void StartAttack()
    {
        isAttacking = true;
        hitTargets.Clear();
        hitBoxData.hasHit = false;
    }

    public void StopAttack()
    {
        isAttacking = false;
        hitBoxData.hasHit = false;
        hitBoxData.hitObject = null;
        hitTargets.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugDrawCollider) return;

        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = isAttacking ? Color.red : Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
        {
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
        else if (col is CapsuleCollider capsule)
        {
            // Упрощенная визуализация капсулы
            Gizmos.DrawWireSphere(capsule.center, capsule.radius);
        }
    }
}
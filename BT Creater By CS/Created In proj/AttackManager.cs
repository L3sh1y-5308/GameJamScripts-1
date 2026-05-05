using UnityEngine;

/// <summary>
/// Управляет атаками и соответствующими анимациями
/// </summary>
public class AttackManager : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Animator animator;

    [Header("Параметры атаки")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float veryCloseRange = 3f;

    [Header("Названия анимаций")]
    [SerializeField] private string attackAnimationName = "Attack Anim";
    [SerializeField] private string attackingParameter = "Rat_Attacking";

    [Header("Отладка")]
    [SerializeField] private bool debugMode = true;

    private float attackTimer = 0f;
    private bool isCurrentlyAttacking = false;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Проверяет, доступна ли атака (кулдаун истек)
    /// </summary>
    public bool CanAttack() => attackTimer <= 0f;

    /// <summary>
    /// Выполняет атаку на цель
    /// </summary>
    public bool TryAttack(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning($"❌ Цель NULL при атаке!");
            return false;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // Проверка расстояния
        if (distanceToTarget > veryCloseRange + 0.5f)
        {
            if (debugMode)
                Debug.LogWarning($"❌ Слишком далеко для атаки! {distanceToTarget:F2}м > {veryCloseRange + 0.5f}м");
            return false;
        }

        // Проверка кулдауна
        if (!CanAttack())
        {
            if (debugMode)
                Debug.Log($"⏱️ Атака на кулдауне: {attackTimer:F2}s / {attackCooldown}s");
            return false;
        }

        // Поворот к цели
        Vector3 dirToTarget = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(dirToTarget);

        // Поиск системы здоровья
        HealfP healthSystem = target.GetComponent<HealfP>();
        if (healthSystem == null)
        {
            Debug.LogError($"❌ HealfP НЕ НАЙДЕН на {target.gameObject.name}!");
            return false;
        }

        // ===== ЗАПУСКАЕМ АНИМАЦИЮ АТАКИ =====
        PlayAttackAnimation();

        // Наносим урон
        healthSystem.TakeDamage(attackDamage);

        if (debugMode)
            Debug.Log($"<color=red>💥 УДАР! Урон: {attackDamage} | Расстояние: {distanceToTarget:F2}м | Цель: {target.gameObject.name}</color>");

        // Запускаем кулдаун
        attackTimer = attackCooldown;

        // Останавливаем анимацию через время
        Invoke(nameof(StopAttackAnimation), 0.5f);

        return true;
    }

    /// <summary>
    /// Запускает анимацию атаки
    /// </summary>
    private void PlayAttackAnimation()
    {
        if (animator == null)
            return;

        isCurrentlyAttacking = true;

        // Используем только параметр Rat_Attacking (который правильно настроен в Animator)
        animator.SetBool(attackingParameter, true);

        if (debugMode)
            Debug.Log($"🎭 Запущена анимация атаки! Параметр: {attackingParameter}");
    }

    /// <summary>
    /// Останавливает анимацию атаки
    /// </summary>
    private void StopAttackAnimation()
    {
        if (animator == null)
            return;

        isCurrentlyAttacking = false;
        animator.SetBool(attackingParameter, false);

        if (debugMode)
            Debug.Log($"🎭 Анимация атаки завершена!");
    }

    public bool IsAttacking() => isCurrentlyAttacking;
    public float GetAttackCooldownRemaining() => attackTimer;
}
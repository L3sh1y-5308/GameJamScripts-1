using UnityEngine;

/// <summary>
/// Управляет состояниями ИИ (Idle, Chase, Attack)
/// Реализует логику из flowchart с проверками расстояния и условиями
/// </summary>
public class AIAttackStateMachine : MonoBehaviour
{

    [Header("Ссылки")]
    [SerializeField] private AttackManager attackManager;
    [SerializeField] private Transform targetPlayer;
    [SerializeField] private Animator animator;

    [Header("Параметры расстояний")]
    [SerializeField] private float detectionRange = 15f;      // На каком расстоянии видит врага
    [SerializeField] private float attackRange = 4.5f;         // На каком расстоянии начинает атаку (из flowchart)
    [SerializeField] private float chaseRange = 0.5f;          // Расстояние остановки при преследовании

    [Header("Параметры движения")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private CharacterController characterController;

    [Header("Отладка")]
    [SerializeField] private bool debugMode = true;

    public enum AIState
    {
        Idle,
        Chase,
        Attack
    }

    private AIState currentState = AIState.Idle;
    private AIState previousState;
    private float stateChangeTime;

    private void Awake()
    {
        if (attackManager == null)
            attackManager = GetComponent<AttackManager>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        // Ищем игрока, если не установлен
        if (targetPlayer == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                targetPlayer = player.transform;
        }
    }

    private void Update()
    {
        if (targetPlayer == null)
        {
            SetState(AIState.Idle);
            return;
        }

        // Основной цикл логики состояния
        UpdateState();
    }

    private void UpdateState()
    {
        switch (currentState)
        {
            case AIState.Idle:
                UpdateIdle();
                break;
            case AIState.Chase:
                UpdateChase();
                break;
            case AIState.Attack:
                UpdateAttack();
                break;
        }
    }

    // ===== IDLE СОСТОЯНИЕ =====
    private void UpdateIdle()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.position);

        // Проверка: видим ли врага?
        if (distanceToTarget <= detectionRange)
        {
            SetState(AIState.Chase);
            if (debugMode)
                Debug.Log($"🔍 Враг обнаружен! Расстояние: {distanceToTarget:F2}м");
        }
    }

    // ===== CHASE СОСТОЯНИЕ =====
    private void UpdateChase()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.position);

        // Проверка: враг далеко? Вернуться в Idle
        if (distanceToTarget > detectionRange)
        {
            SetState(AIState.Idle);
            if (debugMode)
                Debug.Log($"👀 Враг потерян! Расстояние: {distanceToTarget:F2}м > {detectionRange}м");
            return;
        }

        // Проверка: враг в пределах атаки?
        if (distanceToTarget <= attackRange)
        {
            SetState(AIState.Attack);
            if (debugMode)
                Debug.Log($"⚔️ Враг в пределах атаки! Расстояние: {distanceToTarget:F2}м");
            return;
        }

        // Движение к врагу
        MoveTowardsTarget();
    }

    // ===== ATTACK СОСТОЯНИЕ =====
    private void UpdateAttack()
    {
        float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.position);

        // Проверка: враг далеко? Вернуться на преследование
        if (distanceToTarget > detectionRange)
        {
            SetState(AIState.Idle);
            if (debugMode)
                Debug.Log($"👀 Враг потерян во время атаки!");
            return;
        }

        // Проверка: враг вышел из диапазона атаки?
        if (distanceToTarget > attackRange)
        {
            SetState(AIState.Chase);
            if (debugMode)
                Debug.Log($"🏃 Враг выходит из диапазона атаки! Преследование...");
            return;
        }

        // Попытка атаки
        if (attackManager.CanAttack())
        {
            bool attackSucceeded = attackManager.TryAttack(targetPlayer);
            if (!attackSucceeded && debugMode)
                Debug.Log("⏱️ Атака на кулдауне или не удалась");
        }
    }

    // ===== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ =====

    private void MoveTowardsTarget()
    {
        if (characterController == null)
            return;

        Vector3 directionToTarget = (targetPlayer.position - transform.position).normalized;

        // Поворот к цели
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

        // Движение
        Vector3 movement = directionToTarget * chaseSpeed * Time.deltaTime;
        characterController.Move(movement);

        // Зацикливаем анимацию бега (если есть параметр Run)
        if (animator != null)
            animator.SetBool("IsRunning", true);
    }

    private void SetState(AIState newState)
    {
        if (newState == currentState)
            return;

        previousState = currentState;
        currentState = newState;
        stateChangeTime = Time.time;

        // Обновляем анимацию при смене состояния
        UpdateAnimationForState();

        if (debugMode)
            Debug.Log($"<color=yellow>📊 Смена состояния: {previousState} → {currentState}</color>");
    }

    private void UpdateAnimationForState()
    {
        if (animator == null)
            return;

        // Останавливаем все движения перед сменой состояния
        animator.SetBool("IsRunning", false);

        switch (currentState)
        {
            case AIState.Idle:
                animator.SetBool("IsRunning", false);
                break;
            case AIState.Chase:
                animator.SetBool("IsRunning", true);
                break;
            case AIState.Attack:
                animator.SetBool("IsRunning", false);
                // Атака будет управляться AttackManager
                break;
        }
    }

    public AIState GetCurrentState() => currentState;
    public float GetDistanceToTarget() => targetPlayer != null ?
        Vector3.Distance(transform.position, targetPlayer.position) : -1f;

    // Визуализация расстояний для отладки
    private void OnDrawGizmosSelected()
    {
        if (targetPlayer == null)
            return;

        // Радиус обнаружения
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Радиус атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Линия к цели
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, targetPlayer.position);
    }
}
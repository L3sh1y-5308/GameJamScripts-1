using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AdvancedBTBrainController : MonoBehaviour
{
    [Header("Ссылки")]
    public NavMeshAgent navMeshAgent;
    public Animator animator;
    public HungerSystem hungerSystem;
    public AdaptiveVisionSystem visionSystem;
    public AttackManager attackManager;  // ← НОВОЕ: Ссылка на AttackManager

    [Header("Теги для обнаружения")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string foodTag = "Food";
    [SerializeField] private string threatTag = "Monster";

    [Header("Параметры движения")]
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float fleeSpeed = 5f;
    [SerializeField] private float normalSpeed = 2f;

    [Header("Система расстояний для боя")]
    [SerializeField] private float veryCloseRange = 3f;
    [SerializeField] private float closeRange = 6f;
    [SerializeField] private float mediumRange = 15f;
    [SerializeField] private float maxDetectionRange = 30f;
    [SerializeField] private float stoppingDistance = 1.2f;

    [Header("Параметры атаки")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackSpeed = 0.5f;

    [Header("Названия параметров аниматора")]
    [SerializeField] private string speedParameter = "Speed";
    [SerializeField] private string attackingParameter = "Rat_Attacking";

    [Header("Пороги голода")]
    [SerializeField] private float hungerThreshold = 50f;

    [Header("Отладка")]
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool showRangeVisualization = true;
    [SerializeField] private bool showDetailedDebug = true;  // ← НОВОЕ: подробная отладка

    private Node rootNode;
    private Transform playerTarget;
    private Transform foodTarget;
    private Transform threatTarget;
    private bool isInitialized = false;
    private float distanceToPlayer = float.MaxValue;

    private enum DistanceRange
    {
        OutOfRange,
        VeryFar,
        Medium,
        Close,
        VeryClose
    }

    private void Awake()
    {
        try
        {
            if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
            if (animator == null) animator = GetComponent<Animator>();
            if (hungerSystem == null) hungerSystem = GetComponent<HungerSystem>();
            if (visionSystem == null) visionSystem = GetComponent<AdaptiveVisionSystem>();
            if (attackManager == null) attackManager = GetComponent<AttackManager>();  // ← НОВОЕ

            if (navMeshAgent == null)
            {
                Debug.LogError($"❌ NavMeshAgent не найден на {gameObject.name}!");
                enabled = false;
                return;
            }

            if (animator == null)
            {
                Debug.LogError($"❌ Animator не найден на {gameObject.name}!");
                enabled = false;
                return;
            }

            if (attackManager == null)  // ← НОВОЕ
            {
                Debug.LogError($"❌ AttackManager не найден на {gameObject.name}!");
                enabled = false;
                return;
            }

            navMeshAgent.stoppingDistance = stoppingDistance;
            ValidateAnimatorParameters();

            isInitialized = true;

            if (debugMode)
                Debug.Log($"✅ {gameObject.name} инициализирован!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Ошибка инициализации: {ex.Message}");
            enabled = false;
        }
    }

    /// <summary>
    /// Проверяет наличие параметров в аниматоре
    /// </summary>
    private void ValidateAnimatorParameters()
    {
        if (animator == null) return;

        // Проверяем Speed параметр
        bool hasSpeed = false;
        bool hasAttacking = false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == speedParameter && param.type == AnimatorControllerParameterType.Float)
                hasSpeed = true;

            if (param.name == attackingParameter && param.type == AnimatorControllerParameterType.Bool)
                hasAttacking = true;

            if (showDetailedDebug)
                Debug.Log($"🎭 Аниматор параметр: {param.name} ({param.type})");
        }

        if (!hasSpeed)
            Debug.LogWarning($"⚠️ Параметр '{speedParameter}' (Float) не найден в аниматоре!");

        if (!hasAttacking)
            Debug.LogWarning($"⚠️ Параметр '{attackingParameter}' (Bool) не найден в аниматоре!");

        if (hasSpeed && hasAttacking && showDetailedDebug)
            Debug.Log($"✅ Все параметры аниматора найдены!");
    }

    private void Start()
    {
        if (!isInitialized) return;

        try
        {
            BuildBehaviorTree();
            if (debugMode)
                Debug.Log($"✅ Дерево поведения построено");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Ошибка построения дерева: {ex.Message}");
        }
    }

    private void Update()
    {
        if (!isInitialized) return;

        try
        {
            UpdateTargetsFromVision();
            UpdateDistances();
            UpdateAnimatorSpeed();

            if (rootNode != null)
                rootNode.Evaluate();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Ошибка в Update: {ex.Message}");
        }
    }

    private void UpdateAnimatorSpeed()
    {
        if (animator == null || navMeshAgent == null)
            return;

        float currentSpeed = navMeshAgent.velocity.magnitude;
        animator.SetFloat(speedParameter, currentSpeed, 0.1f, Time.deltaTime);
    }

    private DistanceRange GetDistanceRange()
    {
        if (playerTarget == null)
            return DistanceRange.OutOfRange;

        if (distanceToPlayer > maxDetectionRange)
            return DistanceRange.OutOfRange;
        
        if (distanceToPlayer > mediumRange)
            return DistanceRange.VeryFar;
        
        if (distanceToPlayer > closeRange)
            return DistanceRange.Medium;
        
        if (distanceToPlayer > veryCloseRange)
            return DistanceRange.Close;
        
        return DistanceRange.VeryClose;
    }

    private void UpdateDistances()
    {
        distanceToPlayer = playerTarget != null 
            ? Vector3.Distance(transform.position, playerTarget.position) 
            : float.MaxValue;
    }

    private void UpdateTargetsFromVision()
    {
        if (visionSystem == null) return;

        try
        {
            playerTarget = visionSystem.CanSeePlayer() ? visionSystem.GetDetectedPlayer() : null;
            foodTarget = visionSystem.CanSeeFood() ? visionSystem.detectedFood : null;
            threatTarget = visionSystem.CanSeeDanger() ? visionSystem.detectedDanger : null;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"⚠️ Ошибка при обновлении целей: {ex.Message}");
        }
    }

    private void BuildBehaviorTree()
    {
        ConditionNode hasPlayerCondition = new ConditionNode(() => playerTarget != null);
        ConditionNode hasFoodCondition = new ConditionNode(() => foodTarget != null);
        ConditionNode isThreatNearCondition = new ConditionNode(() => threatTarget != null);
        ConditionNode isHungryCondition = new ConditionNode(() => 
            hungerSystem != null && hungerSystem.GetSatietyPercent() * 100f < hungerThreshold);

        ConditionNode isVeryCloseCondition = new ConditionNode(() => 
        {
            bool result = GetDistanceRange() == DistanceRange.VeryClose;
            if (showDetailedDebug && result && playerTarget != null)
                Debug.Log($"📍 ОЧЕНЬ БЛИЗКО! Расстояние: {distanceToPlayer:F2}м (< {veryCloseRange}м)");
            return result;
        });

        ConditionNode isCloseCondition = new ConditionNode(() => GetDistanceRange() == DistanceRange.Close);
        ConditionNode isMediumRangeCondition = new ConditionNode(() => GetDistanceRange() == DistanceRange.Medium);
        ConditionNode isVeryFarCondition = new ConditionNode(() => GetDistanceRange() == DistanceRange.VeryFar);
        
        ConditionNode canAttackCondition = new ConditionNode(() =>  // ← ОБНОВЛЕНО
        {
            bool result = attackManager.CanAttack();
            if (showDetailedDebug && playerTarget != null)
                Debug.Log($"⏱️ Cooldown: {attackManager.GetAttackCooldownRemaining():F2}s / {attackCooldown}s - {(result ? "✅ ГОТОВ" : "❌ ЖДЕМ")}");
            return result;
        });

        // 1. БЕГСТВО
        Sequence fleeSequence = new Sequence(new List<Node>
        {
            isThreatNearCondition,
            new FunctionalNode(() => 
            {
                navMeshAgent.speed = fleeSpeed;
                animator.SetBool(attackingParameter, false);
                if (showDetailedDebug)
                    Debug.Log($"🏃 БЕГСТВО! Угроза обнаружена");
                return UpdateFleeTarget();
            })
        });

        // 2. АТАКА (очень близко)
        Sequence attackSequence = new Sequence(new List<Node>
        {
            hasPlayerCondition,
            isVeryCloseCondition,
            canAttackCondition,
            new FunctionalNode(() => 
            {
                if (showDetailedDebug)
                    Debug.Log($"⚔️ НАЧИНАЕМ АТАКУ!");
                
                // ← ОБНОВЛЕНО: Используем AttackManager
                bool attackSucceeded = attackManager.TryAttack(playerTarget);
                return attackSucceeded ? NodeState.Success : NodeState.Failure;
            })
        });

        // 3. ПОДГОТОВКА К АТАКЕ (очень близко, но кулдаун еще идет)
        Sequence prepareAttackSequence = new Sequence(new List<Node>
        {
            hasPlayerCondition,
            isVeryCloseCondition,
            new FunctionalNode(() => 
            {
                navMeshAgent.speed = 0f;
                animator.SetBool(attackingParameter, false);
                
                if (playerTarget != null)
                {
                    Vector3 dirToTarget = (playerTarget.position - transform.position).normalized;
                    transform.rotation = Quaternion.LookRotation(dirToTarget);
                }
                
                return NodeState.Running;
            })
        });

        // 4. ПОДГОТОВКА К АТАКЕ (средняя дистанция, приближение)
        Sequence approachCloseSequence = new Sequence(new List<Node>
        {
            hasPlayerCondition,
            isCloseCondition,
            new FunctionalNode(() => 
            {
                navMeshAgent.speed = chaseSpeed * 0.5f;
                animator.SetBool(attackingParameter, false);
                navMeshAgent.SetDestination(playerTarget.position);
                return NodeState.Running;
            })
        });

        // 5. ПРЕСЛЕДОВАНИЕ (среднее)
        Sequence activeChaseMediumSequence = new Sequence(new List<Node>
        {
            hasPlayerCondition,
            isMediumRangeCondition,
            new FunctionalNode(() => 
            {
                navMeshAgent.speed = chaseSpeed;
                animator.SetBool(attackingParameter, false);
                navMeshAgent.SetDestination(playerTarget.position);
                return NodeState.Running;
            })
        });

        // 6. ПРЕСЛЕДОВАНИЕ (дальнее)
        Sequence activeChaseDistantSequence = new Sequence(new List<Node>
        {
            hasPlayerCondition,
            isVeryFarCondition,
            new FunctionalNode(() => 
            {
                navMeshAgent.speed = chaseSpeed * 1.2f;
                animator.SetBool(attackingParameter, false);
                navMeshAgent.SetDestination(playerTarget.position);
                return NodeState.Running;
            })
        });

        // 7. ПОИСК ЕДЫ
        Sequence eatFoodSequence = new Sequence(new List<Node>
        {
            isHungryCondition,
            hasFoodCondition,
            new FunctionalNode(() => 
            {
                navMeshAgent.speed = normalSpeed;
                animator.SetBool(attackingParameter, false);
                navMeshAgent.SetDestination(foodTarget.position);
                return NodeState.Running;
            }),
            new FunctionalNode(() => EatFoodIfReached())
        });

        // 8. ОТДЫХ
        Sequence idleSequence = new Sequence(new List<Node>
        {
            new FunctionalNode(() => 
            {
                navMeshAgent.speed = 0f;
                animator.SetBool(attackingParameter, false);
                return NodeState.Success;
            })
        });

        rootNode = new Selector(new List<Node>
        {
            fleeSequence,
            attackSequence,
            prepareAttackSequence,
            approachCloseSequence,
            activeChaseMediumSequence,
            activeChaseDistantSequence,
            eatFoodSequence,
            idleSequence
        });
    }

    private NodeState EatFoodIfReached()
    {
        if (hungerSystem != null && foodTarget != null &&
            Vector3.Distance(transform.position, foodTarget.position) < 0.5f)
        {
            hungerSystem.EatFood(30);
            if (debugMode)
                Debug.Log($"<color=yellow>[{gameObject.name}] Съедаю еду! 🍖</color>");
            return NodeState.Success;
        }
        return NodeState.Running;
    }

    public bool IsAttacking() => attackManager.IsAttacking();  // ← ОБНОВЛЕНО
    
    public Transform GetCurrentTarget() => playerTarget ?? (foodTarget ?? threatTarget);
    public bool IsHungry() => hungerSystem != null && hungerSystem.GetSatietyPercent() * 100f < hungerThreshold;
    public bool CanSeePlayer() => playerTarget != null;
    public bool CanSeeThreat() => threatTarget != null;
    public float GetDistanceToPlayer() => distanceToPlayer;

    private NodeState UpdateFleeTarget()
    {
        if (threatTarget != null)
        {
            // Move away from the threat
            Vector3 directionAway = (transform.position - threatTarget.position).normalized;
            Vector3 fleePosition = transform.position + directionAway * 5f; // Flee 5 units away
            navMeshAgent.SetDestination(fleePosition);
            return NodeState.Running;
        }
        return NodeState.Failure;
    }

    void OnDrawGizmos()
    {
        if (!showRangeVisualization) return;

        Vector3 pos = transform.position;

        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        DrawCircle(pos, veryCloseRange, 32);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        DrawCircle(pos, closeRange, 32);

        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        DrawCircle(pos, mediumRange, 32);

        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        DrawCircle(pos, maxDetectionRange, 32);
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        Vector3[] points = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            points[i] = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }
}
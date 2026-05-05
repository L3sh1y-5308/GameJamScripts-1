using System.Collections.Generic;
using UnityEngine;

// Вспомогательный узел для проверки условий (например, проверки дистанции)
public class ConditionNode : Node
{
    private System.Func<bool> condition;

    public ConditionNode(System.Func<bool> condition)
    {
        this.condition = condition;
    }

    public override NodeState Evaluate()
    {
        // Если условие выполняется - Success, иначе - Failure
        state = condition() ? NodeState.Success : NodeState.Failure;
        return state;
    }
}

// Изолированный контроллер, который управляет поведением через дерево
public class BTBrainController : MonoBehaviour
{
    [Header("Ссылки (можно назначить в инспекторе)")]
    [Tooltip("Если пусто, возьмет компонент с этого же объекта")]
    public Transform myTransform;
    public Animator myAnimator;

    [Header("Настройки ИИ")]
    public Transform target;
    public float moveSpeed = 3f;
    public float attackRange = 1.5f;

    [Header("Названия стейтов в Animator")]
    public string runAnim = "Run";
    public string idleAnim = "Idle";
    public string attackAnim = "Attack";

    // Главный корень нашего дерева
    private Node _rootNode;

    private void Awake()
    {
        // Автоматически подхватываем компоненты, если они не заданы вручную
        if (myTransform == null) myTransform = transform;
        if (myAnimator == null) myAnimator = GetComponent<Animator>();
    }

    private void Start()
    {
        BuildBehaviorTree();
    }

    private void Update()
    {
        // Прогоняем логику дерева каждый кадр
        if (_rootNode != null)
        {
            _rootNode.Evaluate();
        }
    }

    // Метод сборки дерева
    private void BuildBehaviorTree()
    {
        // 1. Создаем узлы действий
        MoveToTargetNode moveNode = new MoveToTargetNode(myTransform, target, moveSpeed, attackRange);
        PlayAnimationNode runAnimNode = new PlayAnimationNode(myAnimator, runAnim);
        PlayAnimationNode attackAnimNode = new PlayAnimationNode(myAnimator, attackAnim);
        PlayAnimationNode idleAnimNode = new PlayAnimationNode(myAnimator, idleAnim);

        // 2. Создаем узлы условий
        ConditionNode hasTargetCondition = new ConditionNode(() => target != null);
        ConditionNode inAttackRangeCondition = new ConditionNode(() =>
            target != null && Vector3.Distance(myTransform.position, target.position) <= attackRange);
        ConditionNode outOfAttackRangeCondition = new ConditionNode(() =>
            target != null && Vector3.Distance(myTransform.position, target.position) > attackRange);

        // 3. Ветка АТАКИ: Если в радиусе атаки -> проиграть анимацию атаки
        Sequence attackSequence = new Sequence(new List<Node>
        {
            inAttackRangeCondition,
            attackAnimNode
        });

        // 4. Ветка ПРЕСЛЕДОВАНИЯ: Если вне радиуса атаки -> включить анимацию бега и двигаться
        Sequence chaseSequence = new Sequence(new List<Node>
        {
            outOfAttackRangeCondition,
            runAnimNode,
            moveNode
        });

        // 5. Ветка БЕЗДЕЙСТВИЯ (Fallback, если нет цели)
        Sequence idleSequence = new Sequence(new List<Node>
        {
            new ConditionNode(() => target == null), // Условие: нет цели
            idleAnimNode
        });

        // 6. КОРЕНЬ: Селектор будет проверять ветки по очереди (сверху вниз)
        // Он выберет первую ветку, которая не вернет Failure
        _rootNode = new Selector(new List<Node>
        {
            attackSequence, // Сначала проверяем, можем ли атаковать
            chaseSequence,  // Если не можем атаковать, пытаемся догнать
            idleSequence    // Если нет цели вообще, стоим на месте
        });
    }

    // Публичный метод, если захочешь менять цель из другого скрипта (например, из скрипта агра/зрения)
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
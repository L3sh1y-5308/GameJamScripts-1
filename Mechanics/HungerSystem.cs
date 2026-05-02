using UnityEngine;
using UnityEngine.Events;

public class HungerSystem : MonoBehaviour
{
    [Header("Hunger Settings")]
    [SerializeField] float maxSatiety = 100f;
    [SerializeField] float hungerDecayRate = 10f; // единиц голода в минуту
    [SerializeField] float hungerDecayInterval = 30f; // секунд между убыванием голода

    [Header("Starvation Damage")]
    [SerializeField] float starvationDamageRate = 1f; // единиц здоровья в минуту при голоде
    [SerializeField] float starvationDamageInterval = 1f; // секунд между уроном от голода

    [Header("Recovery Settings")]
    [SerializeField] float satietyRecoveryRate = 5f; // единиц сытости в минуту при сытости
    [SerializeField] float satietyRecoveryInterval = 1f; // секунд между восстановлением
    [SerializeField] float recoveryHealthThreshold = 50f; // минимальная сытость для восстановления здоровья

    [Header("Events")]
    public UnityEvent<float> OnSatietyChanged;
    public UnityEvent OnStarving;
    public UnityEvent OnRecovering;

    private float currentSatiety;
    private float hungerTimer;
    private float starvationTimer;
    private float recoveryTimer;
    private HealfP healthSystem;
    private bool isStarving;

    public float CurrentSatiety => currentSatiety;
    public float MaxSatiety => maxSatiety;
    public bool IsStarving => isStarving;

    void Awake()
    {
        currentSatiety = maxSatiety;
        healthSystem = GetComponent<HealfP>();

        if (healthSystem == null)
        {
            Debug.LogError($"HungerSystem на {gameObject.name} требует компонент HealfP!");
        }
    }

    void Update()
    {
        UpdateHungerDecay();
        UpdateStarvationDamage();
        UpdateRecovery();
    }

    /// <summary>
    /// Уменьшает сытость с течением времени
    /// </summary>
    void UpdateHungerDecay()
    {
        hungerTimer += Time.deltaTime;

        if (hungerTimer >= hungerDecayInterval)
        {
            currentSatiety = Mathf.Max(0, currentSatiety - hungerDecayRate);
            OnSatietyChanged?.Invoke(currentSatiety);
            hungerTimer = 0f;

            Debug.Log($"[Hunger] Текущая сытость: {currentSatiety}/{maxSatiety}");
        }
    }

    /// <summary>
    /// Наносит урон здоровью при полном голоде
    /// </summary>
    void UpdateStarvationDamage()
    {
        if (currentSatiety <= 0 && healthSystem != null)
        {
            if (!isStarving)
            {
                isStarving = true;
                OnStarving?.Invoke();
                Debug.Log($"<color=red>[Hunger] {gameObject.name} голодает!</color>");
            }

            starvationTimer += Time.deltaTime;

            if (starvationTimer >= starvationDamageInterval)
            {
                healthSystem.TakeDamage(starvationDamageRate);
                starvationTimer = 0f;
                Debug.Log($"<color=red>[Starvation] Урон от голода: -{starvationDamageRate} HP</color>");
            }
        }
        else if (currentSatiety > 0 && isStarving)
        {
            isStarving = false;
            Debug.Log($"<color=green>[Hunger] {gameObject.name} больше не голодает!</color>");
        }
    }

    /// <summary>
    /// Восстанавливает здоровье при достаточной сытости
    /// </summary>
    void UpdateRecovery()
    {
        if (currentSatiety >= recoveryHealthThreshold && healthSystem != null)
        {
            recoveryTimer += Time.deltaTime;

            if (recoveryTimer >= satietyRecoveryInterval)
            {
                float recoveryAmount = satietyRecoveryRate * (satietyRecoveryInterval / 60f);
                healthSystem.Heal(recoveryAmount);
                recoveryTimer = 0f;

                Debug.Log($"<color=green>[Recovery] Восстановление здоровья: +{recoveryAmount} HP</color>");
            }
        }
    }

    /// <summary>
    /// Поедание пищи - увеличивает сытость
    /// </summary>
    public void EatFood(float foodValue)
    {
        currentSatiety = Mathf.Min(maxSatiety, currentSatiety + foodValue);
        OnSatietyChanged?.Invoke(currentSatiety);

        Debug.Log($"<color=yellow>[Hunger] Съедено еды: +{foodValue}. Сытость: {currentSatiety}/{maxSatiety}</color>");
    }

    /// <summary>
    /// Получает процент текущей сытости (0-1)
    /// </summary>
    public float GetSatietyPercent()
    {
        return currentSatiety / maxSatiety;
    }

    /// <summary>
    /// Проверяет голодает ли персонаж
    /// </summary>
    public bool CheckIsStarving()
    {
        return isStarving;
    }

    /// <summary>
    /// Сбрасывает голод на максимум (для тестирования)
    /// </summary>
    public void ResetSatiety()
    {
        currentSatiety = maxSatiety;
        OnSatietyChanged?.Invoke(currentSatiety);
        Debug.Log($"[Hunger] Сытость сброшена на максимум: {maxSatiety}");
    }
}
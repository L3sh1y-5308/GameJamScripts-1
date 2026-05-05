using UnityEngine;
using UnityEngine.Events;

public class HungerSystem : MonoBehaviour
{
    [Header("Hunger Settings")]
    [SerializeField] private int maxSatiety = 100;
    [SerializeField] private int hungerDecayRate = 10; // единиц голода за интервал
    [SerializeField] private float hungerDecayInterval = 30f; // секунд между убыванием голода

    [Header("Starvation Damage")]
    [SerializeField] private int starvationDamageRate = 1; // урон при полном голоде
    [SerializeField] private float starvationDamageInterval = 1f; // частота урона

    [Header("Recovery Settings")]
    [SerializeField] private int satietyRecoveryRate = 5;
    [SerializeField] private float satietyRecoveryInterval = 1f;
    [SerializeField] private int recoveryHealthThreshold = 50; // порог сытости для лечения

    [Header("Events")]
    public UnityEvent<int> OnSatietyChanged;
    public UnityEvent OnStarving;

    private int currentSatiety;
    private float hungerTimer;
    private float starvationTimer;
    private float recoveryTimer;

    private HealfP healthSystem;
    private bool isStarving;
    private bool isInitialized = false;

    void Awake()
    {
        // Инициализируем сытость
        currentSatiety = maxSatiety;
        
        // Пытаемся найти систему здоровья (ОПЦИОНАЛЬНО)
        TryGetHealthSystem();
        
        isInitialized = true;
    }

    void Start()
    {
        // Дополнительная проверка в Start
        if (!isInitialized)
            TryGetHealthSystem();
    }

    void Update()
    {
        if (!isInitialized) return;

        UpdateHungerDecay();
        UpdateStarvationDamage();
        UpdateRecovery();
    }

    /// <summary>
    /// Пытаемся найти систему здоровья (без ошибок если не найдена)
    /// </summary>
    private void TryGetHealthSystem()
    {
        if (healthSystem == null)
        {
            healthSystem = GetComponent<HealfP>();
            
            if (healthSystem == null)
            {
                Debug.LogWarning($"⚠️ HealfP не найден на {gameObject.name}. Голод работает, но урон от голода отключен.");
            }
        }
    }

    /// <summary>
    /// Уменьшает сытость со временем
    /// </summary>
    private void UpdateHungerDecay()
    {
        hungerTimer += Time.deltaTime;

        if (hungerTimer >= hungerDecayInterval)
        {
            currentSatiety = Mathf.Max(0, currentSatiety - hungerDecayRate);

            // Вызываем события
            OnSatietyChanged?.Invoke(currentSatiety);

            hungerTimer = 0f;
            Debug.Log($"[Hunger] Текущая сытость: {currentSatiety}/{maxSatiety}");
        }
    }

    /// <summary>
    /// Наносит урон при голоде (если есть HealfP)
    /// </summary>
    private void UpdateStarvationDamage()
    {
        if (currentSatiety <= 0)
        {
            if (!isStarving)
            {
                isStarving = true;
                OnStarving?.Invoke();
                Debug.Log("<color=red>[Hunger] Голодание началось!</color>");
            }

            // Только если есть компонент здоровья
            if (healthSystem != null)
            {
                starvationTimer += Time.deltaTime;
                if (starvationTimer >= starvationDamageInterval)
                {
                    healthSystem.TakeDamage(starvationDamageRate);
                    starvationTimer = 0f;
                }
            }
        }
        else
        {
            isStarving = false;
        }
    }

    /// <summary>
    /// Лечение при высокой сытости (если есть HealfP)
    /// </summary>
    private void UpdateRecovery()
    {
        if (currentSatiety >= recoveryHealthThreshold && healthSystem != null)
        {
            recoveryTimer += Time.deltaTime;
            if (recoveryTimer >= satietyRecoveryInterval)
            {
                float recoveryAmount = satietyRecoveryRate * (satietyRecoveryInterval / 60f);
                healthSystem.Heal(recoveryAmount);
                recoveryTimer = 0f;
            }
        }
    }

    /// <summary>
    /// Функция для приема пищи
    /// </summary>
    public void EatFood(int foodValue)
    {
        currentSatiety = Mathf.Min(maxSatiety, currentSatiety + foodValue);
        OnSatietyChanged?.Invoke(currentSatiety);

        Debug.Log($"<color=yellow>[Hunger] Съедено: +{foodValue}. Текущая сытость: {currentSatiety}/{maxSatiety}</color>");
    }

    // ===== ПУБЛИЧНЫЕ МЕТОДЫ =====
    
    public float GetSatietyPercent() => (float)currentSatiety / maxSatiety;
    public int GetCurrentSatiety() => currentSatiety;
    public int GetMaxSatiety() => maxSatiety;
    public bool IsStarving() => isStarving;
    public bool CheckIsStarving() => isStarving;
    
    public void SetSatiety(int value)
    {
        currentSatiety = Mathf.Clamp(value, 0, maxSatiety);
        OnSatietyChanged?.Invoke(currentSatiety);
    }

    public void AddSatiety(int value)
    {
        currentSatiety = Mathf.Min(maxSatiety, currentSatiety + value);
        OnSatietyChanged?.Invoke(currentSatiety);
    }

    public void SubtractSatiety(int value)
    {
        currentSatiety = Mathf.Max(0, currentSatiety - value);
        OnSatietyChanged?.Invoke(currentSatiety);
    }

    // ОТЛАДКА
    void OnGUI()
    {
        if (GUILayout.Button($"Test Hunger: {GetSatietyPercent():P0}", GUILayout.Width(200), GUILayout.Height(30)))
        {
            Debug.Log($"Сытость: {currentSatiety}/{maxSatiety} ({GetSatietyPercent():P0})");
        }
    }
}
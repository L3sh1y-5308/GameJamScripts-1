using UnityEngine;
using UnityEngine.Events;

public class Food : MonoBehaviour
{
    [Header("Food Settings")]
    [SerializeField] float satietyValue = 25f; // сколько сытости даёт эта пища

    [Header("Events")]
    public UnityEvent<float> OnFoodConsumed;

    private HungerSystem hungerSystem;
    private Collider foodCollider;

    void Awake()
    {
        foodCollider = GetComponent<Collider>();
        if (foodCollider == null)
        {
            Debug.LogError($"Food на {gameObject.name} требует Collider компонент!");
        }
    }

    /// <summary>
    /// Получает количество сытости от этой пищи
    /// </summary>
    public float GetSatietyValue()
    {
        return satietyValue;
    }

    /// <summary>
    /// Устанавливает количество сытости (для редактирования в инспекторе)
    /// </summary>
    public void SetSatietyValue(float value)
    {
        satietyValue = Mathf.Max(0, value);
    }

    /// <summary>
    /// Поедание пищи персонажем
    /// </summary>
    public void Consume(HungerSystem consumer)
    {
        if (consumer == null)
        {
            Debug.LogWarning("Попытка съесть пищу, но потребитель (HungerSystem) не найден!");
            return;
        }

        hungerSystem = consumer;

        // Добавляем сытость
        hungerSystem.EatFood(satietyValue);
        OnFoodConsumed?.Invoke(satietyValue);

        Debug.Log($"<color=yellow>[Food] {gameObject.name} съедена! +{satietyValue} сытости</color>");

        // Удаляем объект еды со сцены
        Destroy(gameObject);
    }

    /// <summary>
    /// Триггер для автоматического поедания при касании
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        HungerSystem hungerSystem = other.GetComponent<HungerSystem>();

        if (hungerSystem != null)
        {
            Consume(hungerSystem);
        }
    }

    /// <summary>
    /// Информация о пище в консоль (для отладки)
    /// </summary>
    public void PrintFoodInfo()
    {
        Debug.Log($"[Food Info] {gameObject.name} - Satiety Value: {satietyValue}");
    }
}
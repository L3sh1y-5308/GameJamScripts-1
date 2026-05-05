using System.Collections.Generic;
using UnityEngine;

public class FeedOnKill : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HungerSystem hunger;

    [Header("Settings")]
    [Tooltip("Сколько сытости получает крыса за убийство еды")]
    [SerializeField] private int satietyPerKill = 30;

    [Tooltip("Радиус, в котором ищем еду перед атакой")]
    [SerializeField] private float checkRadius = 2f;

    [Tooltip("Тег еды (кого можно есть)")]
    [SerializeField] private string foodTag = "Food";

    private List<GameObject> trackedFood = new List<GameObject>();

    private void Awake()
    {
        if (hunger == null) hunger = GetComponent<HungerSystem>();
    }

    /// <summary>
    /// Вызывается ПЕРЕД атакой — запоминаем всю еду рядом.
    /// </summary>
    public void BeforeAttack()
    {
        trackedFood.Clear();
        Collider[] hits = Physics.OverlapSphere(transform.position, checkRadius);
        foreach (var col in hits)
        {
            if (col.CompareTag(foodTag))
                trackedFood.Add(col.gameObject);
        }
    }

    /// <summary>
    /// Вызывается ПОСЛЕ атаки — проверяем кого убили и восстанавливаем голод.
    /// </summary>
    public void AfterAttack()
    {
        foreach (var food in trackedFood)
        {
            if (food == null || !food.activeInHierarchy)
            {
                if (hunger != null)
                    hunger.EatFood(satietyPerKill);
                break; // одна порция за атаку
            }
        }
        trackedFood.Clear();
    }
}
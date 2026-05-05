using UnityEngine;
using Unity.Behavior; // Не забудьте подключить пространство имен

public class HungerManager : MonoBehaviour
{
    public BehaviorGraphAgent agent; // Ссылка на агента (назначьте в инспекторе)

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Устанавливаем значение переменной HungerStats
            // Убедитесь, что имя совпадает (регистр важен!)
            agent.SetVariableValue("HungerStats", 50);
            Debug.Log("Значение HungerStats изменено на 50");
        }
    }
}

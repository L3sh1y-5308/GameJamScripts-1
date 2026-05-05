using UnityEngine;
using UnityEngine.AI;

public class FleeBehavior : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;

    [Header("Distances")]
    [Tooltip("Комфортная дистанция от опасности — ближе паникуем")]
    [SerializeField] private float safeDistance = 5f;

    [Tooltip("Минимальная дистанция точки убегания от текущей позиции")]
    [SerializeField] private float minFleeDistance = 10f;

    [Tooltip("Максимальная дистанция точки убегания")]
    [SerializeField] private float maxFleeDistance = 20f;

    [Header("Panic")]
    [Tooltip("Сколько секунд враг должен быть близко, чтобы начать паниковать")]
    [SerializeField] private float panicDelay = 1.5f;

    [Tooltip("Сколько раз пробовать найти точку убегания")]
    [SerializeField] private int fleeAttempts = 10;

    private float panicTimer = 0f;
    private bool isCornered = false;

    /// <summary>
    /// Тикается каждый кадр пока DangerDetect=true.
    /// Возвращает текущую фазу: 0=держать дистанцию, 1=убегать, 2=загнан в угол
    /// </summary>
    public int Tick(Transform threat)
    {
        if (threat == null) return 0;

        float dist = Vector3.Distance(transform.position, threat.position);

        // Враг далеко — спокойно держим дистанцию
        if (dist >= safeDistance)
        {
            panicTimer = 0f;
            isCornered = false;
            return 0;
        }

        // Враг близко — копим панику
        panicTimer += Time.deltaTime;

        if (panicTimer < panicDelay)
            return 0; // ещё держимся

        // Паника! Пытаемся убежать
        if (TryFleeTo(threat.position))
        {
            isCornered = false;
            return 1; // убегаем
        }

        // Не нашли куда бежать — мы загнаны
        isCornered = true;
        return 2; // атакуем в отчаянии
    }

    private bool TryFleeTo(Vector3 threatPos)
    {
        Vector3 awayDir = (transform.position - threatPos).normalized;

        for (int i = 0; i < fleeAttempts; i++)
        {
            // Случайное направление, но в основном "от врага"
            Vector3 randomDir = Quaternion.Euler(0, Random.Range(-60f, 60f), 0) * awayDir;
            float dist = Random.Range(minFleeDistance, maxFleeDistance);
            Vector3 candidate = transform.position + randomDir * dist;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                // Проверяем, что путь действительно достижим
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(hit.position);
                    return true;
                }
            }
        }

        return false; // нигде нет валидной точки → загнаны
    }

    public bool IsCornered() => isCornered;

    public void ResetPanic()
    {
        panicTimer = 0f;
        isCornered = false;
    }
}
using UnityEngine;
using UnityEngine.Events;

public class HitBox_Receiver : HitBox_Active
{
    [Header("Receiver Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool canTakeDamage = true;
    [SerializeField] private float invulnerabilityTime = 0.5f;
    
    [Header("Events")]
    public UnityEvent<float> OnDamageTaken;
    public UnityEvent OnDeath;

    private float lastHitTime;
    private bool isDead = false;

    protected override void Start()
    {
        base.Start();
        currentHealth = maxHealth;
    }

    public override bool IsHitBoxColides()
    {
        return hitBoxData.hasHit;
    }

    protected override void PerformHitBoxDetection()
    {
        // Receiver обычно не выполняет активное обнаружение
        // Он реагирует на вызовы TakeDamage
    }

    public void TakeDamage(float damage, Vector3 damageSource)
    {
        if (!canTakeDamage || isDead)
            return;

        // Проверка на неуязвимость после последнего удара
        if (Time.time - lastHitTime < invulnerabilityTime)
            return;

        lastHitTime = Time.time;
        currentHealth -= damage;

        // Создаем HitBoxData для информации об ударе
        hitBoxData.hasHit = true;
        hitBoxData.hitPoint = transform.position;
        hitBoxData.hitNormal = (transform.position - damageSource).normalized;
        hitBoxData.distance = Vector3.Distance(transform.position, damageSource);

        Debug.Log($"{gameObject.name} получил {damage} урона. Осталось HP: {currentHealth}");

        OnDamageTaken?.Invoke(damage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        canTakeDamage = false;

        Debug.Log($"{gameObject.name} умер!");
        OnDeath?.Invoke();

        // Дополнительная логика смерти (анимация, отключение и т.д.)
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"{gameObject.name} вылечен на {amount}. HP: {currentHealth}");
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void OnDrawGizmos()
    {
        // Визуализация здоровья
        Gizmos.color = isDead ? Color.black : Color.Lerp(Color.red, Color.green, currentHealth / maxHealth);
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}
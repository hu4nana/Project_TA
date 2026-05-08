using UnityEngine;

[RequireComponent(typeof(EnemyStaggerController))]
public class EnemyHitReceiver : MonoBehaviour, IDamageable
{
    [SerializeField] int maxHP = 3;
    [SerializeField] float staggerDuration = 0.25f;

    int currentHP;
    EnemyStaggerController staggerController;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    void Awake()
    {
        currentHP = maxHP;
        staggerController = GetComponent<EnemyStaggerController>();
    }

    void Update()
    {
        staggerController?.Tick(Time.deltaTime);
    }

    public void TakeDamage(DamageInfo info)
    {
        currentHP -= info.damage;
        staggerController?.Stagger(staggerDuration);

        if (currentHP <= 0)
            gameObject.SetActive(false);
    }
}

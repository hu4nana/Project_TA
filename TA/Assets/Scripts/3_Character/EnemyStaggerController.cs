using UnityEngine;

public class EnemyStaggerController : MonoBehaviour
{
    [SerializeField] float defaultStaggerDuration = 0.35f;

    float staggerTimer;

    public bool IsStaggered => staggerTimer > 0f;

    public void Tick(float deltaTime)
    {
        if (staggerTimer > 0f)
            staggerTimer -= deltaTime;
    }

    public void Stagger(float duration = -1f)
    {
        staggerTimer = duration > 0f ? duration : defaultStaggerDuration;
    }
}

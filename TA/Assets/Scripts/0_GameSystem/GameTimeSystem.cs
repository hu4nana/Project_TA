using UnityEngine;

public class GameTimeSystem : MonoBehaviour
{
    public static GameTimeSystem Instance { get; private set; }

    [SerializeField] float counterTimeScale = 0f;

    float defaultFixedDeltaTime;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void EnterCounterFreeze()
    {
        SetTimeScale(counterTimeScale);
    }

    public void ExitCounterFreeze()
    {
        SetTimeScale(1f);
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * Mathf.Max(scale, 0.0001f);
    }
}

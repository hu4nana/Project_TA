using UnityEngine;

public class CounterWindowSystem : MonoBehaviour
{
    public static CounterWindowSystem Instance { get; private set; }

    [SerializeField] float defaultDuration = 1.5f;

    float remainingTime;
    bool freeSkillAvailable;

    public bool IsOpen => remainingTime > 0f;
    public bool FreeSkillAvailable => freeSkillAvailable;
    public CounterTriggerType LastTrigger { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        if (!IsOpen)
            return;

        remainingTime -= Time.unscaledDeltaTime;
        if (remainingTime <= 0f)
            Close();
    }

    public void Open(CounterTriggerType triggerType, float duration = -1f)
    {
        LastTrigger = triggerType;
        remainingTime = duration > 0f ? duration : defaultDuration;
        freeSkillAvailable = true;
        GameTimeSystem.Instance?.EnterCounterFreeze();
    }

    public bool TryConsumeFreeSkill()
    {
        if (!freeSkillAvailable)
            return false;

        freeSkillAvailable = false;
        Close();
        return true;
    }

    public void Close()
    {
        remainingTime = 0f;
        freeSkillAvailable = false;
        GameTimeSystem.Instance?.ExitCounterFreeze();
    }
}

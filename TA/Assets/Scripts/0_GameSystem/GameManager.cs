using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] bool debugMode = true;

    public static bool DebugMode => Instance != null && Instance.debugMode;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetDebugMode(bool enabled)
    {
        debugMode = enabled;
    }
}

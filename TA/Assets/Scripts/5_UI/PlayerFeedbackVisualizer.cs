using UnityEngine;

public class PlayerFeedbackVisualizer : MonoBehaviour
{
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color parryColor = Color.cyan;
    [SerializeField] Color dodgeColor = Color.green;
    [SerializeField] Color counterColor = Color.yellow;

    Player player;
    PlayerDefense defense;
    SpriteRenderer spriteRenderer;
    GUIStyle labelStyle;

    void Awake()
    {
        player = GetComponentInParent<Player>();
        Transform root = player != null ? player.transform : transform;
        defense = root.GetComponentInChildren<PlayerDefense>(true);
        spriteRenderer = root.GetComponentInChildren<SpriteRenderer>(true);
    }

    void LateUpdate()
    {
        if (spriteRenderer == null)
            return;

        if (CounterWindowSystem.Instance != null && CounterWindowSystem.Instance.IsOpen)
            spriteRenderer.color = counterColor;
        else if (defense != null && defense.IsParrying)
            spriteRenderer.color = parryColor;
        else if (defense != null && defense.IsInvincible)
            spriteRenderer.color = dodgeColor;
        else
            spriteRenderer.color = normalColor;
    }

    void OnGUI()
    {
        if (!GameManager.DebugMode || player == null)
            return;

        string text = null;
        if (CounterWindowSystem.Instance != null && CounterWindowSystem.Instance.IsOpen)
            text = CounterWindowSystem.Instance.LastTrigger == CounterTriggerType.Parry ? "COUNTER READY" : "DODGE COUNTER";
        else if (defense != null && defense.IsParrying)
            text = "PARRY";
        else if (defense != null && defense.IsInvincible)
            text = "DODGE";

        if (string.IsNullOrEmpty(text))
            return;

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 16
            };
            labelStyle.normal.textColor = Color.white;
        }

        Vector3 worldPosition = player != null ? player.transform.position : transform.position;
        Vector3 screen = Camera.main != null ? Camera.main.WorldToScreenPoint(worldPosition + Vector3.up * 1.8f) : Vector3.zero;
        if (screen.z < 0f)
            return;

        Rect rect = new Rect(screen.x - 80f, Screen.height - screen.y - 20f, 160f, 24f);
        GUI.Label(rect, text, labelStyle);
    }
}

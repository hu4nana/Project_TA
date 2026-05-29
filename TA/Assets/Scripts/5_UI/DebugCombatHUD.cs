using UnityEngine;

public class DebugCombatHUD : MonoBehaviour
{
    Player player;
    PlayerResourceController resources;
    EnemyHitReceiver dummy;
    GUIStyle titleStyle;
    GUIStyle labelStyle;

    void Update()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
            resources = player != null ? player.GetComponentInChildren<PlayerResourceController>(true) : null;
        }

        if (dummy == null)
        {
            GameObject target = GameObject.Find("TrainingDummy");
            dummy = target != null ? target.GetComponent<EnemyHitReceiver>() : null;
        }
    }

    void OnGUI()
    {
        if (!GameManager.DebugMode)
            return;

        EnsureStyle();

        float atp = resources != null ? resources.CurrentATP : 0f;
        float maxATP = resources != null ? resources.MaxATP : 0f;
        string dummyHp = dummy != null && dummy.gameObject.activeInHierarchy
            ? $"{dummy.CurrentHP}/{dummy.MaxHP}"
            : "DEAD";
        string counter = CounterWindowSystem.Instance != null && CounterWindowSystem.Instance.IsOpen
            ? CounterWindowSystem.Instance.LastTrigger.ToString()
            : "None";
        string movement = player != null ? player.movementState.ToString() : "None";
        string action = player != null ? player.actionState.ToString() : "None";
        string grounded = player != null && player.IsGrounded ? "Yes" : "No";
        string jumps = player != null ? $"{player.AirJumpRemaining}/{player.MaxAirJumpCount}" : "0/0";
        string condition = player != null ? player.conditionState.ToString() : "None";
        string vertical = player != null ? player.Rigidbody.linearVelocity.y.ToString("0.00") : "0.00";

        float width = 330f;
        float height = 132f;
        float x = Screen.width - width - 16f;
        float y = 16f;

        Rect panel = new Rect(x, y, width, height);
        GUI.Box(panel, "Debug Combat HUD");
        GUI.Label(new Rect(x + 10f, y + 24f, 305f, 18f), $"ATP: {atp:0}/{maxATP:0}", titleStyle);
        GUI.Label(new Rect(x + 10f, y + 42f, 305f, 18f), $"Dummy HP: {dummyHp}", labelStyle);
        GUI.Label(new Rect(x + 10f, y + 60f, 305f, 18f), $"Counter: {counter}", labelStyle);
        GUI.Label(new Rect(x + 10f, y + 78f, 305f, 18f), $"Move: {movement} / Act: {action} / Cond: {condition}", labelStyle);
        GUI.Label(new Rect(x + 10f, y + 96f, 305f, 18f), $"Ground: {grounded} / AirJump: {jumps} / VelY: {vertical}", labelStyle);
        GUI.Label(new Rect(x + 10f, y + 114f, 305f, 18f), "Keys: ←→ Move / Z Jump / X Attack / ↑ Talk", labelStyle);
    }

    void EnsureStyle()
    {
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 11 };
        titleStyle.normal.textColor = Color.white;

        labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 10 };
        labelStyle.normal.textColor = Color.white;
    }
}

using UnityEngine;

public class DebugCombatHUD : MonoBehaviour
{
    Player player;
    PlayerResourceController resources;
    EnemyHitReceiver dummy;
    GUIStyle titleStyle;

    void Update()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
            resources = player != null ? player.GetComponent<PlayerResourceController>() : null;
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
        string jumps = player != null ? $"{player.jumpChance}/{player.maxJumpChance}" : "0/0";
        string condition = player != null ? player.conditionState.ToString() : "None";
        string vertical = player != null ? player.Rigidbody.linearVelocity.y.ToString("0.00") : "0.00";

        Rect panel = new Rect(20f, Screen.height - 194f, 400f, 166f);
        GUI.Box(panel, "Debug Combat HUD");
        GUI.Label(new Rect(32f, Screen.height - 162f, 360f, 22f), $"ATP: {atp:0}/{maxATP:0}", titleStyle);
        GUI.Label(new Rect(32f, Screen.height - 138f, 360f, 22f), $"Training Dummy HP: {dummyHp}");
        GUI.Label(new Rect(32f, Screen.height - 114f, 360f, 22f), $"Counter Window: {counter}");
        GUI.Label(new Rect(32f, Screen.height - 90f, 360f, 22f), $"Movement: {movement} / Action: {action} / Condition: {condition}");
        GUI.Label(new Rect(32f, Screen.height - 66f, 360f, 22f), $"Grounded: {grounded} / JumpChance: {jumps} / VelY: {vertical}");
        GUI.Label(new Rect(32f, Screen.height - 42f, 360f, 22f), "Keys: Arrow Move / Z Jump / Space Parry / C Dodge / 1~3 Skill");
    }

    void EnsureStyle()
    {
        if (titleStyle != null)
            return;

        titleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
        titleStyle.normal.textColor = Color.white;
    }
}

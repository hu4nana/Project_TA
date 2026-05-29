using UnityEngine;

public class SkillSelectionUI : MonoBehaviour
{
    PlayerSkillLoadout loadout;
    PlayerSkillCaster caster;
    GUIStyle titleStyle;
    GUIStyle bannerStyle;
    GUIStyle selectedStyle;

    void Update()
    {
        if (loadout == null)
        {
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                loadout = player.GetComponentInChildren<PlayerSkillLoadout>(true);
                caster = player.GetComponentInChildren<PlayerSkillCaster>(true);
            }
        }
    }

    void OnGUI()
    {
        if (!GameManager.DebugMode)
            return;

        if (CounterWindowSystem.Instance == null || !CounterWindowSystem.Instance.IsOpen || loadout == null)
            return;

        EnsureStyles();
        DrawOverlay();

        string banner = CounterWindowSystem.Instance.LastTrigger == CounterTriggerType.Parry ? "PARRY!" : "PERFECT DODGE!";
        GUI.Label(new Rect(0f, 40f, Screen.width, 36f), banner, bannerStyle);

        Rect panel = new Rect(20f, 20f, 320f, 48f + loadout.EquippedSkills.Count * 28f);
        GUI.Box(panel, $"Counter: {CounterWindowSystem.Instance.LastTrigger}");
        GUI.Label(new Rect(32f, 48f, 280f, 24f), "Choose a skill with 1 / 2 / 3", titleStyle);

        for (int i = 0; i < loadout.EquippedSkills.Count; i++)
        {
            SkillBehaviour skill = loadout.EquippedSkills[i];
            if (skill == null)
                continue;

            float cooldown = caster != null ? caster.GetCooldownRemaining(i) : 0f;
            string text = $"{i + 1}. {skill.SkillName}  Cost:{skill.AtpCost:0}";
            if (CounterWindowSystem.Instance.FreeSkillAvailable)
                text += "  FREE";
            else if (cooldown > 0f)
                text += $"  CD:{cooldown:0.0}";

            GUIStyle style = i == 0 ? selectedStyle : GUI.skin.label;
            GUI.Label(new Rect(32f, 76f + i * 28f, 280f, 24f), text, style);
        }
    }

    void DrawOverlay()
    {
        Color old = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.35f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = old;
    }

    void EnsureStyles()
    {
        if (titleStyle == null)
        {
            titleStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = Color.white;
        }

        if (bannerStyle == null)
        {
            bannerStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold
            };
            bannerStyle.normal.textColor = Color.yellow;
        }

        if (selectedStyle == null)
        {
            selectedStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            selectedStyle.normal.textColor = Color.cyan;
        }
    }
}

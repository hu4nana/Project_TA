using UnityEngine;

public class RuntimeDebugOverlay : MonoBehaviour
{
    static Texture2D lineTexture;
    GUIStyle labelStyle;

    void OnGUI()
    {
        if (!GameManager.DebugMode)
            return;

        EnsureResources();
        DrawPlayerInfo();
        DrawEnemyInfo();
        DrawSkillInfo();
    }

    void DrawPlayerInfo()
    {
        Player player = FindFirstObjectByType<Player>();
        if (player == null)
            return;

        BoxCollider2D playerCollider = player.GetComponent<BoxCollider2D>();
        if (playerCollider != null)
            DrawBounds(playerCollider.bounds, Color.cyan, "Player Body");

        DrawBox(player.GroundCheckCenter, player.GroundCheckSize, Color.green, "GroundCheck");
    }

    void DrawEnemyInfo()
    {
        EnemyHitReceiver[] enemies = FindObjectsByType<EnemyHitReceiver>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            BoxCollider2D col = enemies[i].GetComponent<BoxCollider2D>();
            if (col != null && enemies[i].gameObject.activeInHierarchy)
                DrawBounds(col.bounds, Color.red, enemies[i].name);
        }

        EnemyAttackEmitter[] emitters = FindObjectsByType<EnemyAttackEmitter>(FindObjectsSortMode.None);
        for (int i = 0; i < emitters.Length; i++)
        {
            BoxCollider2D col = emitters[i].GetComponent<BoxCollider2D>();
            if (col != null && emitters[i].gameObject.activeInHierarchy)
                DrawBounds(col.bounds, new Color(1f, 0f, 1f, 1f), emitters[i].name);
        }
    }

    void DrawSkillInfo()
    {
        if (!MeleeSkillBehaviour.HasDebugHitbox)
            return;

        DrawBox(MeleeSkillBehaviour.DebugHitboxCenter, MeleeSkillBehaviour.DebugHitboxSize, Color.yellow, "Skill Hitbox");
    }

    void DrawBounds(Bounds bounds, Color color, string label)
    {
        DrawBox(bounds.center, bounds.size, color, label);
    }

    void DrawBox(Vector2 center, Vector2 size, Color color, string label)
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        Vector3 min = cam.WorldToScreenPoint(new Vector3(center.x - size.x * 0.5f, center.y - size.y * 0.5f, 0f));
        Vector3 max = cam.WorldToScreenPoint(new Vector3(center.x + size.x * 0.5f, center.y + size.y * 0.5f, 0f));
        if (min.z < 0f || max.z < 0f)
            return;

        Rect rect = Rect.MinMaxRect(min.x, Screen.height - max.y, max.x, Screen.height - min.y);
        DrawRectOutline(rect, color, 2f);
        GUI.Label(new Rect(rect.xMin, rect.yMin - 18f, 160f, 18f), label, labelStyle);
    }

    void DrawRectOutline(Rect rect, Color color, float thickness)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), lineTexture);
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), lineTexture);
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), lineTexture);
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), lineTexture);
        GUI.color = old;
    }

    void EnsureResources()
    {
        if (lineTexture == null)
        {
            lineTexture = new Texture2D(1, 1);
            lineTexture.SetPixel(0, 0, Color.white);
            lineTexture.Apply();
        }

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 12;
        }
    }
}

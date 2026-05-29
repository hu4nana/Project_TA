#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class DialogueSceneSetup
{
    const string GeneratedFolder = "Assets/Generated/Dialogue";
    const string NpcSpritePath = GeneratedFolder + "/DialogueNpcSquare.png";

    public static string SetupActiveScene()
    {
        EnsureGeneratedAssets();
        EnsurePlayerInteractor();
        EnsureDialogueSystem();
        EnsureTestNpc();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();

        return "Dialogue scene setup complete.";
    }

    static void EnsureGeneratedAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Generated"))
            AssetDatabase.CreateFolder("Assets", "Generated");

        if (!AssetDatabase.IsValidFolder(GeneratedFolder))
            AssetDatabase.CreateFolder("Assets/Generated", "Dialogue");

        if (!File.Exists(NpcSpritePath))
        {
            Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;

            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(NpcSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(NpcSpritePath);
        }

        TextureImporter importer = AssetImporter.GetAtPath(NpcSpritePath) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32f;
            importer.SaveAndReimport();
        }
    }

    static void EnsurePlayerInteractor()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null || player.GetComponentInChildren<PlayerInteractor>(true) != null)
            return;

        Transform systems = player.transform.Find("Systems");
        if (systems == null)
        {
            GameObject systemsObject = new("Systems");
            systemsObject.transform.SetParent(player.transform, false);
            systems = systemsObject.transform;
        }

        GameObject interactionObject = new("06_Interaction");
        interactionObject.transform.SetParent(systems, false);
        interactionObject.AddComponent<PlayerInteractor>();
    }

    static void EnsureDialogueSystem()
    {
        GameObject system = GameObject.Find("DialogueSystem");
        if (system == null)
            system = new GameObject("DialogueSystem");

        DialogueController controller = system.GetComponent<DialogueController>();
        if (controller == null)
            controller = system.AddComponent<DialogueController>();

        DialogueUI dialogueUI = system.GetComponent<DialogueUI>();
        if (dialogueUI == null)
            dialogueUI = system.AddComponent<DialogueUI>();

        Transform oldCanvas = system.transform.Find("DialogueCanvas");
        if (oldCanvas != null)
            Object.DestroyImmediate(oldCanvas.gameObject);

        GameObject canvasObject = new GameObject("DialogueCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(system.transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("DialoguePanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.zero;
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(560f, 170f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.02f, 0.02f, 0.025f, 0.92f);

        TMP_Text speakerText = CreateUIText("SpeakerNameText", panel.transform, "NPC", 34, TextAlignmentOptions.Left);
        RectTransform speakerRect = speakerText.GetComponent<RectTransform>();
        speakerRect.anchorMin = new Vector2(0f, 0.72f);
        speakerRect.anchorMax = new Vector2(1f, 1f);
        speakerRect.offsetMin = new Vector2(36f, 0f);
        speakerRect.offsetMax = new Vector2(-36f, -18f);
        speakerText.color = new Color(1f, 0.35f, 0.35f, 1f);
        speakerText.fontSize = 28f;

        TMP_Text bodyText = CreateUIText("BodyText", panel.transform, string.Empty, 42, TextAlignmentOptions.TopLeft);
        RectTransform bodyRect = bodyText.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 0.72f);
        bodyRect.offsetMin = new Vector2(36f, 28f);
        bodyRect.offsetMax = new Vector2(-36f, -8f);
        bodyText.color = Color.white;
        bodyText.fontSize = 32f;

        SerializedObject uiSerialized = new SerializedObject(dialogueUI);
        uiSerialized.FindProperty("panel").objectReferenceValue = panel;
        uiSerialized.FindProperty("speakerText").objectReferenceValue = speakerText;
        uiSerialized.FindProperty("bodyText").objectReferenceValue = bodyText;
        uiSerialized.FindProperty("screenOffset").vector2Value = new Vector2(0f, 0f);
        uiSerialized.FindProperty("minPanelSize").vector2Value = new Vector2(360f, 120f);
        uiSerialized.FindProperty("maxPanelSize").vector2Value = new Vector2(720f, 360f);
        uiSerialized.FindProperty("panelPadding").vector2Value = new Vector2(72f, 96f);
        uiSerialized.FindProperty("maxTextWidth").floatValue = 640f;
        uiSerialized.FindProperty("animateCharacters").boolValue = true;
        uiSerialized.FindProperty("characterWaveAmplitude").floatValue = 1.6f;
        uiSerialized.FindProperty("characterWaveSpeed").floatValue = 18f;
        uiSerialized.ApplyModifiedPropertiesWithoutUndo();

        SerializedObject controllerSerialized = new SerializedObject(controller);
        controllerSerialized.FindProperty("dialogueUI").objectReferenceValue = dialogueUI;
        controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

        panel.SetActive(false);
    }

    static TMP_Text CreateUIText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        TMP_Text label = go.GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.raycastTarget = false;
        return label;
    }

    static void EnsureTestNpc()
    {
        GameObject npc = GameObject.Find("DialogueTestNPC");
        if (npc == null)
            npc = new GameObject("DialogueTestNPC");

        GameObject player = GameObject.Find("Player");
        Vector3 position = player != null ? player.transform.position + new Vector3(3f, 0f, 0f) : new Vector3(3f, 0f, 0f);
        position.z = 0f;
        npc.transform.position = position;

        SpriteRenderer renderer = npc.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = npc.AddComponent<SpriteRenderer>();

        renderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(NpcSpritePath);
        renderer.color = new Color(0.35f, 0.75f, 1f, 1f);
        renderer.sortingOrder = 2;

        BoxCollider2D collider = npc.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = npc.AddComponent<BoxCollider2D>();

        collider.isTrigger = true;
        collider.size = new Vector2(2.2f, 2.2f);
        collider.offset = Vector2.zero;

        NpcDialogue dialogue = npc.GetComponent<NpcDialogue>();
        if (dialogue == null)
            dialogue = npc.AddComponent<NpcDialogue>();

        GameObject prompt = EnsurePrompt(npc.transform);
        Transform dialogueAnchor = EnsureDialogueAnchor(npc.transform);

        SerializedObject serialized = new SerializedObject(dialogue);
        serialized.FindProperty("speakerName").stringValue = "NPC";
        serialized.FindProperty("promptText").stringValue = "대화";
        serialized.FindProperty("promptObject").objectReferenceValue = prompt;
        serialized.FindProperty("dialogueAnchor").objectReferenceValue = dialogueAnchor;

        SerializedProperty lines = serialized.FindProperty("lines");
        lines.arraySize = 2;
        lines.GetArrayElementAtIndex(0).stringValue = "Test Text 1.";
        lines.GetArrayElementAtIndex(1).stringValue = "Test Text End";
        serialized.ApplyModifiedPropertiesWithoutUndo();

        prompt.SetActive(false);
    }

    static Transform EnsureDialogueAnchor(Transform npc)
    {
        Transform anchor = npc.Find("DialogueAnchor");
        if (anchor == null)
        {
            GameObject anchorObject = new GameObject("DialogueAnchor");
            anchor = anchorObject.transform;
            anchor.SetParent(npc, false);
        }

        anchor.localPosition = new Vector3(0f, 1.45f, 0f);
        return anchor;
    }

    static GameObject EnsurePrompt(Transform npc)
    {
        Transform oldPrompt = npc.Find("InteractionPrompt");
        if (oldPrompt != null)
            Object.DestroyImmediate(oldPrompt.gameObject);

        GameObject prompt = new GameObject("InteractionPrompt", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        prompt.transform.SetParent(npc, false);
        prompt.transform.localPosition = new Vector3(0f, 1.45f, 0f);
        prompt.transform.localScale = Vector3.one * 0.01f;

        Canvas canvas = prompt.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 50;

        RectTransform canvasRect = prompt.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(160f, 48f);

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        background.transform.SetParent(prompt.transform, false);

        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        Image bg = background.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.7f);

        TMP_Text text = CreateUIText("Text", background.transform, "대화", 32, TextAlignmentOptions.Center);
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        text.color = Color.white;

        return prompt;
    }
}
#endif

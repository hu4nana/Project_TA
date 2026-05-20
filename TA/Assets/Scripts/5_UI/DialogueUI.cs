using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TMP_Text speakerText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] Vector2 screenOffset = new(0f, 0f);
    [SerializeField] Vector2 minPanelSize = new(360f, 120f);
    [SerializeField] Vector2 maxPanelSize = new(720f, 360f);
    [SerializeField] Vector2 panelPadding = new(72f, 96f);
    [SerializeField] float maxTextWidth = 640f;
    [SerializeField] float characterDelay = 0.025f;
    [SerializeField] float punctuationDelay = 0.12f;
    [SerializeField] bool animateCharacters = true;
    [SerializeField] float characterWaveAmplitude = 1.6f;
    [SerializeField] float characterWaveSpeed = 18f;

    Coroutine typingRoutine;
    Transform followTarget;
    string fullLine;

    public bool IsTyping { get; private set; }

    void Awake()
    {
        Close();
    }

    void LateUpdate()
    {
        UpdatePanelPosition();
        AnimateCharacters();
    }

    public void Open()
    {
        Open(null);
    }

    public void Open(Transform target)
    {
        followTarget = target;

        if (panel != null)
        {
            panel.SetActive(true);
            UpdatePanelPosition();
        }
    }

    public void Close()
    {
        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        IsTyping = false;
        fullLine = string.Empty;
        followTarget = null;

        if (bodyText != null)
            bodyText.text = string.Empty;

        if (panel != null)
            panel.SetActive(false);
    }

    public void ShowLine(string speakerName, string line)
    {
        if (speakerText != null)
            speakerText.text = speakerName;

        fullLine = line ?? string.Empty;
        ResizePanel(fullLine);

        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = StartCoroutine(TypeLine());
    }

    public void CompleteLine()
    {
        if (!IsTyping)
            return;

        if (typingRoutine != null)
        {
            StopCoroutine(typingRoutine);
            typingRoutine = null;
        }

        if (bodyText != null)
            bodyText.text = fullLine;

        IsTyping = false;
    }

    IEnumerator TypeLine()
    {
        IsTyping = true;

        if (bodyText != null)
            bodyText.text = string.Empty;

        for (int i = 0; i < fullLine.Length; i++)
        {
            if (bodyText != null)
                bodyText.text = fullLine.Substring(0, i + 1);

            char c = fullLine[i];
            float delay = IsPunctuation(c) ? punctuationDelay : characterDelay;
            yield return new WaitForSecondsRealtime(delay);
        }

        IsTyping = false;
        typingRoutine = null;
    }

    void ResizePanel(string line)
    {
        RectTransform panelRect = panel != null ? panel.transform as RectTransform : null;
        if (panelRect == null || bodyText == null)
            return;

        Vector2 preferred = bodyText.GetPreferredValues(line, maxTextWidth, 0f);
        float width = Mathf.Clamp(preferred.x + panelPadding.x, minPanelSize.x, maxPanelSize.x);
        float availableTextWidth = Mathf.Max(1f, width - panelPadding.x);

        preferred = bodyText.GetPreferredValues(line, availableTextWidth, 0f);
        float height = Mathf.Clamp(preferred.y + panelPadding.y, minPanelSize.y, maxPanelSize.y);

        panelRect.sizeDelta = new Vector2(width, height);
    }

    void AnimateCharacters()
    {
        if (!animateCharacters || bodyText == null || !bodyText.gameObject.activeInHierarchy || string.IsNullOrEmpty(bodyText.text))
            return;

        bodyText.ForceMeshUpdate();

        TMP_TextInfo textInfo = bodyText.textInfo;
        float time = Time.unscaledTime;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo character = textInfo.characterInfo[i];
            if (!character.isVisible)
                continue;

            int materialIndex = character.materialReferenceIndex;
            int vertexIndex = character.vertexIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            float yOffset = Mathf.Sin(time * characterWaveSpeed + i * 0.85f) * characterWaveAmplitude;
            Vector3 offset = new Vector3(0f, yOffset, 0f);

            vertices[vertexIndex] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            bodyText.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    void UpdatePanelPosition()
    {
        if (panel == null || followTarget == null)
            return;

        RectTransform panelRect = panel.transform as RectTransform;
        Camera camera = Camera.main;
        if (panelRect == null || camera == null)
            return;

        Vector3 worldPosition = GetTargetTopPosition();
        Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition);
        if (screenPosition.z < 0f)
            return;

        panelRect.position = new Vector3(screenPosition.x + screenOffset.x, screenPosition.y + screenOffset.y, 0f);
    }

    Vector3 GetTargetTopPosition()
    {
        Collider2D col = followTarget.GetComponent<Collider2D>();
        if (col != null)
            return new Vector3(col.bounds.center.x, col.bounds.max.y, followTarget.position.z);

        Renderer renderer = followTarget.GetComponentInChildren<Renderer>();
        if (renderer != null)
            return new Vector3(renderer.bounds.center.x, renderer.bounds.max.y, followTarget.position.z);

        return followTarget.position;
    }

    bool IsPunctuation(char c)
    {
        return c == '.' || c == ',' || c == '!' || c == '?' || c == '…' || c == '。' || c == '、';
    }
}

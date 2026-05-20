using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusHUD : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] PlayerResourceController resources;
    [SerializeField] Image hpFill;
    [SerializeField] Image atpFill;

    void Awake()
    {
        BindReferences();
        BindFillImages();
    }

    void Update()
    {
        BindReferences();

        SetFill(hpFill, player != null && player.MaxHP > 0 ? (float)player.HP / player.MaxHP : 0f);
        SetFill(atpFill, resources != null && resources.MaxATP > 0f ? resources.CurrentATP / resources.MaxATP : 0f);
    }

    void BindReferences()
    {
        if (player == null)
            player = FindFirstObjectByType<Player>();

        if (resources == null && player != null)
            resources = player.GetComponent<PlayerResourceController>();
    }

    void BindFillImages()
    {
        if (hpFill == null)
            hpFill = FindFill("HP Bar/HP Fill");

        if (atpFill == null)
            atpFill = FindFill("ATP Bar/ATP Fill");
    }

    Image FindFill(string path)
    {
        Transform target = transform.Find(path);
        return target != null ? target.GetComponent<Image>() : null;
    }

    void SetFill(Image fill, float value)
    {
        if (fill == null)
            return;

        value = Mathf.Clamp01(value);
        fill.type = Image.Type.Simple;
        fill.fillAmount = 1f;
        fill.rectTransform.anchorMax = new Vector2(value, fill.rectTransform.anchorMax.y);
    }

#if UNITY_EDITOR
    void Reset()
    {
        if (hpFill == null)
            hpFill = CreateBar("HP", new Vector2(24f, -24f), Color.red);

        if (atpFill == null)
            atpFill = CreateBar("ATP", new Vector2(24f, -52f), Color.cyan);
    }

    Image CreateBar(string barName, Vector2 anchoredPosition, Color fillColor)
    {
        GameObject background = new(barName + " Bar");
        background.transform.SetParent(transform, false);

        RectTransform bgRect = background.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.pivot = new Vector2(0f, 1f);
        bgRect.anchoredPosition = anchoredPosition;
        bgRect.sizeDelta = new Vector2(220f, 18f);

        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject fill = new(barName + " Fill");
        fill.transform.SetParent(background.transform, false);

        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(2f, 2f);
        fillRect.offsetMax = new Vector2(-2f, -2f);

        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 1f;

        return fillImage;
    }
#endif
}

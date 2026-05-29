using System.Collections;
using UnityEngine;
using MoreMountains.Feedbacks;

public class PlayerFeedbacks : MonoBehaviour
{
    [Header("Optional MMFeedback Players")]
    [SerializeField] MMF_Player parryStart;
    [SerializeField] MMF_Player parrySuccess;
    [SerializeField] MMF_Player dodgeStart;
    [SerializeField] MMF_Player perfectDodge;
    [SerializeField] MMF_Player basicAttack;
    [SerializeField] MMF_Player skillCast;
    [SerializeField] MMF_Player hitTaken;

    [Header("Fallback Flash")]
    [SerializeField] SpriteRenderer targetRenderer;
    [SerializeField] Color parryStartColor = Color.cyan;
    [SerializeField] Color parrySuccessColor = new(0.1f, 0.7f, 1f, 1f);
    [SerializeField] Color dodgeStartColor = Color.green;
    [SerializeField] Color perfectDodgeColor = Color.yellow;
    [SerializeField] Color hitTakenColor = Color.red;
    [SerializeField] float flashDuration = 0.08f;

    Color baseColor = Color.white;
    Coroutine flashRoutine;

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInParent<SpriteRenderer>();

        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<SpriteRenderer>(true);

        if (targetRenderer != null)
            baseColor = targetRenderer.color;
    }

    public void PlayParryStart()
    {
        parryStart?.PlayFeedbacks();
        Flash(parryStartColor);
    }

    public void PlayParrySuccess()
    {
        parrySuccess?.PlayFeedbacks();
        Flash(parrySuccessColor, flashDuration * 1.5f);
    }

    public void PlayDodgeStart()
    {
        dodgeStart?.PlayFeedbacks();
        Flash(dodgeStartColor);
    }

    public void PlayPerfectDodge()
    {
        perfectDodge?.PlayFeedbacks();
        Flash(perfectDodgeColor, flashDuration * 1.5f);
    }

    public void PlayBasicAttack() => basicAttack?.PlayFeedbacks();
    public void PlaySkillCast() => skillCast?.PlayFeedbacks();

    public void PlayHitTaken()
    {
        hitTaken?.PlayFeedbacks();
        Flash(hitTakenColor);
    }

    void Flash(Color color, float duration = -1f)
    {
        if (targetRenderer == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine(color, duration > 0f ? duration : flashDuration));
    }

    IEnumerator FlashRoutine(Color color, float duration)
    {
        targetRenderer.color = color;
        yield return new WaitForSecondsRealtime(duration);
        targetRenderer.color = baseColor;
        flashRoutine = null;
    }
}

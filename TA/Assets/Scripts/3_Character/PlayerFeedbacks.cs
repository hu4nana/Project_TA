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

    public void PlayParryStart() => parryStart?.PlayFeedbacks();
    public void PlayParrySuccess() => parrySuccess?.PlayFeedbacks();
    public void PlayDodgeStart() => dodgeStart?.PlayFeedbacks();
    public void PlayPerfectDodge() => perfectDodge?.PlayFeedbacks();
    public void PlayBasicAttack() => basicAttack?.PlayFeedbacks();
    public void PlaySkillCast() => skillCast?.PlayFeedbacks();
    public void PlayHitTaken() => hitTaken?.PlayFeedbacks();
}

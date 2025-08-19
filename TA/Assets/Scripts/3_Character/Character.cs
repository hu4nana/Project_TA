using System.Collections;
using UnityEngine;

// 전투 중, 결과 값의 예측을 쉽게하기위해 웬만해선 int로 선언함.
public class Character : MonoBehaviour
{
    public MovementState movementState;
    public ActionState actionState;
    public ConditionState conditionState;


    #region Character Basic Stats

    [Header("Basic Stats")]
    // HP. 0이 될 시 행동불능이 됨
    public int MaxHP = 5;
    public int MaxChance = 5;
    // 스킬 사용에 필요한 자원. FP가 임계점에 도달했을 시 생성됨.
    public int Chance;

    public int ATK;
    public int DEF;

    public float walkSpeed;
    public float runSpeed;
    public float jumpForce;
    public float dashForce;
    public float dashTime;
    #endregion

    #region Variable About Jump

    [Space]
    [Header("About Jump")]
    public int maxJumpChance;
    public int jumpChance;

    #endregion


    public string ID { get; }

    [HideInInspector]
    // FP가 Chance로 전환되는 기준치.
    public int FP_Threshold { get; set; } = 2;


    // Current Health Point
    public int HP {  get; set; }

    [HideInInspector]
    // Current Skill Resource
    public float FP { get; set; }


    [HideInInspector]
    // regain FP per second
    public float Fps { get; set; } =0.1f;

    protected Rigidbody2D rigid;
    protected Animator ani;

    #region About CheckGround

    LayerMask groundMask;
    readonly float downOffset = 0.02f;
    readonly float edgeInset = 0.02f;
    readonly float maxSlopeAngle = 60f;

    
    protected BoxCollider2D col;
    float minGroundNormalY;
    protected RaycastHit2D groundHIt;

    [SerializeField]
    protected bool isGrounded;


    #endregion

    public virtual void Initialize()
    {
        this.HP=this.MaxHP;
        this.FP = 0;

        this.MaxChance = 5;
        this.Chance = 0;

        this.jumpChance = this.maxJumpChance;

        rigid=GetComponent<Rigidbody2D>();
        ani=GetComponent<Animator>();
        col= GetComponent<BoxCollider2D>();

        col=GetComponent<BoxCollider2D>();
        minGroundNormalY=Mathf.Cos(maxSlopeAngle*Mathf.Deg2Rad);
        groundMask = LayerMask.NameToLayer("Platform");
    }

    #region About CharacterState

    public void ChangeState(MovementState newState)
    {
        if(movementState == newState) return;

        movementState = newState;
        ChangeParameters(movementState);
    }

    public void ChangeState(ActionState newState)
    {
        if (actionState == newState) return;

        actionState = newState;
        ChangeParameters(actionState);
    }

    public void ChangeState(ConditionState newState)
    {
        if (conditionState == newState) return;

        conditionState = newState;
        ChangeParameters(conditionState);
    }

    #endregion

    #region About ConditionState
    protected IEnumerator ChangeToInvincible(float index)
    {
        
        yield return new WaitForSeconds(index);
    }
    #endregion

    #region About HP
    // 체력회복
    public virtual void RegainHP(int index)
    {
        HP += index;
        if(HP > MaxHP) HP = MaxHP;
    }

    // 체력감소
    public virtual void LossHP(int index)
    {
        HP -= index;
        if( HP<=0 ) HP = 0;
    }
    #endregion

    #region About FP
    // 초당 집중력 회복
    public virtual void RegainFocusPointPerSecond()
    {
        if (FP < FP_Threshold)
        {
            FP += this.Fps * Time.deltaTime;
        }
    }


    // 보너스 집중력 획득
    public virtual void AcquireBonusFP(float fp)
    {
        this.FP += fp;
        if( this.FP >= FP_Threshold ) this.FP = FP_Threshold;
    }


    // FP를 Chance로 바꿈
    public void Change_FP_To_Chance()
    {
        AddChanceIndex(1);
        this.FP = 0;
    }
    #endregion

    #region About Chance
    // 찬스 획득
    public void AddChanceIndex(int index)
    {
        this.Chance += index;
        if( this.Chance > MaxChance ) this.Chance = MaxChance;
    }


    // 찬스 소모
    public void UseChance(int index)
    {
        if (this.Chance - index >= 0)
        {
            this.Chance -= index;
        }
        else
        {
            Debug.LogWarning($"Lack of Chance");
        }
    }
    #endregion

    #region About Animation

    public void ChangeParameters(MovementState state)
    {
        for(int i=0; i < EnumUtil<MovementState>.Count; i++)
        {
            if (EnumUtil<MovementState>.FromIndex(i) != state)
            {
                ani.SetBool($"{EnumUtil<MovementState>.FromIndex(i)}", false);
            }
            else
            {
                ani.SetBool($"{EnumUtil<MovementState>.FromIndex(i)}", true);
            }
        }
    }

    public void ChangeParameters(ActionState state)
    {
        for (int i = 0; i < EnumUtil<ActionState>.Count; i++)
        {
            if (EnumUtil<ActionState>.FromIndex(i) != state)
            {
                ani.SetBool($"{EnumUtil<ActionState>.FromIndex(i)}", false);
            }
            else
            {
                ani.SetBool($"{EnumUtil<ActionState>.FromIndex(i)}", true);
            }
        }
    }

    public void ChangeParameters(ConditionState state)
    {
        for (int i = 0; i < EnumUtil<ConditionState>.Count; i++)
        {
            if (EnumUtil<ConditionState>.FromIndex(i) != state)
            {
                ani.SetBool($"{EnumUtil<ConditionState>.FromIndex(i)}", false);
            }
            else
            {
                ani.SetBool($"{EnumUtil<ConditionState>.FromIndex(i)}", true);
            }
        }
    }
    #endregion

    // 바닥 체크
    public virtual void GroundCheck()
    {
        isGrounded =Grounded(out groundHIt);
    }

    // 점프횟수 초기화
    public virtual void JumpChanceInit()
    {
        if (isGrounded && jumpChance != maxJumpChance)
        {
            jumpChance = maxJumpChance;
        }
    }
    public bool Grounded(out RaycastHit2D hit)
    {
        var b = col.bounds;                       // 월드 AABB
        float y = b.min.y - downOffset;            // 하단보다 살짝 아래
        Vector2 a = new(b.min.x + edgeInset, y);   // 왼쪽 시작점(안쪽으로 살짝)
        Vector2 c = new(b.max.x - edgeInset, y);   // 오른쪽 끝점(안쪽으로 살짝)

        Debug.DrawLine(a, c, Color.cyan);          // 씬에서 시각화

        hit = Physics2D.Linecast(a, c, groundMask);
        if (hit.collider == null) return false;

        // 경사 필터(너무 가파른 면 제외)
        if (hit.normal.y < minGroundNormalY) return false;

        return true;
    }

    public bool IsGrounded => Grounded(out _);

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!col) col = GetComponent<BoxCollider2D>();
        var b = col.bounds;
        float y = b.min.y - downOffset;
        Vector3 L = new(b.min.x + edgeInset, y, 0f);
        Vector3 R = new(b.max.x - edgeInset, y, 0f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(L, R);
    }
#endif

}

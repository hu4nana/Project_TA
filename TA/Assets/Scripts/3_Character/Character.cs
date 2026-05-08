using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public MovementState movementState;
    public ActionState actionState;
    public ConditionState conditionState;


    #region Character Basic Stats

    [Header("Basic Stats")]
    // HP. 0�� �� �� �ൿ�Ҵ��� ��
    public int MaxHP = 5;
    public int MaxChance = 5;
    // ��ų ��뿡 �ʿ��� �ڿ�. FP�� �Ӱ����� �������� �� ������.
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
    // FP�� Chance�� ��ȯ�Ǵ� ����ġ.
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

    public Rigidbody2D Rigidbody => rigid;

    #region About CheckGround

    LayerMask groundMask;
    readonly float downOffset = 0.02f;
    readonly float edgeInset = 0.02f;
    readonly float maxSlopeAngle = 60f;

    
    protected BoxCollider2D col;
    float minGroundNormalY;
    protected RaycastHit2D groundHIt;
    readonly ContactPoint2D[] groundContacts = new ContactPoint2D[8];

    [SerializeField]
    protected bool isGrounded;
    protected Vector2 groundCheckCenter;
    protected Vector2 groundCheckSize;


    #endregion

    public virtual void Initialize()
    {
        this.HP=this.MaxHP;
        this.FP = 0;

        this.MaxChance = 5;
        this.Chance = 0;

        this.jumpChance = this.maxJumpChance;

        movementState = MovementState.Idle;
        actionState = ActionState.None;
        conditionState = ConditionState.Normal;

        rigid = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();

        minGroundNormalY = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        groundMask = LayerMask.GetMask(
            "Platform",
            "Platforms",
            "OneWayPlatforms",
            "MovingPlatforms",
            "MovingOneWayPlatforms",
            "PlatformsPlayerOnly",
            "MidHeightOneWayPlatforms"
        );
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
    // ü��ȸ��
    public virtual void RegainHP(int index)
    {
        HP += index;
        if(HP > MaxHP) HP = MaxHP;
    }

    // ü�°���
    public virtual void LossHP(int index)
    {
        HP -= index;
        if( HP<=0 ) HP = 0;
    }
    #endregion

    #region About FP
    // �ʴ� ���߷� ȸ��
    public virtual void RegainFocusPointPerSecond()
    {
        if (FP < FP_Threshold)
        {
            FP += this.Fps * Time.deltaTime;
        }
    }


    // ���ʽ� ���߷� ȹ��
    public virtual void AcquireBonusFP(float fp)
    {
        this.FP += fp;
        if( this.FP >= FP_Threshold ) this.FP = FP_Threshold;
    }


    // FP�� Chance�� �ٲ�
    public void Change_FP_To_Chance()
    {
        AddChanceIndex(1);
        this.FP = 0;
    }
    #endregion

    #region About Chance
    // ���� ȹ��
    public void AddChanceIndex(int index)
    {
        this.Chance += index;
        if( this.Chance > MaxChance ) this.Chance = MaxChance;
    }


    // ���� �Ҹ�
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

    // �ٴ� üũ
    public virtual void GroundCheck()
    {
        isGrounded =Grounded(out groundHIt);
    }

    // ����Ƚ�� �ʱ�ȭ
    public virtual void JumpChanceInit()
    {
        if (isGrounded)
            jumpChance = maxJumpChance;
    }
    public bool Grounded(out RaycastHit2D hit)
    {
        var b = col.bounds;
        groundCheckCenter = new Vector2(b.center.x, b.min.y - 0.02f);
        groundCheckSize = new Vector2(Mathf.Max(0.05f, b.size.x - edgeInset * 2f), 0.1f);
        hit = default;

        int contactCount = col.GetContacts(groundContacts);
        for (int i = 0; i < contactCount; i++)
        {
            ContactPoint2D contact = groundContacts[i];
            if (((1 << contact.collider.gameObject.layer) & groundMask) == 0)
                continue;

            if (contact.normal.y >= minGroundNormalY)
                return true;
        }

        Collider2D overlap = Physics2D.OverlapBox(groundCheckCenter, groundCheckSize, 0f, groundMask);
        return overlap != null;
    }

    public bool IsGrounded => isGrounded;
    public Vector2 GroundCheckCenter => groundCheckCenter;
    public Vector2 GroundCheckSize => groundCheckSize;
    public LayerMask GroundMask => groundMask;

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

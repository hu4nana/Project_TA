using System;
using UnityEngine;

// Layer와 Tag 구분
//  Tag는 플레이어인지 npc인지 구분함.
//   Player - 플레이어
//   Enemy - 적
//   AttackRange - 공격 범위
//   Projectile - 투사체
//  Layer는
//   Default, Enviroment, Platform, Interaction, HitBox, Invincible로 구분됨.
// Layer
//  Default - Player, Enemy, NPC의 레이어. Interaction와 HitBox, Platform과 충돌
//  Enviroment - 배경레이어. 그 어떤 것과도 충돌하지 않음.
//  Platform - 플랫폼. Default와 HitBox, Invincible과 충돌
//  Interaction - 상호작용가능한 오브젝트의 레이어. Default와 HitBox, Invincible과 충돌
//  HItBox - 히트박스레이어. Default
//  Invincible - 무적레이어. Platform과 충돌


public enum MovementState
{
    Idle,
    Walk,
    Run,
    Dash,
    Jump
}

public enum ActionState
{
    None,
    Attack,
    Defence,
    Dodge,
    Parry,
    Skill
}

// Attack 자원을 소모하지 않는 일반공격
//  해당 동작 수행 시 조작불가능한 구간이 존재한다.

// Defence 방어 받는 피해가 자신의 DEF에 비례해 감소함

// Dodge 해당 상태일 때 상대의 공격판정과 겹칠 시 무적에 돌입하며, 턴을 잡는다.
//  추가적으로 FP를 획득한다.
//  이때 선택한 공격 애니메이션은 전부 무적이다.

// Parry 해당 상태일 때 상대의 공격판정과 겹칠 시 무적에 돌입하며, 턴을 잡는다.
//  추가적으로 FP를 획득하고, 대상은 Stun에 걸린다.
//  이때 선택한 공격 및 기술 애니메이션은 전부 무적이다.

// Skill 자원을 소모하는 특수한 행동
//  해당 동작 수행 중엔 무적이 존재하며, fps가 0이 된다.
//  조작불가능한 구간이 존재한다.

public enum ConditionState
{
    Normal,
    Invincible,
    Controlled,
    Dead,
    Stun,
    Root,
    Slow,
    Fear,
    Disarm
}

// Invicinble 외부에서 어떤 영향도 끼칠 수 없음
//  레이어가 히트판정이 있는 레이어와 충돌하지않는 Invincible레이어로 변경됨.
//  이후 Default레이어로 복귀

// Controlled 플레이어의 조작이 제한됨

// Dead 자신의 모든 행동을 취소하고 부활을 제외한 어떤 영향을 받지않음.

// Stun 그 어떤 조작 불가 ( Controlled )

// Root 이동 불가 

// Slow 이동속도 감소

// Fear 방어만 가능

// Disarm 공격불가, 스킬사용 불가

public static class EnumUtil<T> where T : struct, Enum
{ 
    public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
    public static readonly int Count = Values.Length;
    public static T FromIndex(int i) => Values[i];
    public static int ToIndex(T e) => Array.IndexOf(Values, e);
}

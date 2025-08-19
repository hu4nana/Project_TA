using System;
using UnityEngine;

// Layer�� Tag ����
//  Tag�� �÷��̾����� npc���� ������.
//   Player - �÷��̾�
//   Enemy - ��
//   AttackRange - ���� ����
//   Projectile - ����ü
//  Layer��
//   Default, Enviroment, Platform, Interaction, HitBox, Invincible�� ���е�.
// Layer
//  Default - Player, Enemy, NPC�� ���̾�. Interaction�� HitBox, Platform�� �浹
//  Enviroment - ��淹�̾�. �� � �Ͱ��� �浹���� ����.
//  Platform - �÷���. Default�� HitBox, Invincible�� �浹
//  Interaction - ��ȣ�ۿ밡���� ������Ʈ�� ���̾�. Default�� HitBox, Invincible�� �浹
//  HItBox - ��Ʈ�ڽ����̾�. Default
//  Invincible - �������̾�. Platform�� �浹


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

// Attack �ڿ��� �Ҹ����� �ʴ� �Ϲݰ���
//  �ش� ���� ���� �� ���ۺҰ����� ������ �����Ѵ�.

// Defence ��� �޴� ���ذ� �ڽ��� DEF�� ����� ������

// Dodge �ش� ������ �� ����� ���������� ��ĥ �� ������ �����ϸ�, ���� ��´�.
//  �߰������� FP�� ȹ���Ѵ�.
//  �̶� ������ ���� �ִϸ��̼��� ���� �����̴�.

// Parry �ش� ������ �� ����� ���������� ��ĥ �� ������ �����ϸ�, ���� ��´�.
//  �߰������� FP�� ȹ���ϰ�, ����� Stun�� �ɸ���.
//  �̶� ������ ���� �� ��� �ִϸ��̼��� ���� �����̴�.

// Skill �ڿ��� �Ҹ��ϴ� Ư���� �ൿ
//  �ش� ���� ���� �߿� ������ �����ϸ�, fps�� 0�� �ȴ�.
//  ���ۺҰ����� ������ �����Ѵ�.

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

// Invicinble �ܺο��� � ���⵵ ��ĥ �� ����
//  ���̾ ��Ʈ������ �ִ� ���̾�� �浹�����ʴ� Invincible���̾�� �����.
//  ���� Default���̾�� ����

// Controlled �÷��̾��� ������ ���ѵ�

// Dead �ڽ��� ��� �ൿ�� ����ϰ� ��Ȱ�� ������ � ������ ��������.

// Stun �� � ���� �Ұ� ( Controlled )

// Root �̵� �Ұ� 

// Slow �̵��ӵ� ����

// Fear �� ����

// Disarm ���ݺҰ�, ��ų��� �Ұ�

public static class EnumUtil<T> where T : struct, Enum
{ 
    public static readonly T[] Values = (T[])Enum.GetValues(typeof(T));
    public static readonly int Count = Values.Length;
    public static T FromIndex(int i) => Values[i];
    public static int ToIndex(T e) => Array.IndexOf(Values, e);
}

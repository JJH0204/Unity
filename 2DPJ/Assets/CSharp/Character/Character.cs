using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] protected int maxHp = 100;
    [SerializeField] protected int currentHp;
    [SerializeField] protected int maxMp = 50;
    [SerializeField] protected int currentMp;
    
    [Header("Level Information")]
    [SerializeField] protected int level = 1;
    [SerializeField] protected int experience = 0;
    [SerializeField] protected int maxExperience = 100;

    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 5f;
    
    [Header("Combat Stats")]
    [SerializeField] protected int attack = 10;
    [SerializeField] protected int defense = 5;

    protected bool isAlive = true;
    protected bool isMoveable = true;

    protected virtual void Start()
    {
        InitializeStats();
    }

    protected virtual void InitializeStats()
    {
        currentHp = maxHp;
        currentMp = maxMp;
    }

    public virtual bool TakeDamage(int damage)
    {
        currentHp = Mathf.Max(0, currentHp - damage);
        isAlive = currentHp > 0;
        return isAlive;
    }

    public virtual void Heal(int amount)
    {
        currentHp = Mathf.Min(maxHp, currentHp + amount);
    }

    public virtual bool UseMp(int amount)
    {
        if (currentMp >= amount)
        {
            currentMp -= amount;
            return true;
        }
        return false;
    }

    // 기본 게터 메서드들
    public int GetCurrentHp() => currentHp;
    public int GetMaxHp() => maxHp;
    public int GetCurrentMp() => currentMp;
    public int GetMaxMp() => maxMp;
    public int GetLevel() => level;
    public bool IsAlive() => isAlive;
    public float GetMoveSpeed() => moveSpeed;
}


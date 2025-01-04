using UnityEngine;

/// <summary>
/// 모든 캐릭터의 기본이 되는 추상 클래스
/// 플레이어와 적 캐릭터가 공유하는 기본 속성과 메서드를 정의
/// </summary>
public abstract class Character : MonoBehaviour
{
    // 캐릭터 기본 속성
    [SerializeField] public string name;             // 캐릭터의 이름
    [SerializeField] public int health;              // 현재 체력
    [SerializeField] public int power;               // 기본 공격력
    [SerializeField] public int depense;             // 기본 방어력
    
    // 캐릭터 이동 관련 속성
    [SerializeField] public float moveSpeed = 5.0f;      // 기본 이동 속도
    [SerializeField] public float rotationSpeed = 10.0f; // 회전 속도 (초당 회전 각도)
    
    // 캐릭터 전투 관련 속성
    [SerializeField] public float attackSpeed = 20f;     // 발사체가 날아가는 속도
    [SerializeField] public float maxAttackDistance = 10f; // 최대 공격 사거리

    /// <summary>
    /// 다른 캐릭터를 공격하는 추상 메서드
    /// 각 캐릭터 타입별로 다르게 구현됨
    /// </summary>
    public abstract void Attack(Character target);

    /// <summary>
    /// 캐릭터 이동을 처리하는 추상 메서드
    /// 각 캐릭터 타입별로 다르게 구현됨
    /// </summary>
    public abstract void Move(Vector3 movement);

    /// <summary>
    /// 데미지를 받았을 때의 처리를 담당하는 가상 메서드
    /// 필요시 자식 클래스에서 재정의 가능
    /// </summary>
    public virtual void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log($"{name}이(가) {damage}의 데미지를 입었습니다. 남은 체력: {health}");

        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 캐릭터 사망 처리를 담당하는 가상 메서드
    /// 필요시 자식 클래스에서 재정의 가능
    /// </summary>
    public virtual void Die()
    {
        Debug.Log($"{name}이(가) 사망했습니다.");
        gameObject.SetActive(false);
    }
}

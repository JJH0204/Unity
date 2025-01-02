using UnityEngine;

/// <summary>
/// 투사체 발사를 관리하는 클래스
/// </summary>
public class Attack : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;      // 발사할 투사체 프리팹
    [SerializeField] private float projectileSpeed = 20f;      // 투사체의 이동 속도
    [SerializeField] private float maxProjectileDistance = 10f;// 투사체가 이동할 수 있는 최대 거리
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    /// <summary>
    /// 매 프레임마다 마우스 입력을 체크
    /// </summary>
    void Update()
    {
        // 마우스 좌클릭 감지
        if (Input.GetMouseButtonDown(0))
        {
            FireProjectile();
        }
    }

    /// <summary>
    /// 투사체를 생성하고 발사하는 메서드
    /// </summary>
    void FireProjectile()
    {
        // 현재 위치와 회전값으로 투사체 생성
        GameObject projectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
        
        // 투사체의 물리 컴포넌트와 컨트롤러 컴포넌트 가져오기
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        ProjectileController controller = projectile.GetComponent<ProjectileController>();
        
        // 투사체에 속도 적용
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * projectileSpeed;
        }
        
        // 투사체 컨트롤러에 최대 거리와 시작 위치 설정
        if (controller != null)
        {
            controller.SetMaxDistance(maxProjectileDistance);
            controller.SetStartPosition(transform.position);
        }
    }
}

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

/// <summary>
/// 플레이어 캐릭터를 제어하는 클래스
/// Character 클래스를 상속받아 기본적인 캐릭터 특성을 가짐
/// </summary>
public class Player : Character
{
    #region 변수 선언
    // 투사체 관련 설정
    [SerializeField] private GameObject projectilePrefab = null;     // 발사할 투사체 프리팹
    [SerializeField] private float projectileOffset = 1.5f;          // 발사 시작 위치 오프셋
    [SerializeField] private LayerMask enemyLayer;                   // 적 감지용 레이어 마스크
    [SerializeField] private bool showDebugRay = true;              // 디버깅 시각화 옵션

    // 내부 참조 및 상태 변수
    private SphereCollider attackRangeCollider = null;  // 공격 범위 감지용 콜라이더
    private GameObject currentTarget = null;             // 현재 선택된 타겟
    private bool isRotatingToTarget = false;            // 타겟을 향한 회전 중 여부
    private float rotationThreshold = 0.1f;             // 회전 완료 판정 임계값
    private Camera mainCamera;                          // 메인 카메라 캐시 참조
    #endregion

    /// <summary>
    /// 초기화 메서드
    /// </summary>
    void Start()
    {
        // 공격 범위 콜라이더 생성 및 설정
        attackRangeCollider = gameObject.AddComponent<SphereCollider>();
        attackRangeCollider.isTrigger = true;
        attackRangeCollider.radius = maxAttackDistance / 2f; // 실제 거리의 절반으로 조정
        attackRangeCollider.center = Vector3.zero; // 중심점 명시적 설정
        
        Debug.Log($"Attack range collider initialized: radius = {attackRangeCollider.radius}");
        mainCamera = Camera.main;
        Debug.Log("Main camera referenced: " + (mainCamera != null));
    }

    /// <summary>
    /// 매 프레임마다 호출되는 업데이트 메서드
    /// 플레이어의 이동과 공격 입력을 처리
    /// </summary>
    void Update()
    {
        // 키보드 입력을 통한 이동 처리
        float moveHorizontal = Input.GetAxis("Horizontal");   // 좌우 이동 입력
        float moveVertical = Input.GetAxis("Vertical");       // 전후 이동 입력
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // 이동 입력이 있을 경우 캐릭터 이동 및 회전 처리
        if (movement != Vector3.zero)
        {
            Move(movement);
        }
        // Debug.Log("Player Update");
        // 적이 감지되면 먼저 회전을 시작
        if (currentTarget != null)
        {
            Debug.Log($"Current target: {currentTarget.name}, Distance: {Vector3.Distance(transform.position, currentTarget.transform.position)}");
            RotateToTarget();
            // 회전이 충분히 이루어졌을 때만 발사
            if (isRotatingToTarget)
            {
                FireProjectileAtTarget();
            }
        }

        // 마우스 클릭으로 타겟 선택
        if (Input.GetMouseButtonDown(0))
        {
            SelectTargetWithRaycast();
        }

        // 타겟이 있을 경우 회전 및 발사 로직
        if (currentTarget != null)
        {
            RotateToTarget();
            if (isRotatingToTarget)
            {
                FireProjectileAtTarget();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Enemy 태그를 가진 게임오브젝트가 감지되면 타겟으로 설정
        if (other.CompareTag("Enemy"))
        {
            // Debug.Log($"Enemy entered range: {other.name}");
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance <= maxAttackDistance)
            {
                currentTarget = other.gameObject;
                Debug.Log($"Target acquired: {currentTarget.name} at distance {distance}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 타겟이 범위를 벗어나면 타겟 해제
        if (other.gameObject == currentTarget)
        {
            // Debug.Log($"Target {currentTarget.name} left range");
            currentTarget = null;
        }
    }

    private void RotateToTarget()
    {
        if (currentTarget == null) return;

        // 타겟 방향 계산
        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // 현재 회전값과 목표 회전값의 차이 계산
        float angleDifference = Quaternion.Angle(transform.rotation, targetRotation);

        // 부드러운 회전 적용
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // 회전이 거의 완료되었는지 확인
        isRotatingToTarget = angleDifference > rotationThreshold;
    }

    private void FireProjectileAtTarget()
    {
        if (currentTarget == null) return;

        Vector3 spawnPosition = transform.position + transform.forward * projectileOffset;
        Vector3 directionToTarget = (currentTarget.transform.position - transform.position).normalized;

        // 투사체 생성 및 발사 (현재 플레이어의 회전값 사용)
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, transform.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        ProjectileController controller = projectile.GetComponent<ProjectileController>();

        if (rb != null)
        {
            rb.linearVelocity = directionToTarget * attackSpeed;
        }

        if (controller != null)
        {
            controller.SetMaxDistance(maxAttackDistance);
            controller.SetStartPosition(transform.position);
        }
    }

    /// <summary>
    /// 레이캐스트를 사용하여 마우스 클릭 위치의 적을 감지하고 타겟으로 설정
    /// </summary>
    private void SelectTargetWithRaycast()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        float maxRayDistance = 100f; // 레이캐스트 최대 검사 거리

        // 디버그 시각화
        if (showDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * maxRayDistance, Color.red, 1f);
            Debug.Log($"레이캐스트 발사 - 시작점: {ray.origin}, 방향: {ray.direction}");
        }

        // 레이캐스트로 적 감지
        if (Physics.Raycast(ray, out hit, maxRayDistance, enemyLayer))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                currentTarget = hit.collider.gameObject;
                
                Debug.Log($"타겟 선택: {currentTarget.name}, 거리: {distance:F2}m" + 
                    (distance <= maxAttackDistance ? " (공격 범위 내)" : " (공격 범위 외)"));
            }
        }
        else
        {
            if (currentTarget != null)
            {
                Debug.Log($"타겟 해제: {currentTarget.name}");
                currentTarget = null;
            }
        }
    }

    /// <summary>
    /// Character 클래스의 Attack 메서드를 오버라이드
    /// 대상 캐릭터에게 데미지를 입히는 메서드
    /// </summary>
    /// <param name="target">공격 대상 캐릭터</param>
    public override void Attack(Character target)
    {
        // 대상 캐릭터에게 현재 플레이어의 공격력만큼 데미지를 입힘
        target.TakeDamage(power);
    }

    /// <summary>
    /// Character 클래스의 Move 메서드를 오버라이드
    /// 플레이어 캐릭터의 이동을 처리하는 메서드
    /// </summary>
    /// <param name="movement">이동 벡터</param>
    public override void Move(Vector3 movement)
    {
        // 이동 벡터를 정규화하여 대각선 이동 시에도 일정한 속도로 이동
        movement.Normalize();
        // 이동 방향으로 캐릭터 회전
        Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        // 실제 이동 처리
        transform.position += movement * moveSpeed * Time.deltaTime;
    }

    /// <summary>
    /// 디버그용 기즈모 그리기
    /// 씬 뷰에서 공격 범위와 타겟 관계를 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (showDebugRay && mainCamera != null)
        {
            // 최대 공격 범위 표시
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxAttackDistance);
            
            // 현재 타겟과의 연결선 표시
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
        }
    }
}

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

    // 네비게이션 관련 변수
    private NavMeshAgent navMeshAgent;         // NavMesh 경로 계산을 위한 에이전트
    private NavMeshPath currentPath;           // 현재 계산된 경로 정보 저장
    private float pathUpdateInterval = 0.5f;   // 경로 재계산 간격 (초)
    private float lastPathUpdateTime;          // 마지막 경로 계산 시간
    private bool isPathValid = false;          // 현재 경로의 유효성 상태

    // 이동 관련 변수 추가
    private bool isMovingToTarget = false;     // 타겟을 향해 이동 중인지 여부
    private float arrivalDistance = 0.5f;      // 도착 판정 거리
    private Vector3 currentDestination;        // 현재 이동 목표 지점

    // NavMesh 관련 추가 변수
    [SerializeField] private float obstacleAvoidanceRadius = 0.5f;  // 장애물 회피 반경
    [SerializeField] private bool showPathLine = true;              // 경로 시각화 여부
    private LineRenderer pathLineRenderer;                          // 경로 시각화용 라인 렌더러
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

        // NavMeshAgent 초기화 수정
        navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        navMeshAgent.enabled = true;           // 경로 계산을 위해 활성화
        navMeshAgent.updatePosition = true;    // 실제 이동 활성화
        navMeshAgent.updateRotation = true;    // 실제 회전 활성화
        navMeshAgent.speed = moveSpeed;
        navMeshAgent.angularSpeed = rotationSpeed;
        navMeshAgent.stoppingDistance = arrivalDistance;
        navMeshAgent.radius = obstacleAvoidanceRadius;
        navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        navMeshAgent.autoTraverseOffMeshLink = true;
        currentPath = new NavMeshPath();
        
        Debug.Log("NavMeshAgent initialized for path calculation");

        // 경로 시각화를 위한 LineRenderer 설정
        SetupPathVisualizer();
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
            StopMovingToTarget();
            Move(movement);
        }
        // 이동 중이 아닐 때만 자동 이동 처리
        else if (isMovingToTarget)
        {
            UpdatePathToTarget();
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
        // 마우스 우클릭으로 타겟을 향해 이동
        if (Input.GetMouseButtonDown(1) && currentTarget != null)
        {
            StartMovingToTarget();
        }

        // 타겟이 있을 경우 주기적으로 경로 업데이트
        if (currentTarget != null)
        {
            CalculatePathToTarget();
            if (isPathValid)
            {
                float currentDistance = GetPathLength();
                Debug.Log($"현재 타겟까지의 거리: {currentDistance:F2}m");
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

        // 타겟과의 거리 체크
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (distanceToTarget > maxAttackDistance)
        {
            Debug.Log($"타겟이 공격 범위를 벗어남 (거리: {distanceToTarget:F2}m, 최대 사거리: {maxAttackDistance}m)");
            return;
        }

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

        Debug.Log($"투사체 발사 - 거리: {distanceToTarget:F2}m");
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
                
                // 최단 경로 계산
                CalculatePathToTarget();
                
                string pathInfo = isPathValid ? $"경로 길이: {GetPathLength():F2}m" : "경로 없음";
                Debug.Log($"타겟 선택: {currentTarget.name}, 직선거리: {distance:F2}m, {pathInfo}");
            }
        }
        else
        {
            if (currentTarget != null)
            {
                Debug.Log($"타겟 해제: {currentTarget.name}");
                currentTarget = null;
                isPathValid = false;
            }
        }
    }

    /// <summary>
    /// 타겟을 향한 이동을 시작하는 메서드
    /// </summary>
    private void StartMovingToTarget()
    {
        if (currentTarget == null) return;

        isMovingToTarget = true;
        navMeshAgent.enabled = true;
        UpdatePathToTarget();
        Debug.Log("타겟을 향한 이동 시작");
    }

    /// <summary>
    /// 타겟을 향한 이동을 중지하는 메서드
    /// </summary>
    private void StopMovingToTarget()
    {
        if (!isMovingToTarget) return;

        isMovingToTarget = false;
        navMeshAgent.ResetPath();
        pathLineRenderer.positionCount = 0; // 경로 시각화 제거
        Debug.Log("타겟을 향한 이동 중지");
    }

    /// <summary>
    /// 타겟을 향한 경로를 업데이트하고 이동을 처리하는 메서드
    /// </summary>
    private void UpdatePathToTarget()
    {
        if (!isMovingToTarget || currentTarget == null) return;

        CalculatePathToTarget();
        
        if (isPathValid)
        {
            // 타겟 위치로 이동 설정
            navMeshAgent.SetDestination(currentTarget.transform.position);
            UpdatePathVisualization();
            
            // 도착 여부 체크
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            if (distanceToTarget <= arrivalDistance)
            {
                Debug.Log("타겟 지점에 도착");
                StopMovingToTarget();
            }
        }
        else
        {
            Debug.LogWarning("유효한 경로를 찾을 수 없음");
            StopMovingToTarget();
        }
    }

    /// <summary>
    /// 현재 선택된 타겟까지의 최단 경로를 계산하는 메서드
    /// </summary>
    private void CalculatePathToTarget()
    {
        if (currentTarget == null)
        {
            isPathValid = false;
            return;
        }

        // 현재 시간 체크
        if (Time.time - lastPathUpdateTime < pathUpdateInterval)
            return;

        // NavMesh 경로 계산
        try
        {
            NavMeshPath newPath = new NavMeshPath();
            isPathValid = NavMesh.CalculatePath(
                transform.position,
                currentTarget.transform.position,
                NavMesh.AllAreas,
                newPath
            );

            if (isPathValid && newPath.status == NavMeshPathStatus.PathComplete)
            {
                currentPath = newPath;
                lastPathUpdateTime = Time.time;
                float pathLength = GetPathLength();
                Debug.Log($"경로 계산 성공 - 길이: {pathLength:F2}m, 경유지 수: {currentPath.corners.Length}");
            }
            else
            {
                Debug.LogWarning($"경로 계산 실패 - Status: {newPath.status}");
                isPathValid = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"경로 계산 중 오류 발생: {e.Message}");
            isPathValid = false;
        }
    }

    /// <summary>
    /// 현재 계산된 경로의 총 길이를 반환하는 메서드
    /// </summary>
    /// <returns>경로의 총 길이 (미터 단위)</returns>
    private float GetPathLength()
    {
        if (!isPathValid || currentPath == null || currentPath.corners.Length < 2)
        {
            return float.MaxValue; // 유효하지 않은 경로는 무한대 거리로 처리
        }

        float pathLength = 0f;
        Vector3[] corners = currentPath.corners;

        // 각 경유지 사이의 거리 합산
        for (int i = 1; i < corners.Length; i++)
        {
            pathLength += Vector3.Distance(corners[i - 1], corners[i]);
            
            // 디버그용 경유지 정보 출력
            if (showDebugRay)
            {
                Debug.DrawLine(corners[i - 1], corners[i], Color.blue, pathUpdateInterval);
                Debug.Log($"경유지 {i}: {corners[i]}, 누적 거리: {pathLength:F2}m");
            }
        }

        return pathLength;
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
        StopMovingToTarget(); // 수동 이동 시 자동 이동 중지
        navMeshAgent.enabled = false;
        
        // NavMesh 상의 유효한 위치 확인
        NavMeshHit hit;
        Vector3 targetPosition = transform.position + movement * moveSpeed * Time.deltaTime;
        
        if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            // 유효한 위치로만 이동
            transform.position = hit.position;
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 경로 시각화를 위한 LineRenderer 설정
    /// </summary>
    private void SetupPathVisualizer()
    {
        pathLineRenderer = gameObject.AddComponent<LineRenderer>();
        pathLineRenderer.startWidth = 0.15f;
        pathLineRenderer.endWidth = 0.15f;
        pathLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        pathLineRenderer.startColor = Color.green;
        pathLineRenderer.endColor = Color.green;
        pathLineRenderer.positionCount = 0;
    }

    /// <summary>
    /// 현재 경로를 시각화
    /// </summary>
    private void UpdatePathVisualization()
    {
        if (!showPathLine || !isPathValid || currentPath == null)
        {
            pathLineRenderer.positionCount = 0;
            return;
        }

        Vector3[] corners = currentPath.corners;
        pathLineRenderer.positionCount = corners.Length;
        
        // 경로의 모든 지점을 라인 렌더러에 설정
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i].y += 0.1f; // 지면보다 약간 위에 표시
            pathLineRenderer.SetPosition(i, corners[i]);
        }

        // 디버그 정보 출력
        if (showDebugRay)
        {
            for (int i = 1; i < corners.Length; i++)
            {
                Debug.DrawLine(corners[i - 1], corners[i], Color.blue, pathUpdateInterval);
            }
        }
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
                
                // NavMesh 경로 시각화 (초록색)
                if (isPathValid && currentPath != null && currentPath.corners.Length > 0)
                {
                    Gizmos.color = Color.green;
                    // 각 경로 포인트를 선으로 연결하여 표시
                    for (int i = 1; i < currentPath.corners.Length; i++)
                    {
                        Gizmos.DrawLine(currentPath.corners[i - 1], currentPath.corners[i]);
                    }
                }
            }
        }
    }
}

using UnityEngine;

/// <summary>
/// 3인칭 시점 카메라 컨트롤러
/// 플레이어를 추적하고 마우스 입력으로 회전과 줌을 제어
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;  // 카메라가 추적할 대상 (일반적으로 플레이어)

    [Header("Camera Settings")]
    public float cameraDistance = 10f;    // 카메라와 타겟 사이의 기본 거리
    public float cameraHeight = 5f;       // 카메라의 기본 높이
    public float cameraAngle = 45f;       // 카메라의 기본 시야각
    public float zoomSpeed = 2f;          // 줌 속도 계수
    public float minCameraDistance = 5f;   // 최소 줌 거리
    public float maxCameraDistance = 20f;  // 최대 줌 거리

    [Header("Camera Rotation")]
    public float rotationSpeed = 3f;       // 카메라 회전 속도
    private float currentRotationAngle = 0f;    // 현재 회전 각도
    private bool isDragging = false;            // 마우스 드래그 중인지 여부

    /// <summary>
    /// 초기화 시 카메라 위치와 각도 설정
    /// </summary>
    void Start()
    {
        // 타겟 유효성 검사
        if (target == null)
        {
            Debug.LogError("카메라 타겟이 설정되지 않았습니다!");
            return;
        }

        // 초기 카메라 위치 및 회전 설정
        Vector3 cameraPosition = target.position;
        cameraPosition.y += cameraHeight;      // 높이 설정
        cameraPosition.z -= cameraDistance;    // 거리 설정
        transform.position = cameraPosition;
        transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
    }

    /// <summary>
    /// 매 프레임 카메라 위치와 회전 업데이트
    /// </summary>
    void Update()
    {
        if (target == null) return;

        // 마우스 우클릭 드래그로 카메라 회전 제어
        if (Input.GetMouseButtonDown(1)) isDragging = true;
        if (Input.GetMouseButtonUp(1)) isDragging = false;

        // 드래그 중일 때 카메라 회전 처리
        if (isDragging)
        {
            float mouseX = Input.GetAxis("Mouse X");
            currentRotationAngle += mouseX * rotationSpeed;
        }

        // 마우스 휠로 줌 인/아웃 처리
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0.0f)
        {
            // 줌 거리 조정 및 제한
            cameraDistance -= scrollInput * zoomSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance, minCameraDistance, maxCameraDistance);
        }

        // 카메라 위치 계산 및 업데이트
        float angleInRadians = currentRotationAngle * Mathf.Deg2Rad;
        Vector3 cameraOffset = new Vector3(
            Mathf.Sin(angleInRadians) * cameraDistance,  // X축 위치
            cameraHeight,                                // Y축 위치
            Mathf.Cos(angleInRadians) * cameraDistance  // Z축 위치
        );

        // 카메라 위치와 시선 방향 설정
        transform.position = target.position + cameraOffset;
        transform.LookAt(target.position);  // 항상 타겟을 바라보도록 설정
    }
}

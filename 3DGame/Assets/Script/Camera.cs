using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;  // 추적할 플레이어 Transform

    [Header("Camera Settings")]
    public float cameraDistance = 10f;
    public float cameraHeight = 5f;
    public float cameraAngle = 45f;
    public float zoomSpeed = 2f;
    public float minCameraDistance = 5f;
    public float maxCameraDistance = 20f;

    [Header("Camera Rotation")]
    public float rotationSpeed = 3f;
    private float currentRotationAngle = 0f;
    private bool isDragging = false;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("카메라 타겟이 설정되지 않았습니다!");
            return;
        }

        // 초기 카메라 설정
        Vector3 cameraPosition = target.position;
        cameraPosition.y += cameraHeight;
        cameraPosition.z -= cameraDistance;
        transform.position = cameraPosition;
        transform.rotation = Quaternion.Euler(cameraAngle, 0, 0);
    }

    void Update()
    {
        if (target == null) return;

        // 마우스 우클릭 감지
        if (Input.GetMouseButtonDown(1)) isDragging = true;
        if (Input.GetMouseButtonUp(1)) isDragging = false;

        // 카메라 회전
        if (isDragging)
        {
            float mouseX = Input.GetAxis("Mouse X");
            currentRotationAngle += mouseX * rotationSpeed;
        }

        // 줌 처리
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0.0f)
        {
            cameraDistance -= scrollInput * zoomSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance, minCameraDistance, maxCameraDistance);
        }

        // 카메라 위치 업데이트
        float angleInRadians = currentRotationAngle * Mathf.Deg2Rad;
        Vector3 cameraOffset = new Vector3(
            Mathf.Sin(angleInRadians) * cameraDistance,
            cameraHeight,
            Mathf.Cos(angleInRadians) * cameraDistance
        );

        transform.position = target.position + cameraOffset;
        transform.LookAt(target.position);
    }
}

using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 5.0f;
    public Transform cameraTransform;
    private Vector3 offset;

    [Header("Camera Settings")]
    public float cameraDistance = 10f;
    public float cameraHeight = 5f;
    public float cameraAngle = 45f;
    public float zoomSpeed = 2f; // 줌 속도
    public float minCameraDistance = 5f; // 최소 카메라 거리
    public float maxCameraDistance = 20f; // 최대 카메라 거리

    [Header("Camera Rotation")]
    public float rotationSpeed = 3f;
    private float currentRotationAngle = 0f;
    private bool isDragging = false;

    void Start()
    {
        // 카메라 초기 위치 설정
        Vector3 cameraPosition = transform.position;
        cameraPosition.y += cameraHeight;
        cameraPosition.z -= cameraDistance;
        cameraTransform.position = cameraPosition;

        // 카메라 회전 설정
        cameraTransform.rotation = Quaternion.Euler(cameraAngle, 0, 0);

        offset = cameraTransform.position - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // 마우스 드래그 감지
        if (Input.GetMouseButtonDown(1)) // 우클릭 시작
        {
            isDragging = true;
        }
        if (Input.GetMouseButtonUp(1)) // 우클릭 종료
        {
            isDragging = false;
        }

        // 드래그 중일 때 카메라 회전
        if (isDragging)
        {
            float mouseX = Input.GetAxis("Mouse X");
            currentRotationAngle += mouseX * rotationSpeed;
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        
        // 줌 처리
        if (scrollInput != 0.0f)
        {
            cameraDistance -= scrollInput * zoomSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance, minCameraDistance, maxCameraDistance);
        }

        // 캐릭터 이동
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        transform.Translate(movement * speed * Time.deltaTime, Space.World);

        // 카메라 위치와 회전 업데이트
        float angleInRadians = currentRotationAngle * Mathf.Deg2Rad;
        Vector3 cameraOffset = new Vector3(
            Mathf.Sin(angleInRadians) * cameraDistance,
            cameraHeight,
            Mathf.Cos(angleInRadians) * cameraDistance
        );
        
        cameraTransform.position = transform.position + cameraOffset;
        cameraTransform.LookAt(transform.position);
    }
}

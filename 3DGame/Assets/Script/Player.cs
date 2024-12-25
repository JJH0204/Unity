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
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        transform.Translate(movement * speed * Time.deltaTime, Space.World);

        cameraTransform.position = transform.position + offset;
    }
}

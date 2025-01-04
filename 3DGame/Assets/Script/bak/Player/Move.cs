using UnityEngine;

public class Move : MonoBehaviour
{
    // 캐릭터의 이동 속도를 조절하는 변수
    public float speed = 5.0f;
    // 캐릭터의 회전 속도를 조절하는 변수
    public float rotationSpeed = 10.0f;

    void Update()
    {
        // 수평(좌/우) 입력값 감지 (-1 ~ 1)
        float moveHorizontal = Input.GetAxis("Horizontal");
        // 수직(앞/뒤) 입력값 감지 (-1 ~ 1)
        float moveVertical = Input.GetAxis("Vertical");

        // 입력값을 기반으로 3D 이동 벡터 생성 (Y축은 사용하지 않음)
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        
        // 이동 입력이 있을 때만 회전과 이동 처리
        if (movement != Vector3.zero)
        {
            // 이동 벡터를 정규화하여 대각선 이동시에도 일정한 속도 유지
            movement.Normalize();
            
            // 현재 이동 방향을 바라보는 회전값 계산
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            // 현재 회전값에서 목표 회전값으로 부드럽게 보간
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            
            // 정규화된 이동 벡터에 속도와 시간을 곱하여 실제 이동량 계산 후 적용
            transform.position += movement * speed * Time.deltaTime;
        }
    }
}

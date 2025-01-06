using UnityEngine;

/// <summary>
/// 투사체의 동작을 제어하는 클래스
/// </summary>
public class ProjectileController : MonoBehaviour
{
    private float maxDistance;    // 투사체가 이동할 수 있는 최대 거리
    private Vector3 startPosition;// 투사체의 발사 시작 위치

    /// <summary>
    /// 투사체의 최대 이동 거리를 설정하는 메서드
    /// </summary>
    /// <param name="distance">설정할 최대 거리</param>
    public void SetMaxDistance(float distance)
    {
        maxDistance = distance;
    }
    
    /// <summary>
    /// 투사체의 시작 위치를 설정하는 메서드
    /// </summary>
    /// <param name="position">발사 시작 위치</param>
    public void SetStartPosition(Vector3 position)
    {
        startPosition = position;
    }
    
    /// <summary>
    /// 매 프레임마다 이동 거리를 체크하여 최대 거리 도달 시 제거
    /// </summary>
    void Update()
    {
        // 시작 위치로부터의 현재 이동 거리 계산
        float traveledDistance = Vector3.Distance(startPosition, transform.position);

        // 최대 거리 도달 시 오브젝트 제거
        if (traveledDistance >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
}

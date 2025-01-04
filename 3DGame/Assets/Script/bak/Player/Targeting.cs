using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public interface ITargetable
{
    void OnTargeted();
    void OnUntargeted();
}
    
public class Targeting : MonoBehaviour
{
    public GameObject target;
    public UnityEvent<GameObject> onTargetChanged;
    public float maxTargetDistance = 100f;
    
    private NavMeshAgent agent;
    private NavMeshPath path;
    public float pathUpdateRate = 0.2f; // 경로 갱신 주기
    private float nextPathUpdate;

    void Start()
    {
        onTargetChanged = new UnityEvent<GameObject>();
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();
        
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing!");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySetNewTarget();
        }
        UpdatePathToTarget();
    }

    private void UpdatePathToTarget()
    {
        if (target == null || agent == null) return;

        // 일정 시간마다 경로 갱신
        if (Time.time > nextPathUpdate)
        {
            nextPathUpdate = Time.time + pathUpdateRate;
            
            // NavMesh를 사용하여 경로 계산
            if (NavMesh.CalculatePath(transform.position, target.transform.position, NavMesh.AllAreas, path))
            {
                // 유효한 경로가 있다면 에이전트에 설정
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetPath(path);
                }
                else
                {
                    Debug.LogWarning("Path to target is not complete!");
                }
            }
        }
    }

    private void TrySetNewTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxTargetDistance))
        {
            GameObject clickedObject = hit.collider.gameObject;

            if (clickedObject.CompareTag("Enemy"))
            {
                // 이전 타겟의 인터페이스 호출
                if (target != null)
                {
                    var targetable = target.GetComponent<ITargetable>();
                    targetable?.OnUntargeted();
                }

                // 새로운 타겟 설정
                target = clickedObject;
                
                // 새 타겟의 인터페이스 호출
                var newTargetable = target.GetComponent<ITargetable>();
                newTargetable?.OnTargeted();

                // 이벤트 발생
                onTargetChanged?.Invoke(target);
                
                // 새로운 타겟이 설정되면 즉시 경로 갱신
                nextPathUpdate = 0;
            }
        }
    }

    public GameObject GetCurrentTarget()
    {
        return target;
    }

    public void ClearTarget()
    {
        var targetable = target?.GetComponent<ITargetable>();
        targetable?.OnUntargeted();
        target = null;
        onTargetChanged?.Invoke(null);
        agent.ResetPath(); // 경로 초기화
    }

    // 현재 경로의 모든 지점을 반환
    public Vector3[] GetCurrentPath()
    {
        if (path == null) return null;
        return path.corners;
    }

    // 디버그용 경로 시각화
    private void OnDrawGizmos()
    {
        if (path != null && path.corners.Length > 1)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(path.corners[i], path.corners[i + 1]);
            }
        }
    }
}

using UnityEngine;

public enum EnemyPersonality
{
    Aggressive,  // 호전적
    Peaceful     // 온순
}

public class Enemy : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private EnemyPersonality personality;
    
    private Transform playerTransform;
    private bool isPlayerInRange = false;
    private bool wasAttacked = false;
    private Transform attackerTransform;

    void Start()
    {
        // 감지 범위를 위한 SphereCollider 추가
        SphereCollider detector = gameObject.AddComponent<SphereCollider>();
        detector.radius = detectionRadius;
        detector.isTrigger = true;
    }

    void Update()
    {
        if (!isPlayerInRange || playerTransform == null) return;

        bool shouldChase = false;
        
        switch (personality)
        {
            case EnemyPersonality.Aggressive:
                shouldChase = true;
                break;
            case EnemyPersonality.Peaceful:
                shouldChase = wasAttacked && attackerTransform == playerTransform;
                break;
        }

        if (shouldChase)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform;
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerTransform = null;
        }
    }

    public void OnDamaged(Transform attacker)
    {
        wasAttacked = true;
        attackerTransform = attacker;
    }
}

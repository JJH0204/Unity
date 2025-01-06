using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;

    private GameObject player;
    private List<GameObject> enemies = new List<GameObject>();

    public void Initialize()
    {
        if (playerPrefab == null || enemyPrefab == null)
        {
            Debug.LogError("Prefabs are not assigned in ObjectManager!");
            return;
        }

        CreatePlayer();
        CreateInitialEnemies();
    }

    private void CreatePlayer()
    {
        Vector3 playerSpawnPosition = Vector3.zero; // 시작 위치 설정
        player = Instantiate(playerPrefab, playerSpawnPosition, Quaternion.identity);
        Character playerCharacter = player.GetComponent<Character>();
        
        if (playerCharacter != null)
        {
            Debug.Log("Player created successfully with HP: " + playerCharacter.GetCurrentHp());
        }
    }

    private void CreateInitialEnemies()
    {
        // 예시로 3개의 적 생성
        Vector3[] enemyPositions = {
            new Vector3(3, 0, 0),
            new Vector3(-3, 0, 0),
            new Vector3(0, 3, 0)
        };

        foreach (Vector3 position in enemyPositions)
        {
            GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            enemies.Add(enemy);
            Debug.Log($"Enemy created at position: {position}");
        }
    }

    public void UpdateManager()
    {
        // 오브젝트 업데이트 로직
        CleanupDestroyedEnemies();
    }

    private void CleanupDestroyedEnemies()
    {
        enemies.RemoveAll(enemy => enemy == null);
    }

    // 게터 메서드들
    public GameObject GetPlayer() => player;
    public List<GameObject> GetEnemies() => enemies;

    // 새로운 적 생성 메서드
    public GameObject SpawnEnemy(Vector3 position)
    {
        GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        enemies.Add(enemy);
        return enemy;
    }
}

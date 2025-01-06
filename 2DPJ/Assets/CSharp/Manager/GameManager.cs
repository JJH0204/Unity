using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 하위 관리자 참조
    private ScriptManager scriptManager;
    private ObjectManager objectManager;
    private TurnManager turnManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManagers()
    {
        scriptManager = GetComponent<ScriptManager>() ?? gameObject.AddComponent<ScriptManager>();
        objectManager = GetComponent<ObjectManager>() ?? gameObject.AddComponent<ObjectManager>();
        turnManager = GetComponent<TurnManager>() ?? gameObject.AddComponent<TurnManager>();
    }

    void Start()
    {
        // 각 매니저 초기화
        scriptManager.Initialize();
        objectManager.Initialize();
        turnManager.Initialize();
    }

    void Update()
    {
        // 각 매니저 업데이트
        scriptManager.UpdateManager();
        objectManager.UpdateManager();
        turnManager.UpdateManager();
    }

    // 게임 상태 제어 메서드
    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
    }
}

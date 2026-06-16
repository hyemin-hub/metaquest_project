using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ⚠️ 임시 GameManager — C의 GameManager가 완성되면 교체.
/// 회의 자료의 게임 흐름 구현: 제한 시간 + 파괴율 + 성공/실패 판정.
/// </summary>
public class SimpleGameManager : MonoBehaviour
{
    [Header("게임 설정")]
    [Tooltip("제한 시간 (초)")]
    public float timeLimit = 90f;

    [Tooltip("성공 판정에 필요한 파괴율 (0~1)")]
    public float winRatio = 0.7f;

    [Header("진행 상태 (런타임)")]
    public int totalBlocks;
    public int destroyedBlocks;
    public float timeRemaining;
    public bool gameEnded;

    public static SimpleGameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timeRemaining = timeLimit;
        totalBlocks = FindObjectsByType<SimpleHealthBlock>(FindObjectsSortMode.None).Length;
        Debug.Log($"[GameManager] 게임 시작! 총 블록: {totalBlocks}개, 제한 시간: {timeLimit}초");
    }

    void Update()
    {
        if (gameEnded) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            EndGame(false);
        }
    }

    public void OnBlockDestroyed()
    {
        destroyedBlocks++;
        float ratio = totalBlocks > 0 ? (float)destroyedBlocks / totalBlocks : 0f;
        Debug.Log($"[GameManager] 파괴: {destroyedBlocks}/{totalBlocks} ({ratio:P0})");

        if (ratio >= winRatio)
        {
            EndGame(true);
        }
    }

    void EndGame(bool win)
    {
        gameEnded = true;
        if (win)
        {
            Debug.Log("[GameManager] 🎉 클리어!");
            // LevelManager 있으면 다음 레벨로
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelComplete();
            }
        }
        else
        {
            Debug.Log("[GameManager] ❌ 실패");
        }
    }

    public float GetDestroyRatio()
    {
        return totalBlocks > 0 ? (float)destroyedBlocks / totalBlocks : 0f;
    }
}

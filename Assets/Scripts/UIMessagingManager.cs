using System;
using UnityEngine;
using UnityEngine.Events;

public class UIMessagingManager : MonoBehaviour
{
    public static UIMessagingManager instance;
    
    [Header("UI Action Events")]
    public UnityEvent OnGameStarts = new UnityEvent();
    public UnityEvent OnGameQuit = new UnityEvent();
    public UnityEvent OnGamePause = new UnityEvent();
    public UnityEvent OnGameResume = new UnityEvent();
    public UnityEvent OnAnswerCorrect = new UnityEvent();
    public UnityEvent OnAnswerWrong = new UnityEvent();
    public UnityEvent OnRoundCompleted = new UnityEvent();
    public UnityEvent OnDifficultyChanged = new UnityEvent();
    public UnityEvent OnScoreUpdated = new UnityEvent();
    public UnityEvent OnInstructionsShown = new UnityEvent();
    public UnityEvent OnReturnMainMenu = new UnityEvent();
    public UnityEvent OnSelectedDifficulty = new UnityEvent();
    public UnityEvent OnloadingStarted = new UnityEvent();
    public UnityEvent OnloadingCompleted = new UnityEvent();
    public UnityEvent OnFetchFailed = new UnityEvent();
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void GameStart() => OnGameStarts?.Invoke();
    public void GameQuit() => OnGameQuit?.Invoke();
    public void GamePause() => OnGamePause?.Invoke();
    public void GameResume() => OnGameResume?.Invoke();
    public void AnswerCorrect() => OnAnswerCorrect?.Invoke();
    public void AnswerWrong() => OnAnswerWrong?.Invoke();
    public void DifficultyChanged() => OnDifficultyChanged?.Invoke();
    public void ScoreUpdated() => OnScoreUpdated?.Invoke();
    public void ReturnMainMenu() => OnReturnMainMenu?.Invoke();
    public void SelectedDifficulty() => OnSelectedDifficulty?.Invoke();
    public void SelectedInstructionsShown() => OnInstructionsShown?.Invoke();
    public void LoadingStarted() => OnloadingStarted?.Invoke();
    public void LoadingCompleted() => OnloadingCompleted?.Invoke();
    public void RoundCompleted() => OnRoundCompleted?.Invoke();
    public void FetchFailed() => OnFetchFailed?.Invoke();
    
}
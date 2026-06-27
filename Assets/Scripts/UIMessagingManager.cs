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
    public UnityEvent OnQuestionAsked = new UnityEvent();
    public UnityEvent OnAnswerCorrect = new UnityEvent();
    public UnityEvent OnAnswerWrong = new UnityEvent();
    public UnityEvent OnCategoryChanged = new UnityEvent();
    public UnityEvent OnScoreUpdated = new UnityEvent();
    public UnityEvent OnInstructionsShown = new UnityEvent();
    public UnityEvent OnReturnMainMenu = new UnityEvent();
    public UnityEvent onSelectedCategory = new UnityEvent();

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
    public void QuestionAsked() => OnQuestionAsked?.Invoke();
    public void AnswerCorrect() => OnAnswerCorrect?.Invoke();
    public void AnswerWrong() => OnAnswerWrong?.Invoke();
    public void CategoryChanged() => OnCategoryChanged?.Invoke();
    public void ScoreUpdated() => OnScoreUpdated?.Invoke();
    public void ReturnMainMenu() => OnReturnMainMenu?.Invoke();
    public void SelectedCategory() => onSelectedCategory?.Invoke();
    public void SelectedInstructionsShown() => OnInstructionsShown?.Invoke();
    
}
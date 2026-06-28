using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using TMPro;

public class GamePlayManager : MonoBehaviour
{
    // Editable in Inspector

    [SerializeField][Tooltip("Number of questions per level")]
    public int questionsPerLevel = 50;

    private string selectedDifficulty = "easy";
    
    // Timer
    [SerializeField]
    [Tooltip("Drag a TextMeshPro Text object here to show the countdown")]
    public TMP_Text timerText; 
    
    [SerializeField]
    [Tooltip("Time allowed per question in seconds")]
    public float questionTimeLimit = 15f;
    
    private float timeRemaining;
    public float GetTimeRemaining() => timeRemaining;
    public bool isTimerRunning = false;
    
    private int retryCount = 0;
    private const int maxRetries = 3;
    private Coroutine timeoutCoroutine;

    
    // Game state 
    public List<Question> questions = new List<Question>();
    private int currentIndex = 0;
    public int score = 0;
    public bool isGameOver = false;

    // Current difficulty based on round
    public string CurrentDifficulty => selectedDifficulty;
    
    public int CurrentRoundQuestionCount => questionsPerLevel;
    public bool isPlayerWon = false;
   
    [SerializeField] private TriviaDataManager triviaDataManager;

    [SerializeField] private GameObject hud;
    
    [SerializeField] private GameObject failedFetchPopUp;
    
    [SerializeField] private GameObject loadingScreen;
    
    [SerializeField] private TMP_Text questionText;
    
    [SerializeField] private UnityEngine.UI.Button[] answerButtons;
    
    public System.Action onAnswerCorrect;
    public System.Action onAnswerWrong;
    public System.Action<int> onScoreUpdated;
    
    // TriviaManager call this with the questions for the current round
    public void LoadQuestions(List<Question> fetchedQuestions)
    {
        questions = fetchedQuestions;
        timeRemaining = questionTimeLimit;
        isTimerRunning = true;
        currentIndex = 0;
        isGameOver = false;
    }

    public Question GetCurrentQuestion()
    {
        if(isGameOver || questions.Count == 0) return null;
        return questions[currentIndex];
    }

    public bool SubmitAnswer(string answer)
    {
        if (isGameOver) return false;

        Question question = GetCurrentQuestion();
        bool correct = (answer == question.AnswerOptions[question.CorrectAnswerID]);

        if (!correct)
        {
            isGameOver = true;
            isTimerRunning = false;
            isPlayerWon = false;
            return false;
        }

        score++;
        onAnswerCorrect?.Invoke();
        onScoreUpdated?.Invoke(score);
        currentIndex++;

        if (currentIndex >= questions.Count)
        {
            isGameOver = true;
            onAnswerWrong?.Invoke();
            isTimerRunning = false;
            isPlayerWon = true;
            return true;
        }

        DisplayQuestion(); 
        timeRemaining = questionTimeLimit;
        isTimerRunning = true;
        return true;
    }


    public string[] GetShuffledAnswers()
    {
        Question question = GetCurrentQuestion();
        if (question == null) return null;
        return question.AnswerOptions;
    }
    
    // Difficulty
    public void SetDifficulty(string difficulty)
    {
        selectedDifficulty = difficulty;
        PlayerPrefs.SetString("Difficulty", difficulty);
    }

    
    void Start()
    {
        selectedDifficulty = PlayerPrefs.GetString("Difficulty",  "easy");
        score = 0;
        RequestQuestions();
    }

    private void RequestQuestions()
    {
        if (loadingScreen != null) loadingScreen.SetActive(true);
        if (failedFetchPopUp != null) failedFetchPopUp.SetActive(false);
        StartCoroutine(FetchAndLoad());
    }

    private IEnumerator FetchAndLoad()
    {
        timeoutCoroutine = StartCoroutine(FetchTimeout());
        yield return StartCoroutine(triviaDataManager.GetTriviaQuestions(questionsPerLevel, selectedDifficulty));

        if (triviaDataManager.Questions != null && triviaDataManager.Questions.Count > 0)
            OnFetchSuccess(triviaDataManager.Questions);
        else
            OnFetchFailed();
    }


    private IEnumerator FetchTimeout()
    {
        yield return new WaitForSeconds(15f);
        OnFetchFailed();  // auto-called if no response in 15 sec
    }
    
    // TriviaManager calls this when fetch succeeds
    public void OnFetchSuccess(List<Question> fetchedQuestions)
    {
        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        retryCount = 0;

        if (loadingScreen != null) loadingScreen.SetActive(false);
        if (hud != null) hud.SetActive(true);
        LoadQuestions(fetchedQuestions);
    }

// TriviaManager calls this when fetch fails
    public void OnFetchFailed()
    {
        if (timeoutCoroutine != null) StopCoroutine(timeoutCoroutine);
        retryCount++;

        if (retryCount < maxRetries)
        {
            Debug.Log($"Fetch failed, retrying... ({retryCount}/{maxRetries})");
            RequestQuestions();  // auto retry
        }
        else
        {
            retryCount = 0;
            if (loadingScreen != null) loadingScreen.SetActive(false);
            if (failedFetchPopUp != null) failedFetchPopUp.SetActive(true);
            Debug.Log("Fetch failed after 3 attempts.");
        }
    }

    public void DisplayQuestion()
    {
        Question question = GetCurrentQuestion();
        if (question == null) return;

        questionText.text = question.QuestionText;
        string[] answers = GetShuffledAnswers();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            string answer = answers[i];
            answerButtons[i].GetComponentInChildren<TMP_Text>().text = answer;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => SubmitAnswer(answer));
        }
    }


    void Update()
    {
        if (!isTimerRunning || isGameOver)
            return;
        
        timeRemaining -= Time.deltaTime;

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
        
        if (timeRemaining <= 0.0f)
        {
            timeRemaining = 0.0f;
            isTimerRunning = false;
            SubmitAnswer(""); // empty = wrong answer
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f; isTimerRunning = false;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f; isTimerRunning = true;
    }

}

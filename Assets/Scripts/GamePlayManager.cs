using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
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
    
    [SerializeField][Tooltip("Text to show while loading questions")]
    public TMP_Text loadingText;

    [SerializeField][Tooltip("Text to show when fetch fails")]
    public TMP_Text errorText;

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
            Debug.Log($"Wrong! Game Over. Score: {score}/{questionsPerLevel}");
            return false;
        }

        score++;
        currentIndex++;

        if (currentIndex >= questions.Count)
        {
            isGameOver = true;
            isTimerRunning = false;
            isPlayerWon = true;
            Debug.Log($"You beat the level! Score: {score}/{questionsPerLevel}");
            return true;
        }

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
        if (loadingText != null) loadingText.gameObject.SetActive(true);
        if (errorText != null) errorText.gameObject.SetActive(false);
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

        if (loadingText != null) loadingText.gameObject.SetActive(false);
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
            if (loadingText != null) loadingText.gameObject.SetActive(false);
            if (errorText != null)
            {
                errorText.gameObject.SetActive(true);
                errorText.text = "Please restart the game and check your internet.";
            }
            Debug.Log("Fetch failed after 3 attempts.");
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
}

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
    
    [SerializeField][Tooltip("Correct answers needed to trigger Round Finished screen")]
    public int questionsPerRound = 5;
    
    public bool isPlayerWon = false;
    
    private bool isFetching = false;
   
    [SerializeField] 
    private TriviaDataManager triviaDataManager;

    [SerializeField] 
    private GameObject hud;
    
    [SerializeField] 
    private GameObject failedFetchPopUp;
    
    [SerializeField] 
    private GameObject loadingScreen;
    
    [SerializeField]
    private TMP_Text questionText;
    
    [SerializeField]
    private UnityEngine.UI.Button[] answerButtons;
    
    public System.Action onAnswerCorrect;
    public System.Action onAnswerWrong;
    public System.Action<int> onScoreUpdated;
    
    [SerializeField] 
    public int easyScore = 1;
    
    [SerializeField]
    public int mediumScore = 2;
    
    [SerializeField]
    public int hardScore = 3;
    
    [SerializeField] 
    private GameObject gameOverScreen;
    
    [SerializeField] 
    private GameObject correctPopUp;
    
    [SerializeField] 
    private GameObject roundFinishedScreen;
    
    [SerializeField] 
    private TMP_Text correctPopUpScoreText;
    
    
    [SerializeField]
    private TMP_Text gameOverScoreText;  
    
    [SerializeField]
    private TMP_Text roundFinishedScoreText;
    
    [SerializeField]
    private TMP_Text bestScoreText;
    
    [SerializeField] 
    public float correctPopUpDuration = 5f;
    
    [SerializeField] 
    private GameObject mainMenuPanel;
    
    [SerializeField]
    private GameObject chooseDifficultyPanel;

    // Shows the next question and restarts the timer
    private void ShowNextQuestion()
    {
        DisplayQuestion();
        timeRemaining = questionTimeLimit;
        isTimerRunning = true;
    }
    
    private IEnumerator ShowCorrectAndAdvance()
    {
        isTimerRunning = false;
        if (correctPopUpScoreText != null) correctPopUpScoreText.text = score.ToString();
        if (correctPopUp != null) correctPopUp.SetActive(true);

        yield return new WaitForSeconds(correctPopUpDuration);

        if (correctPopUp != null) correctPopUp.SetActive(false);
        ShowNextQuestion();
    }
    
    
    // TriviaManager call this with the questions for the current round
    public void LoadQuestions(List<Question> fetchedQuestions)
    {
        questions = fetchedQuestions;
        currentIndex = 0;
        isGameOver = false;
        ShowNextQuestion(); // sets timer + displays question
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
            onAnswerWrong?.Invoke();
            UIMessagingManager.instance.AnswerWrong();
            if (hud != null) hud.SetActive(false);
            if (gameOverScreen != null) gameOverScreen.SetActive(true);
            if (gameOverScoreText != null) gameOverScoreText.text = $"Your final score:\n{score}";
            UpdateBestScore();
            return false;
        }

        score += selectedDifficulty switch
        {
            "medium" => mediumScore,
            "hard" => hardScore,
            _ => easyScore
        };
        onAnswerCorrect?.Invoke();
        UIMessagingManager.instance.AnswerCorrect();
        onScoreUpdated?.Invoke(score);
        currentIndex++;

        bool roundComplete = (currentIndex % questionsPerRound == 0);
        bool allDone = (currentIndex >= questions.Count);

        if (roundComplete || allDone)
        {
            isGameOver = true;
            isTimerRunning = false;
            isPlayerWon = allDone;
            StartCoroutine(ShowCorrectPopUpThenRoundFinished());
            return true;
        }

        StartCoroutine(ShowCorrectAndAdvance());
        return true;
        
    }
    
    private IEnumerator ShowCorrectPopUpThenRoundFinished()
    {
        if (correctPopUpScoreText != null) correctPopUpScoreText.text = score.ToString();
        if (correctPopUp != null) correctPopUp.SetActive(true);

        yield return new WaitForSeconds(correctPopUpDuration);

        if (correctPopUp != null) correctPopUp.SetActive(false);
        if (roundFinishedScoreText != null) roundFinishedScoreText.text = $"Your score:\n{score}";
        UpdateBestScore();
        if (roundFinishedScreen != null) roundFinishedScreen.SetActive(true);
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
        int best = PlayerPrefs.GetInt("BestScore", 0);
        if (bestScoreText != null) bestScoreText.text = $"Best Score:\n{best}";
    }
    
    private void UpdateBestScore()
    {
        int best = PlayerPrefs.GetInt("BestScore", 0);
        if (score > best)
        {
            PlayerPrefs.SetInt("BestScore", score);
            best = score;
        }
        if (bestScoreText != null) bestScoreText.text = $"Best Score:\n{best}";
    }
    
    public void StartGame()
    {
        StopAllCoroutines();
        isFetching = false;
        retryCount = 0; 
        score = 0;
        currentIndex = 0;
        isGameOver = false;
        isPlayerWon = false;
        RequestQuestions();
    }

    private void RequestQuestions()
    {
        if (loadingScreen != null) loadingScreen.SetActive(true);
        if (failedFetchPopUp != null) failedFetchPopUp.SetActive(false);
        StartCoroutine(FetchAndLoad());
        UIMessagingManager.instance.LoadingStarted();
    }

    private IEnumerator FetchAndLoad()
    {
        isFetching = true;
        timeoutCoroutine = StartCoroutine(FetchTimeout());
        yield return StartCoroutine(triviaDataManager.GetTriviaQuestions(questionsPerLevel, selectedDifficulty));

        if (!isFetching) yield break;  // timeout already handled it, stop here
        isFetching = false;

        if (triviaDataManager.Questions != null && triviaDataManager.Questions.Count > 0)
            OnFetchSuccess(triviaDataManager.Questions);
        else
            OnFetchFailed();
    }


    private IEnumerator FetchTimeout()
    {
        yield return new WaitForSeconds(15f);
        isFetching = false;
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
        UIMessagingManager.instance.LoadingCompleted();
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
        {
            int totalSeconds = Mathf.CeilToInt(timeRemaining);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
        
        if (timeRemaining <= 0.0f)
        {
            timeRemaining = 0.0f;
            isTimerRunning = false;
            SubmitAnswer(""); // empty = wrong answer
        }
    }
    public void ShowDifficultySelection()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (chooseDifficultyPanel != null) chooseDifficultyPanel.SetActive(true);
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
    
    public void SetDifficultyFromDropdown(int index)
    {
        string[] difficulties = { "easy", "medium", "hard" };
        if (index >= 0 && index < difficulties.Length)
            SetDifficulty(difficulties[index]);
    }
    
    public void NextQuestions()
    {
        if (roundFinishedScreen != null) roundFinishedScreen.SetActive(false);
        isGameOver = false;
        isPlayerWon = false;

        if (currentIndex >= questions.Count)
        {
            // All questions used, fetch new batch
            currentIndex = 0;
            if (hud != null) hud.SetActive(true);
            RequestQuestions();
        }
        else
        {
            // Questions remaining, continue
            if (hud != null) hud.SetActive(true);
            ShowNextQuestion();
        }
    }
    
}
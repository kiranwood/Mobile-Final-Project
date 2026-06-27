using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class GamePlayManager : MonoBehaviour
{
    // Editable in Inspector

    [SerializeField][Tooltip("Number of questions per level")]
    public int questionsPerLevel = 100;

    
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
    public List<TriviaQuestion> questions = new List<TriviaQuestion>();
    private int currentIndex = 0;
    public int score = 0;
    public bool isGameOver = false;

    // Current difficulty based on round
    public string CurrentDifficulty => selectedDifficulty;
    
    public int CurrentRoundQuestionCount => questionsPerLevel;
    public bool isPlayerWon = false;
   
    
    // TriviaManager call this with the questions for the current round
    public void LoadQuestions(List<TriviaQuestion> fetchedQuestions)
    {
        questions = fetchedQuestions;
        timeRemaining = questionTimeLimit;
        isTimerRunning = true;
        ShuffleQuestions();
        currentIndex = 0;
        isGameOver = false;
    }

    public TriviaQuestion GetCurrentQuestion()
    {
        if(isGameOver || questions.Count == 0) return null;
        return questions[currentIndex];
    }

    public bool SubmitAnswer(string answer)
    {
        if (isGameOver) return false;

        TriviaQuestion question = GetCurrentQuestion();
        bool correct = (answer == question.correct_answer);

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


    public List<string> GetShuffledAnswers()
    {
        TriviaQuestion question = GetCurrentQuestion();
        if (question == null) return null;
        
        List<string> answers = new List<string>(question.incorrect_answers){question.correct_answer};
        Shuffle(answers);
        return answers;
    }

    private void ShuffleQuestions()
    {
        for (int i = questions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (questions[i], questions[j]) = (questions[j], questions[i]);
        }
    }
    
    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
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

        //timeoutCoroutine = StartCoroutine(FetchTimeout());
        //onNeedQuestions?.Invoke();
    }

    /*private IEnumerator FetchTimeout()
    {
        yield return new WaitForSeconds(15f);
        OnFetchFailed();  // auto-called if no response in 15 sec
    }*/
    
    // TriviaManager calls this when fetch succeeds
    public void OnFetchSuccess(List<TriviaQuestion> fetchedQuestions)
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


[System.Serializable]
public class TriviaQuestion
{
    public string type;
    public string difficulty;
    public string category;
    public string question;
    public string correct_answer;
    public List<string> incorrect_answers;
}
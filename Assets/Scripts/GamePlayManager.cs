using System.Collections.Generic;
using UnityEngine;

public class GamePlayManager : MonoBehaviour
{
    // Editable in Inspector

    public int round1QuestionCount = 5;
    public int round2QuestionCount = 10;
    public int round3QuestionCount = 20;
    
    // Game state 
    public List<TriviaQuestion> questions = new List<TriviaQuestion>();
    private int currentIndex = 0;
    public int score = 0;
    public int currentRound = 1;
    public bool isGameOver = false;

    // Current difficulty based on round
    public string CurrentDifficulty
    {
        get
        {
            if(currentRound == 1) 
                return "easy";
            if(currentRound == 2)
                return "medium";
            return "hard";
        }
    }
    
    // How many questions for the current round
    public int CurrentRoundQuestionCount
    {
        get
        {
            if(currentRound == 1)
                return round1QuestionCount;
            if(currentRound == 2)
                return round2QuestionCount;
            return round3QuestionCount;
        }
    }
    
    // TriviaManager call this with the questions for the current round
    public void LoadQuestions(List<TriviaQuestion> fetchedQuestions)
    {
        questions = fetchedQuestions;
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
        if(isGameOver) return false;
        
        TriviaQuestion question = GetCurrentQuestion();
        bool correct = (answer == question.correct_answer);
        if(correct) score++;
        
        currentRound++;
        if (currentIndex >= questions.Count)
            AdvanceRound();
        
        return correct;
    }

    private void AdvanceRound()
    {
        if (currentRound >= 3)
        {
            isGameOver = true;
            Debug.Log($"Game Over! Final score: {score}");
        }
        else
        {
            //  TriviaManager should now fetch questions using CurrentDifficulty and CurrentRoundQuestionCount
            currentRound++;
        }
    }

    public List<string> GetShuffledAnswers()
    {
        TriviaQuestion question = GetCurrentQuestion();
        if (question == null) return null;
        
        List<string> answers = new List<string>(question.incorrect_answer){question.correct_answer};
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

}


[System.Serializable]
public class TriviaQuestion
{
    public string type;
    public string difficulty;
    public string category;
    public string question;
    public string correct_answer;
    public List<string> incorrect_answer;
}
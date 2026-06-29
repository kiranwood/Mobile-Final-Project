using UnityEngine;

[System.Serializable]
public class Question
{
    public string QuestionText {  get; set; }

    public int CorrectAnswerID { get; set; }

    public string[] AnswerOptions { get; set; }
}

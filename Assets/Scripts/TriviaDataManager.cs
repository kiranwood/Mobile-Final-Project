using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class TriviaDataManager : MonoBehaviour
{
    public TriviaDataManager Instance {  get; private set; }

    private string _sessionToken;
    public List<Question> Questions { get; private set; }

    private void Awake()
    {
        // Setting singleton instance
        if (Instance != null)
        {
            Destroy(Instance);
        }

        Instance = this;

        // Gets sessions token
        StartCoroutine(GetSessionToken());
    }

    IEnumerator GetSessionToken()
    {
        UnityWebRequest request = UnityWebRequest.Get("https://opentdb.com/api_token.php?command=request");
        yield return request.SendWebRequest();

        // Logs error
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        // Sets sessions token
        else
        {
            Dictionary<string, string> response = ParseResponse(request.downloadHandler.text);
            _sessionToken = response["token"];
        }
    }

    // Parses the data into a dictionary
    private Dictionary<string, string> ParseResponse(string response)
    {
        Dictionary<string, string> parsedResponse = new();

        // Filter string into array of values
        string trimmedResponse = response.Replace("{", "").Replace("}", "").Replace(":", "").Replace(",", "").Replace("]", "");
        string[] splitResponse = trimmedResponse.Split('"');
        splitResponse = splitResponse.Where(x => !string.IsNullOrEmpty(x)).ToArray();

        // Transfer array into dictionary
        for (int i = 0; i < splitResponse.Length - 1; i += 2)
        {
            parsedResponse[splitResponse[i]] = splitResponse[i + 1];
        }

        return parsedResponse;
        
    }

    /// <summary>
    /// 
    /// Gets a set number of questions based on difficulty
    /// 
    /// </summary>
    public IEnumerator GetTriviaQuestions(int questionAmount, string difficulty)
    {
        Questions = new List<Question>();

        UnityWebRequest request = UnityWebRequest.Get(  $"https://opentdb.com/api.php?amount={questionAmount}" +
                                                        $"&difficulty={difficulty}&type=multiple");
        yield return request.SendWebRequest();

        // Logs error
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);

            // Tries to get another token
            yield return StartCoroutine(GetSessionToken());
        }
        // Sets sessions token
        else
        {
            // Filters the results
            string[] results = request.downloadHandler.text.Split("[", 2);
            results = results[1].Split("{");
            results = results.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            
            foreach (string question in results)
            {
                // Parses the first half of data
                string[] data = question.Split("[");
                Dictionary<string, string> questionData = ParseResponse(data[0]);
                questionData["question"] = DecodeString(questionData["question"]);


                // Gets the other optional answers for question
                string trimmedAnswers= data[1].Replace("{", "").Replace("}", "").Replace(":", "").Replace(",", "").Replace("]", "");
                string[] splitAnswers = trimmedAnswers.Split('"');
                splitAnswers = splitAnswers.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                // Merge answers
                splitAnswers = splitAnswers.Append(questionData["correct_answer"]).ToArray();
                
                // Shuffle Answers
                for (int i  = 0; i < splitAnswers.Length; i++)
                {
                    string tmp = splitAnswers[i];
                    int r = Random.Range(i, splitAnswers.Length);
                    splitAnswers[i] = splitAnswers[r];
                    splitAnswers[r] = tmp;
                }

                // Decodes answers and gets correct answer index
                int correctAnswerIndex = 0;
                for (int i = 0; i < splitAnswers.Length; i++)
                {
                    if (splitAnswers[i] == questionData["correct_answer"])
                    {
                        correctAnswerIndex = i;
                    }

                    splitAnswers[i] = DecodeString(splitAnswers[i]);
                }

                // Connect Final Question
                Question finalQuestion = new();
                finalQuestion.QuestionText = questionData["question"];
                finalQuestion.AnswerOptions = splitAnswers;
                finalQuestion.CorrectAnswerID = correctAnswerIndex;
                
                Questions.Add(finalQuestion);
            }
        }
    }

    // Decodes the string
    private string DecodeString(string input)
    {
        input = input.Replace("&#039;", "'");
        input = input.Replace("&quot;", "\"");
        input = input.Replace("&‌amp;", "&");
        input = input.Replace("&‌pi;", "π");

        return input;
    }
}

using UnityEngine;

public class TriviaManager : MonoBehaviour
{
    public TriviaManager Instance {  get; private set; }

    private void Awake()
    {
        // Setting singleton instance
        if (Instance != null)
        {
            Destroy(Instance);
        }

        Instance = this;
    }



}

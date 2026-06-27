using UnityEngine;

public class TriviaDataManager : MonoBehaviour
{
    public TriviaDataManager Instance {  get; private set; }

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

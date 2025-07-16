using UnityEngine;

public class GameSystems : MonoBehaviour
{
    public static GameSystems Instance { get; private set; }

    private void Awake()
    {
        // Ensure only one instance of GameSystems exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

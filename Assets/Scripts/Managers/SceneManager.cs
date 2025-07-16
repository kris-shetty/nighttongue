using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance { get; private set; }

    public void RestartScene()
    {
        // Get the currently active scene
        Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        // Reload the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
    }

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartScene();
        }
    }
}

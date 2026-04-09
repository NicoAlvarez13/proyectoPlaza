using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameLanguage
    {
        english,
        spanish
    }

    private GameLanguage _gameLanguage = GameLanguage.spanish;
    public GameLanguage CurrentLanguage => _gameLanguage;

    private void Awake()
    {
        SingletonInitialization();
        InitializeDefaultLanguage();
    }

    private void SingletonInitialization()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDefaultLanguage()
    {
        // Application.systemLanguage detects the device OS or browser language
        if (Application.systemLanguage == SystemLanguage.English)
        {
            _gameLanguage = GameLanguage.english;
        }
        else
        {
            // Default to English for any other detected language
            _gameLanguage = GameLanguage.spanish;
        }

        Debug.Log($"System Language detected: {Application.systemLanguage}. Game language set to: {_gameLanguage}");
    }

    public void SetLanguage(GameLanguage language)
    {
        _gameLanguage = language;
    }
}
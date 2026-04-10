using UnityEngine;

public class TriviaCharacterManager : MonoBehaviour
{
    public static TriviaCharacterManager Instance { get; private set; }

    [SerializeField] private TriviaCharacter[] _characters;

    private int _lastShownIndex = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        HideAll();
    }

    /// <summary>
    /// Muestra un personaje aleatorio distinto al anterior.
    /// </summary>
    public void ShowRandom()
    {
        if (_characters == null || _characters.Length == 0) return;

        HideAll();

        int next = _lastShownIndex;

        if (_characters.Length > 1)
        {
            while (next == _lastShownIndex)
                next = Random.Range(0, _characters.Length);
        }
        else
        {
            next = 0;
        }

        _lastShownIndex = next;
        _characters[next]?.Show();
    }

    /// <summary>
    /// Oculta todos los personajes.
    /// </summary>
    public void HideAll()
    {
        if (_characters == null) return;
        foreach (var c in _characters)
            c?.Hide();
    }
}

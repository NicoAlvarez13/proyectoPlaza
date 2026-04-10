using UnityEngine;
using UnityEngine.UIElements;

public class TriviaCharacterManager : MonoBehaviour
{
    public static TriviaCharacterManager Instance { get; private set; }

    [SerializeField] private TriviaCharacter[] _characters;

    private int  _lastShownIndex = -1;
    private bool _initialized;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Called by TriviaGameUIController once the CharacterLayer VisualElement is ready.
    /// Creates one background-image VE per character and hands it to TriviaCharacter.
    /// </summary>
    public void Initialize(VisualElement characterLayer)
    {
        characterLayer.Clear();

        foreach (var c in _characters)
        {
            if (c == null || c.sprite == null) continue;

            var img = new VisualElement();
            img.style.position         = Position.Absolute;
            img.style.backgroundImage = new StyleBackground(c.sprite);
            img.style.backgroundSize  = new StyleBackgroundSize(
                                            new BackgroundSize(BackgroundSizeType.Contain));
            img.style.display  = DisplayStyle.None;
            img.pickingMode    = PickingMode.Ignore;

            characterLayer.Add(img);
            c.SetElement(img);
        }

        _initialized = true;
        HideAll();
    }

    private void Update()
    {
        if (!_initialized || _characters == null) return;
        foreach (var c in _characters)
            c?.Tick(Time.deltaTime);
    }

    /// <summary>Shows a random character different from the last one shown.</summary>
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

    /// <summary>Hides all characters.</summary>
    public void HideAll()
    {
        if (_characters == null) return;
        foreach (var c in _characters)
            c?.Hide();
    }
}

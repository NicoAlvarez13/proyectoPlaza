using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TriviaGame_UICharcatersAnimations : MonoBehaviour
{
    public static TriviaGame_UICharcatersAnimations Instance { get; private set; }

    [Header("Escalador Settings")]
    [Tooltip("Lower value = faster animation")]
    public float EscaladorAnimDuration = 3f;

    [Header("Averga Settings")]
    public float AvergaAnimDuration = 3f;

    [Header("Limawe Settings")]
    public float LimaweMoveDuration = 3f;
    public float LimaweRotDuration = 4f;
    public float LimaweMinY = 0f;
    public float LimaweMaxY = 180f;

    [Header("Astronauta Settings")]
    public float AstronautMoveDuration = 3f;
    public float AstronautRotDuration = 4f;
    public float AstronautMinY = -320f;
    public float AstronautMaxY = 0f;

    private UIDocument _uiDocument;
    private VisualElement _characterLayer;

    // Character Images
    private Image _escalador;
    private Image _averga;
    private Image _astronauta;
    private Image _alonso;
    private Image _limawe;

    // State Tracking
    private Image _currentActiveCharacter = null;
    private Image _lastShownCharacter = null;

    // Animation States
    private bool _escaladorForward = true;
    private bool _avergaForward = true;
    private bool _limaweXForward = true;
    private bool _astronautaXForward = true;

    // Rotation accumulators to avoid snapping issues
    private float _limaweRot = 0f;
    private float _astronautaRot = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;

        var root = _uiDocument.rootVisualElement;
        _characterLayer = root.Q<VisualElement>("CharacterLayer");

        if (_characterLayer != null)
        {
            _escalador = _characterLayer.Q<Image>("Escalador");
            _averga = _characterLayer.Q<Image>("Averga");
            _astronauta = _characterLayer.Q<Image>("Astronauta");
            _alonso = _characterLayer.Q<Image>("Alonso");
            _limawe = _characterLayer.Q<Image>("Limawe");

            HideAllCharacters();
            RegisterAnimationCallbacks();
        }
    }

    public void ShowCharacterForCategory(string categoryName)
    {
        if (_characterLayer == null) return;

        HideAllCharacters();

        string catLower = categoryName.ToLower();
        List<Image> validCharacters = new List<Image>();

        if (catLower.Contains("earth") || catLower.Contains("tierra") ||
            catLower.Contains("fire") || catLower.Contains("fuego"))
        {
            validCharacters.Add(_escalador);
            validCharacters.Add(_alonso);
        }
        else if (catLower.Contains("air") || catLower.Contains("aire"))
        {
            validCharacters.Add(_escalador);
            validCharacters.Add(_alonso);
            validCharacters.Add(_limawe);
        }
        else if (catLower.Contains("water") || catLower.Contains("agua") || catLower.Contains("bonus"))
        {
            validCharacters.Add(_averga);
            validCharacters.Add(_astronauta);
        }

        if (validCharacters.Count > 1 && _lastShownCharacter != null && validCharacters.Contains(_lastShownCharacter))
        {
            validCharacters.Remove(_lastShownCharacter);
        }

        if (validCharacters.Count > 0)
        {
            Image chosenCharacter = validCharacters[Random.Range(0, validCharacters.Count)];
            ActivateCharacter(chosenCharacter);
        }
    }

    public void HideAllCharacters()
    {
        if (_escalador != null) _escalador.style.display = DisplayStyle.None;
        if (_averga != null) _averga.style.display = DisplayStyle.None;
        if (_astronauta != null) _astronauta.style.display = DisplayStyle.None;
        if (_alonso != null) _alonso.style.display = DisplayStyle.None;
        if (_limawe != null) _limawe.style.display = DisplayStyle.None;

        _currentActiveCharacter = null;
    }

    private void ActivateCharacter(Image character)
    {
        character.style.display = DisplayStyle.Flex;
        _currentActiveCharacter = character;
        _lastShownCharacter = character;

        character.style.transitionProperty = new StyleList<StylePropertyName>(StyleKeyword.None);
        character.RegisterCallback<GeometryChangedEvent>(OnCharacterGeometryCalculated);
    }

    private void OnCharacterGeometryCalculated(GeometryChangedEvent evt)
    {
        var character = evt.target as Image;
        if (character == null) return;

        character.UnregisterCallback<GeometryChangedEvent>(OnCharacterGeometryCalculated);

        SetupTransitions(character);
        StartAnimation(character);
    }

    private void SetupTransitions(Image img)
    {
        img.style.transitionProperty = new StyleList<StylePropertyName>(StyleKeyword.None);

        if (img == _escalador)
        {
            img.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("rotate") };
            img.style.transitionDuration = new List<TimeValue> { new TimeValue(EscaladorAnimDuration, TimeUnit.Second) };
            img.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(EasingMode.Linear) };
        }
        else if (img == _averga)
        {
            img.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate") };
            img.style.transitionDuration = new List<TimeValue> { new TimeValue(AvergaAnimDuration, TimeUnit.Second) };
            img.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(EasingMode.Linear) };
        }
        else if (img == _limawe)
        {
            img.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate"), new StylePropertyName("rotate") };
            img.style.transitionDuration = new List<TimeValue> { new TimeValue(LimaweMoveDuration, TimeUnit.Second), new TimeValue(LimaweRotDuration, TimeUnit.Second) };
            img.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(EasingMode.Linear), new EasingFunction(EasingMode.Linear) };
        }
        else if (img == _astronauta)
        {
            img.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate"), new StylePropertyName("rotate") };
            img.style.transitionDuration = new List<TimeValue> { new TimeValue(AstronautMoveDuration, TimeUnit.Second), new TimeValue(AstronautRotDuration, TimeUnit.Second) };
            img.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(EasingMode.Linear), new EasingFunction(EasingMode.Linear) };
        }
    }

    private void StartAnimation(Image img)
    {
        if (img == _escalador)
        {
            _escaladorForward = true;

            img.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0, TimeUnit.Second) });
            img.style.rotate = new StyleRotate(new Rotate(new Angle(12f, AngleUnit.Degree)));

            img.schedule.Execute(() =>
            {
                SetupTransitions(img);
                img.style.rotate = new StyleRotate(new Rotate(new Angle(-45f, AngleUnit.Degree)));
            }).StartingIn(50);
        }
        else if (img == _averga)
        {
            _avergaForward = true;

            img.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0, TimeUnit.Second) });
            img.style.translate = new StyleTranslate(new Translate(new Length(2f, LengthUnit.Percent), new Length(0f, LengthUnit.Percent), 0));

            img.schedule.Execute(() =>
            {
                SetupTransitions(img);
                img.style.translate = new StyleTranslate(new Translate(new Length(2f, LengthUnit.Percent), new Length(-96f, LengthUnit.Percent), 0));
            }).StartingIn(50);
        }
        else if (img == _limawe)
        {
            float startY = Random.Range(LimaweMinY, LimaweMaxY);
            float targetY = Random.Range(LimaweMinY, LimaweMaxY);
            _limaweXForward = Random.value > 0.5f;

            float targetX = _limaweXForward ? 210f : -210f;
            float startX = _limaweXForward ? -210f : 210f;

            _limaweRot = 0f;

            // Snap to initial start point
            img.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0, TimeUnit.Second), new TimeValue(0, TimeUnit.Second) });
            img.style.translate = new StyleTranslate(new Translate(new Length(startX, LengthUnit.Percent), new Length(startY, LengthUnit.Percent), 0));
            img.style.rotate = new StyleRotate(new Rotate(new Angle(_limaweRot, AngleUnit.Degree)));

            img.schedule.Execute(() =>
            {
                SetupTransitions(img);
                _limaweRot = 360f;
                img.style.translate = new StyleTranslate(new Translate(new Length(targetX, LengthUnit.Percent), new Length(targetY, LengthUnit.Percent), 0));
                img.style.rotate = new StyleRotate(new Rotate(new Angle(_limaweRot, AngleUnit.Degree)));
            }).StartingIn(50);
        }
        else if (img == _astronauta)
        {
            float startY = Random.Range(AstronautMinY, AstronautMaxY);
            float targetY = Random.Range(AstronautMinY, AstronautMaxY);
            _astronautaXForward = Random.value > 0.5f;

            float targetX = _astronautaXForward ? 355f : -355f;
            float startX = _astronautaXForward ? -355f : 355f;

            _astronautaRot = 0f;

            // Snap to initial start point
            img.style.transitionDuration = new StyleList<TimeValue>(new List<TimeValue> { new TimeValue(0, TimeUnit.Second), new TimeValue(0, TimeUnit.Second) });
            img.style.translate = new StyleTranslate(new Translate(new Length(startX, LengthUnit.Percent), new Length(startY, LengthUnit.Percent), 0));
            img.style.rotate = new StyleRotate(new Rotate(new Angle(_astronautaRot, AngleUnit.Degree)));

            img.schedule.Execute(() =>
            {
                SetupTransitions(img);
                _astronautaRot = 360f;
                img.style.translate = new StyleTranslate(new Translate(new Length(targetX, LengthUnit.Percent), new Length(targetY, LengthUnit.Percent), 0));
                img.style.rotate = new StyleRotate(new Rotate(new Angle(_astronautaRot, AngleUnit.Degree)));
            }).StartingIn(50);
        }
        else if (img == _alonso)
        {
            bool isLeft = Random.value > 0.5f;
            float randomY = Random.Range(-50f, 150f);

            img.style.translate = new StyleTranslate(new Translate(0, new Length(randomY, LengthUnit.Percent), 0));

            if (isLeft)
            {
                img.style.scale = new StyleScale(new Scale(new Vector3(-1f, 1f, 1f)));
                img.style.left = -30f;
                img.style.right = new StyleLength(StyleKeyword.Auto);
            }
            else
            {
                img.style.scale = new StyleScale(new Scale(new Vector3(1f, 1f, 1f)));
                img.style.right = -30f;
                img.style.left = new StyleLength(StyleKeyword.Auto);
            }
        }
    }

    private void RegisterAnimationCallbacks()
    {
        if (_escalador != null) _escalador.RegisterCallback<TransitionEndEvent>(OnEscaladorTransitionEnd);
        if (_averga != null) _averga.RegisterCallback<TransitionEndEvent>(OnAvergaTransitionEnd);
        if (_limawe != null) _limawe.RegisterCallback<TransitionEndEvent>(OnLimaweTransitionEnd);
        if (_astronauta != null) _astronauta.RegisterCallback<TransitionEndEvent>(OnAstronautaTransitionEnd);
    }

    private void OnEscaladorTransitionEnd(TransitionEndEvent evt)
    {
        if (_currentActiveCharacter != _escalador || !evt.stylePropertyNames.Contains("rotate")) return;

        _escaladorForward = !_escaladorForward;
        float targetAngle = _escaladorForward ? -45f : 12f;
        // FIX: Replaced Gradian with Degree
        _escalador.style.rotate = new StyleRotate(new Rotate(new Angle(targetAngle, AngleUnit.Degree)));
    }

    private void OnAvergaTransitionEnd(TransitionEndEvent evt)
    {
        if (_currentActiveCharacter != _averga || !evt.stylePropertyNames.Contains("translate")) return;

        _avergaForward = !_avergaForward;
        float targetY = _avergaForward ? -96f : 0f;
        _averga.style.translate = new StyleTranslate(new Translate(new Length(2f, LengthUnit.Percent), new Length(targetY, LengthUnit.Percent), 0));
    }

    private void OnLimaweTransitionEnd(TransitionEndEvent evt)
    {
        if (_currentActiveCharacter != _limawe) return;

        if (evt.stylePropertyNames.Contains("translate"))
        {
            _limaweXForward = !_limaweXForward;
            float targetX = _limaweXForward ? 210f : -210f;

            float nextTargetY = Random.Range(LimaweMinY, LimaweMaxY);
            _limawe.style.translate = new StyleTranslate(new Translate(new Length(targetX, LengthUnit.Percent), new Length(nextTargetY, LengthUnit.Percent), 0));
        }

        if (evt.stylePropertyNames.Contains("rotate"))
        {
            // FIX: Instead of snapping to 0, simply accumulate 360 degrees
            _limaweRot += 360f;
            _limawe.style.rotate = new StyleRotate(new Rotate(new Angle(_limaweRot, AngleUnit.Degree)));
        }
    }

    private void OnAstronautaTransitionEnd(TransitionEndEvent evt)
    {
        if (_currentActiveCharacter != _astronauta) return;

        if (evt.stylePropertyNames.Contains("translate"))
        {
            _astronautaXForward = !_astronautaXForward;
            float targetX = _astronautaXForward ? 355f : -355f;

            float nextTargetY = Random.Range(AstronautMinY, AstronautMaxY);
            _astronauta.style.translate = new StyleTranslate(new Translate(new Length(targetX, LengthUnit.Percent), new Length(nextTargetY, LengthUnit.Percent), 0));
        }

        if (evt.stylePropertyNames.Contains("rotate"))
        {
            // FIX: Instead of snapping to 0, simply accumulate 360 degrees
            _astronautaRot += 360f;
            _astronauta.style.rotate = new StyleRotate(new Rotate(new Angle(_astronautaRot, AngleUnit.Degree)));
        }
    }

    private void OnDisable()
    {
        if (_escalador != null) _escalador.UnregisterCallback<TransitionEndEvent>(OnEscaladorTransitionEnd);
        if (_averga != null) _averga.UnregisterCallback<TransitionEndEvent>(OnAvergaTransitionEnd);
        if (_limawe != null) _limawe.UnregisterCallback<TransitionEndEvent>(OnLimaweTransitionEnd);
        if (_astronauta != null) _astronauta.UnregisterCallback<TransitionEndEvent>(OnAstronautaTransitionEnd);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; private set; }

    [SerializeField] private UIDocument _uiDocument;

    // --- Constants ---
    private const string LAST_ROOM_KEY = "LastRoomCode";


    // --- UI Elements ---
    private Button _btnPlay;
    private Button _joinButton;
    private Button _reconnectButton;
    private VisualElement _reconnectWrapper;
    private TextField _codeInput;
    private VisualElement _menuesContainer;
    private VisualElement _clouds;
    private Label _lblError;
    private Label _lblEnterCode;
    private VisualElement _playButtonBack;
    private VisualElement _panelDatos;
    private Button _btnDevJoin;
    private Button _btnBanderaEsp;
    private Button _btnBanderaEng;

    public bool IsProcessing = false;

    // Transition Elements
    private bool _transitioned;
    private bool _transitioning = false;
    private int _pendingTransitions = 0;

    // Decoratives
    [Header("Animation Settings")]
    [SerializeField] private float astronautAnimSpeed = 2f;
    private Image _astronaut;
    private bool _isAstronautDown = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (_uiDocument == null) return;
        VisualElement root = _uiDocument.rootVisualElement;

        // Initialize Elements based on UXML names
        _btnPlay = root.Q<Button>("PlayButton");
        _joinButton = root.Q<Button>("JoinButton");
        _codeInput = root.Q<TextField>("CodeInput");
        _menuesContainer = root.Q<VisualElement>("MenuesContainer");
        _clouds = root.Q<VisualElement>("Clouds");

        _lblError = root.Q<Label>("lblErrorMessage");

        var codeMainContainer = root.Q<VisualElement>("CodeMainContainer");
        if (codeMainContainer != null) _lblEnterCode = codeMainContainer.Q<Label>();


        // Reconnect Button Wrapper & Button
        _reconnectWrapper = root.Q<VisualElement>("btnReconnect");
        if (_reconnectWrapper != null)
        {
            _reconnectButton = _reconnectWrapper.Q<Button>("Button");
        }

        _playButtonBack = root.Q<VisualElement>("PlayButtonBack");
        _panelDatos = root.Q<VisualElement>("PanelDatos");
        _btnBanderaEsp = root.Q<Button>("BanderEsp");
        _btnBanderaEng = root.Q<Button>("BanderaEng");

        _btnDevJoin = root.Q<Button>("BtnDevJoin");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_btnDevJoin != null) _btnDevJoin.style.display = DisplayStyle.Flex;
#endif

        _astronaut = root.Q<Image>("Astronaut");

        SetupInitialState();
        RegisterEvents();

        if (_astronaut != null)
        {
            _astronaut.RegisterCallback<GeometryChangedEvent>(OnGeometryCalculated);
        }
    }

    private void Start()
    {
        CheckUrlParameters();
        ApplyLocalization();
    }

    private void OnDisable()
    {
        UnregisterEvents();
    }

    private void SetupInitialState()
    {
        if (_reconnectWrapper != null)
        {
            bool hasSavedRoom = PlayerPrefs.HasKey(LAST_ROOM_KEY);
            _reconnectWrapper.style.display = hasSavedRoom ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (_playButtonBack != null) _playButtonBack.style.display = DisplayStyle.None;
        if (_panelDatos != null) _panelDatos.style.display = DisplayStyle.Flex;

        if (_codeInput != null) _codeInput.value = string.Empty;

        if (_lblError != null) _lblError.text = string.Empty;

        SetMenuesTransitionToInitialState();
    }

    // --- URL Logic ---
    private void CheckUrlParameters()
    {
        string url = Application.absoluteURL;
        if (string.IsNullOrEmpty(url) || !url.Contains("idsala=")) return;

        string roomCode = GetParamFromUrl(url, "idsala");
        if (!string.IsNullOrEmpty(roomCode))
        {
            if (_codeInput != null) _codeInput.value = roomCode;

            // NEW: Automatically hide the language UI when joining via URL
            ShowPlayButton();

            SetMenuesTransitionToFinalState();
        }
    }

    private string GetParamFromUrl(string url, string paramName)
    {
        try
        {
            string searchString = paramName + "=";
            int startIndex = url.IndexOf(searchString);
            if (startIndex == -1) return null;

            startIndex += searchString.Length;
            int endIndex = url.IndexOf('&', startIndex);

            return (endIndex == -1) ? url.Substring(startIndex) : url.Substring(startIndex, endIndex - startIndex);
        }
        catch { return null; }
    }


    #region Animation Logic

    private void OnGeometryCalculated(GeometryChangedEvent evt)
    {
        _astronaut.UnregisterCallback<GeometryChangedEvent>(OnGeometryCalculated);
        StartAstronautAnimation();
    }

    private void StartAstronautAnimation()
    {
        if (_astronaut == null) return;

        _astronaut.style.translate = new Translate(new Length(0, LengthUnit.Percent), new Length(20, LengthUnit.Percent));
        _astronaut.style.transitionDuration = new List<TimeValue> { new TimeValue(astronautAnimSpeed, TimeUnit.Second) };
        _astronaut.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate") };
        _astronaut.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(EasingMode.EaseInOutSine) };

        _astronaut.RegisterCallback<TransitionEndEvent>(OnAstronautTransitionEnd);
        _astronaut.schedule.Execute(MoveAstronautDown).StartingIn(50);
    }

    private void MoveAstronautDown()
    {
        _isAstronautDown = true;
        _astronaut.style.translate = new Translate(new Length(0, LengthUnit.Percent), new Length(45, LengthUnit.Percent));
    }

    private void MoveAstronautUp()
    {
        _isAstronautDown = false;
        _astronaut.style.translate = new Translate(new Length(0, LengthUnit.Percent), new Length(0, LengthUnit.Percent));
    }

    private void OnAstronautTransitionEnd(TransitionEndEvent evt)
    {
        if (!evt.stylePropertyNames.Contains("translate")) return;

        if (_isAstronautDown)
        {
            MoveAstronautUp();
        }
        else
        {
            MoveAstronautDown();
        }
    }

    #endregion

    // --- Event Management ---
    private void RegisterEvents()
    {
        if (_btnPlay != null) _btnPlay.clicked += OnPlayButtonClicked;
        if (_joinButton != null) _joinButton.clicked += OnJoinButtonClicked;
        if (_reconnectButton != null) _reconnectButton.clicked += OnReconnectButtonClicked;
        if (_btnBanderaEsp != null) _btnBanderaEsp.clicked += OnBanderaEspClicked;
        if (_btnBanderaEng != null) _btnBanderaEng.clicked += OnBanderaEngClicked;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_btnDevJoin != null) _btnDevJoin.clicked += OnDevJoinClicked;
#endif

        if (_codeInput != null)
        {
            _codeInput.isDelayed = true;
            _codeInput.RegisterCallback<KeyDownEvent>(OnCodeInputEnter);
        }
    }

    private void UnregisterEvents()
    {
        if (_btnPlay != null) _btnPlay.clicked -= OnPlayButtonClicked;
        if (_joinButton != null) _joinButton.clicked -= OnJoinButtonClicked;
        if (_reconnectButton != null) _reconnectButton.clicked -= OnReconnectButtonClicked;
        if (_btnBanderaEsp != null) _btnBanderaEsp.clicked -= OnBanderaEspClicked;
        if (_btnBanderaEng != null) _btnBanderaEng.clicked -= OnBanderaEngClicked;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (_btnDevJoin != null) _btnDevJoin.clicked -= OnDevJoinClicked;
#endif

        if (_codeInput != null) _codeInput.UnregisterCallback<KeyDownEvent>(OnCodeInputEnter);
    }

    #region Menu Transition Logic
    private void OnPlayButtonClicked() => ToggleMenuesTransition();

    public void ToggleMenuesTransition(bool instant = false, bool toggle = true, bool transitioned = false)
    {
        if (_transitioning) return;

        bool willMoveDown = (toggle && !_transitioned) || (!toggle && !_transitioned && transitioned);
        bool willMoveUp = (toggle && _transitioned) || (!toggle && _transitioned && !transitioned);

        if (!willMoveDown && !willMoveUp) return;

        _transitioning = true;

        SetDuration(_menuesContainer, instant ? 0f : 1.2f);
        SetDuration(_clouds, instant ? 0f : 1.4f);

        if (willMoveDown)
        {
            SetTranslate(_menuesContainer, 0, 120);
            SetTranslate(_clouds, 0, 0);
            _transitioned = true;
            AudioManager.Instance?.PlayLobbyMusic();
        }
        else
        {
            SetTranslate(_menuesContainer, 0, 0);
            SetTranslate(_clouds, 0, -86);
            _transitioned = false;
        }

        if (instant)
        {
            _transitioning = false;
            OnTransitionComplete();
            return;
        }

        _pendingTransitions = 0;
        if (_menuesContainer != null) RegisterTransitionEnd(_menuesContainer);
        if (_clouds != null) RegisterTransitionEnd(_clouds);
    }
    public void SetMenuesTransitionToFinalState()
    {
        if (!_transitioning)
        {
            _transitioned = false;
            ToggleMenuesTransition(true, false, true);
        }
    }
    public void SetMenuesTransitionToInitialState()
    {
        if (!_transitioning)
        {
            _transitioned = true;
            ToggleMenuesTransition(true, false, false);
        }
    }

    private void SetDuration(VisualElement el, float seconds)
    {
        if (el != null)
            el.style.transitionDuration = new List<TimeValue> { new TimeValue(seconds) };
    }

    private void SetTranslate(VisualElement el, float x, float yPercent)
    {
        if (el != null)
            el.style.translate = new StyleTranslate(
                new Translate(x, new Length(yPercent, LengthUnit.Percent), 0)
            );
    }

    private void RegisterTransitionEnd(VisualElement element)
    {
        _pendingTransitions++;
        EventCallback<TransitionEndEvent> callback = null;
        callback = (_) =>
        {
            element.UnregisterCallback(callback);
            if (--_pendingTransitions <= 0)
            {
                _transitioning = false;
                OnTransitionComplete();
            }
        };
        element.RegisterCallback(callback);
    }

    private void OnTransitionComplete()
    {
        Debug.Log("Transition finished!");
    }
    #endregion

    public void SetUIState(bool isVisible)
    {
        if (_uiDocument != null && _uiDocument.rootVisualElement != null)
        {
            _uiDocument.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }


    //Joining logic methods
    private void OnJoinButtonClicked()
    {
        if (_codeInput != null && !string.IsNullOrEmpty(_codeInput.value))
        {
            HandleRoomJoining(_codeInput.value);
        }
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void OnDevJoinClicked()
    {
        if (IsProcessing) return;
        IsProcessing = true;

        if (GameManager.Instance != null && GameManager.Instance.CurrentLanguage == default)
            //GameManager.Instance.SetLanguage(GameManager.GameLanguage.spanish);

            SetMenuesTransitionToFinalState();

        if (_btnDevJoin != null) _btnDevJoin.SetEnabled(false);

        QuizNetworkManager.Instance.DevCreateAndPlay((success, errorMsg) =>
        {
            IsProcessing = false;
            if (_btnDevJoin != null) _btnDevJoin.SetEnabled(true);

            if (!success)
            {
                SetUIState(true);
                ShowJoinError(errorMsg);
            }
        });
    }
#endif

    private void OnReconnectButtonClicked()
    {
        string savedRoom = PlayerPrefs.GetString(LAST_ROOM_KEY, string.Empty);
        if (!string.IsNullOrEmpty(savedRoom))
        {
            HandleRoomJoining(savedRoom);
        }
    }

    private void OnCodeInputEnter(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        {
            OnJoinButtonClicked();
            evt.StopPropagation();
        }
    }

    // Centralized method for all join attempts
    private void HandleRoomJoining(string roomCode)
    {
        if (IsProcessing || string.IsNullOrEmpty(roomCode)) return;

        IsProcessing = true;

        if (_lblError != null) _lblError.text = string.Empty;

        if (QuizNetworkManager.Instance != null)
        {
            QuizNetworkManager.Instance.JoinRoom(roomCode, false, (success, errorMessage) =>
            {
                IsProcessing = false;

                if (!success)
                {
                    if (_lblError != null) _lblError.text = errorMessage;
                    Debug.LogWarning($"Join failed: {errorMessage}");
                }
            });
        }
        else
        {
            IsProcessing = false;
            if (_lblError != null) _lblError.text = "Network Manager not found.";
        }
    }

    public void ShowJoinError(string message)
    {
        SetUIState(true);
        IsProcessing = false;

        if (_lblError != null) _lblError.text = message;
    }

    private void OnBanderaEspClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetLanguage(GameManager.GameLanguage.spanish);
        ApplyLocalization();
        ShowPlayButton();
    }

    private void OnBanderaEngClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SetLanguage(GameManager.GameLanguage.english);
        ApplyLocalization();
        ShowPlayButton();
        Debug.Log("Language set to English" + GameManager.Instance.CurrentLanguage);
    }

    private void ShowPlayButton()
    {
        if (_panelDatos != null) _panelDatos.style.display = DisplayStyle.None;
        if (_playButtonBack != null) _playButtonBack.style.display = DisplayStyle.Flex;

        if (TriviaGameUIController.Instance != null)
            TriviaGameUIController.Instance.ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;

        if (_lblEnterCode != null) _lblEnterCode.text = isEnglish ? "Enter the room code" : "Ingresar el codigo de la sala";
        if (_btnPlay != null) _btnPlay.text = isEnglish ? "PLAY" : "JUGAR";
        if (_joinButton != null) _joinButton.text = isEnglish ? "JOIN" : "UNIRSE";

        // Translate the TextField placeholder
        if (_codeInput != null)
        {
            _codeInput.textEdition.placeholder = isEnglish ? "Code..." : "Codigo...";
        }
    }
}
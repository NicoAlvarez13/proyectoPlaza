using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class MainMenuController_guide : MonoBehaviour
{
    public static MainMenuController_guide Instance { get; private set; }

    [Header("UIDocument")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Join URL")]
    [SerializeField] private string joinRoomBaseUrl = "https://mati7gomez.github.io/pct-build-client/?idsala=";

    [Header("QR Settings")]
    [SerializeField] private int qrSize = 512;

    [Header("Animation Settings")]
    [SerializeField] private float astronautAnimSpeed = 2f; // Duration of one float direction in seconds

    private const string LAST_ROOM_KEY = "LastRoomCode";

    // Containers
    private VisualElement _createRoomContainer;
    private VisualElement _reconnectContainer;
    private VisualElement _leaveWarningContainer;

    // Buttons
    private Button _btnCreateRoom;
    private Button _btnStartGame;
    private Button _btnLeave;
    private Button _btnGoToReconnect;
    private Button _btnGoToCreate;
    private Button _btnAcceptLeave;
    private Button _btnCancelLeave;
    private Button _btnJoin;

    // Fields & Labels
    private Label _lblMessageCreate;
    private Label _lblPlayers;
    private Label _lblErrorCreate;
    private Label _lblErrorReconnect;
    private TextField _inputCode;
    private Image _imgQR;

    // Decoratives

    private Image _astronaut;
    private bool _isAstronautDown = false;

    private bool _waitingResponse = false;
    private Texture2D _qrTexture;

    // Transition Elements
    private bool _transitioned;
    private bool _transitioning = false;
    private int _pendingTransitions = 0;
    private VisualElement _menuesContainer;
    private VisualElement _clouds;
    private Button _btnPlay;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null) return;
        var root = uiDocument.rootVisualElement;

        // 1. Assign Containers
        _createRoomContainer = root.Q<VisualElement>("CreateRoomContainer");
        _reconnectContainer = root.Q<VisualElement>("ReconnectContainer");
        _leaveWarningContainer = root.Q<VisualElement>("LeaveAndDeleteRoomWarningContainer");

        // 2. Assign Buttons
        var createRoomWrapper = root.Q<VisualElement>("btnCreateRoom");
        if (createRoomWrapper != null) _btnCreateRoom = createRoomWrapper.Q<Button>();

        var startGameWrapper = root.Q<VisualElement>("btnStartGame");
        if (startGameWrapper != null) _btnStartGame = startGameWrapper.Q<Button>();

        var leaveWrapper = root.Q<VisualElement>("btnLeaveAndDeleteRoom");
        if (leaveWrapper != null) _btnLeave = leaveWrapper.Q<Button>();

        var reconnectWrapper = root.Q<VisualElement>("btnGoToReconnectContainer");
        if (reconnectWrapper != null) _btnGoToReconnect = reconnectWrapper.Q<Button>();

        var createWrapper = root.Q<VisualElement>("btnGoToCreateRoomContainer");
        if (createWrapper != null) _btnGoToCreate = createWrapper.Q<Button>();

        var acceptWrapper = root.Q<VisualElement>("btnAccept");
        if (acceptWrapper != null) _btnAcceptLeave = acceptWrapper.Q<Button>();

        var cancelWrapper = root.Q<VisualElement>("btnCancel");
        if (cancelWrapper != null) _btnCancelLeave = cancelWrapper.Q<Button>();

        var joinWrapper = root.Q<VisualElement>("JoinButton");
        if (joinWrapper != null) _btnJoin = joinWrapper.Q<Button>();


        //Tranistion Elements
        _btnPlay = root.Q<Button>("PlayButton");
        _clouds = root.Q<VisualElement>("Clouds");
        _menuesContainer = root.Q<VisualElement>("MenuesContainer");

        // 3. Labels, Inputs & Decoratives
        if (_createRoomContainer != null)
        {
            _lblMessageCreate = _createRoomContainer.Q<Label>("lblMessage");
            _lblPlayers = _createRoomContainer.Q<Label>("lblPlayers");
            _lblErrorCreate = _createRoomContainer.Q<Label>("lblErrorMessage");
            _imgQR = _createRoomContainer.Q<Image>("imgQR");
        }

        if (_reconnectContainer != null)
        {
            _lblErrorReconnect = _reconnectContainer.Q<Label>("lblErrorMessage");
            _inputCode = _reconnectContainer.Q<TextField>("CodeInput");
        }

        // Query the astronaut image
        _astronaut = root.Q<Image>("Astronaut");

        SetupInitialState();
        RegisterCallbacks();


        // WAIT for the UI to be fully drawn before starting percentage-based animations
        if (_astronaut != null)
        {
            _astronaut.RegisterCallback<GeometryChangedEvent>(OnGeometryCalculated);
        }
    }

    private void OnDisable()
    {
        UnregisterCallbacks();

        // Critical: Unregister the animation event to prevent memory leaks
        if (_astronaut != null)
        {
            _astronaut.UnregisterCallback<TransitionEndEvent>(OnAstronautTransitionEnd);
        }
    }

    private void OnDestroy()
    {
        if (_qrTexture != null)
        {
            Destroy(_qrTexture);
            _qrTexture = null;
        }
    }

    /// <summary>
    /// Resets the UI to its absolute default state. Used on start and when leaving a room.
    /// </summary>
    public void SetupInitialState()
    {
        SetUIState(true); // Ensure Guide menu is visible
        if (TriviaGameUIController.Instance != null) TriviaGameUIController.Instance.SetUIState(false); // Hide Trivia UI

        if (_createRoomContainer != null) _createRoomContainer.style.display = DisplayStyle.Flex;
        if (_reconnectContainer != null) _reconnectContainer.style.display = DisplayStyle.None;
        if (_leaveWarningContainer != null) _leaveWarningContainer.style.display = DisplayStyle.None;

        bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
        if (_lblMessageCreate != null) _lblMessageCreate.text = isEnglish ? "Generate room code" : "Generar código de sala";

        if (_lblErrorCreate != null) _lblErrorCreate.text = string.Empty;
        if (_lblErrorReconnect != null) _lblErrorReconnect.text = string.Empty;

        if (_lblPlayers != null) _lblPlayers.style.display = DisplayStyle.None;
        if (_imgQR != null)
        {
            _imgQR.style.display = DisplayStyle.None;
            _imgQR.image = null;
        }

        if (_btnCreateRoom != null && _btnCreateRoom.parent != null) _btnCreateRoom.parent.style.display = DisplayStyle.Flex;
        if (_btnLeave != null && _btnLeave.parent != null) _btnLeave.parent.style.display = DisplayStyle.None;
        if (_btnStartGame != null && _btnStartGame.parent != null) _btnStartGame.parent.style.display = DisplayStyle.None;

        if (_btnGoToReconnect != null && _btnGoToReconnect.parent != null)
        {
            _btnGoToReconnect.parent.style.display = PlayerPrefs.HasKey(LAST_ROOM_KEY) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        SetMenuesTransitionToInitialState();
    }

    //---------------------------------*****************---------------------------------//
    #region Menues Transition Logic
    private void OnPlayButtonClicked() => ToggleMenuesTransition();

    //Main methods
    public void ToggleMenuesTransition(bool instant = false, bool toggle = true, bool transitioned = false)
    {
        if (_transitioning) return;

        // Determinar si hay algo que mover antes de tocar estado
        bool willMoveDown = (toggle && !_transitioned) || (!toggle && !_transitioned && transitioned);
        bool willMoveUp = (toggle && _transitioned) || (!toggle && _transitioned && !transitioned);

        if (!willMoveDown && !willMoveUp) return; // nada que hacer, no bloquear

        _transitioning = true;

        // Durations
        SetDuration(_menuesContainer, instant ? 0f : 1.2f);
        SetDuration(_clouds, instant ? 0f : 1.4f);

        // Aplicar translate
        if (willMoveDown)
        {
            SetTranslate(_menuesContainer, 0, 120);
            SetTranslate(_clouds, 0, 0);
            _transitioned = true;
        }
        else
        {
            SetTranslate(_menuesContainer, 0, 0);
            SetTranslate(_clouds, 0, -86);
            _transitioned = false;
        }

        // Resolver fin de transición
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

    // --- Helpers ---

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
    //---------------------------------*****************---------------------------------//

    private void RegisterCallbacks()
    {
        
        if (_btnPlay != null) _btnPlay.clicked += OnPlayButtonClicked;
        if (_btnCreateRoom != null) _btnCreateRoom.clicked += OnCreateRoomClicked;
        if (_btnGoToReconnect != null) _btnGoToReconnect.clicked += () => SwitchContainer(_reconnectContainer);
        if (_btnGoToCreate != null) _btnGoToCreate.clicked += () => SwitchContainer(_createRoomContainer);
        if (_btnJoin != null) _btnJoin.clicked += OnJoinClicked;
        if (_btnAcceptLeave != null) _btnAcceptLeave.clicked += OnAcceptLeaveClicked;
        if (_btnStartGame != null) _btnStartGame.clicked += OnStartGameClicked;



        if (_btnLeave != null)
        {
            _btnLeave.clicked += () => { if (_leaveWarningContainer != null) _leaveWarningContainer.style.display = DisplayStyle.Flex; };
        }

        if (_btnCancelLeave != null)
        {
            _btnCancelLeave.clicked += () => { if (_leaveWarningContainer != null) _leaveWarningContainer.style.display = DisplayStyle.None; };
        }
    }

    private void UnregisterCallbacks()
    {
        if (_btnCreateRoom != null) _btnCreateRoom.clicked -= OnCreateRoomClicked;
        if (_btnJoin != null) _btnJoin.clicked -= OnJoinClicked;
        if (_btnAcceptLeave != null) _btnAcceptLeave.clicked -= OnAcceptLeaveClicked;
        if (_btnStartGame != null) _btnStartGame.clicked -= OnStartGameClicked;
    }

    #region Animation Logic

    /// <summary>
    /// Fired once the UI Toolkit has finished calculating screen sizes.
    /// </summary>
    private void OnGeometryCalculated(GeometryChangedEvent evt)
    {
        // 1. Unregister immediately so this only runs once
        _astronaut.UnregisterCallback<GeometryChangedEvent>(OnGeometryCalculated);

        // 2. Now that the UI has a real size, start the animation
        StartAstronautAnimation();
    }


    /// <summary>
    /// Configures the USS Transition properties and starts the infinite loop.
    /// </summary>
    /// 
    private void StartAstronautAnimation()
    {
        Debug.Log($"astro nashe {_astronaut}");
        if (_astronaut == null) return;

        // 1. Explicitly set the starting position before applying transitions
        _astronaut.style.translate = new Translate(new Length(0, LengthUnit.Percent), new Length(20, LengthUnit.Percent));

        // 2. Configure USS transition settings via C#
        _astronaut.style.transitionDuration = new List<TimeValue> { new TimeValue(astronautAnimSpeed, TimeUnit.Second) };
        _astronaut.style.transitionProperty = new List<StylePropertyName> { new StylePropertyName("translate") };
        _astronaut.style.transitionTimingFunction = new List<EasingFunction> { new EasingFunction(EasingMode.EaseInOutSine) };

        // 3. Listen for when the translate animation finishes to ping-pong it
        _astronaut.RegisterCallback<TransitionEndEvent>(OnAstronautTransitionEnd);

        // 4. Delay the first movement by 50 milliseconds so the UI engine registers the initial state
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
        // Only react if the 'translate' property finished transitioning
        if (!evt.stylePropertyNames.Contains("translate")) return;

        // Ping-pong the direction
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

    private void SwitchContainer(VisualElement target)
    {
        if (_lblErrorCreate != null) _lblErrorCreate.text = string.Empty;
        if (_lblErrorReconnect != null) _lblErrorReconnect.text = string.Empty;

        if (_createRoomContainer != null)
            _createRoomContainer.style.display = (target == _createRoomContainer) ? DisplayStyle.Flex : DisplayStyle.None;

        if (_reconnectContainer != null)
            _reconnectContainer.style.display = (target == _reconnectContainer) ? DisplayStyle.Flex : DisplayStyle.None;

        if (target == _reconnectContainer && _inputCode != null)
        {
            _inputCode.value = PlayerPrefs.GetString(LAST_ROOM_KEY, string.Empty);
        }
    }

    private void OnCreateRoomClicked()
    {
        if (_waitingResponse) return;
        if (_lblErrorCreate != null) _lblErrorCreate.text = string.Empty;

        _waitingResponse = true;

        if (QuizNetworkManager.Instance != null)
        {
            QuizNetworkManager.Instance.CreateRoom((success, errorMessage) =>
            {
                _waitingResponse = false;

                if (success)
                {
                    string roomCode = QuizNetworkManager.Instance.GetSessionName();
                    EnterActiveRoomUI(roomCode);
                }
                else
                {
                    if (_lblErrorCreate != null) _lblErrorCreate.text = errorMessage;
                }
            });
        }
        else
        {
            _waitingResponse = false;
            if (_lblErrorCreate != null) _lblErrorCreate.text = "Network Manager not found.";
        }
    }

    private void OnJoinClicked()
    {
        if (_waitingResponse) return;
        if (_lblErrorReconnect != null) _lblErrorReconnect.text = string.Empty;

        string code = _inputCode != null ? _inputCode.value : string.Empty;
        if (string.IsNullOrEmpty(code)) return;

        _waitingResponse = true;

        if (QuizNetworkManager.Instance != null)
        {
            QuizNetworkManager.Instance.JoinRoom(code, true, (success, errorMessage) =>
            {
                _waitingResponse = false;

                if (success)
                {
                    SwitchContainer(_createRoomContainer);
                    EnterActiveRoomUI(code);

                    // FIX: Instantly grab the true player count because PlayerJoined 
                    // callbacks won't fire for players who are already in the room!
                    if (QuizNetworkManager.Instance._runner != null)
                    {
                        int currentPlayers = Mathf.Max(0, QuizNetworkManager.Instance._runner.ActivePlayers.Count() - 1);
                        UpdatePlayerCount(currentPlayers);
                    }
                }
                else
                {
                    if (_lblErrorReconnect != null) _lblErrorReconnect.text = errorMessage;
                }
            });
        }
        else
        {
            _waitingResponse = false;
            if (_lblErrorReconnect != null) _lblErrorReconnect.text = "Network Manager not found.";
        }
    }

    private void EnterActiveRoomUI(string roomCode)
    {
        bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;

        if (_lblMessageCreate != null) _lblMessageCreate.text = (isEnglish ? "Room: " : "Sala: ") + roomCode;

        if (_lblPlayers != null)
        {
            _lblPlayers.text = isEnglish ? "Players: 0/20" : "Jugadores: 0/20";
            _lblPlayers.style.display = DisplayStyle.Flex;
        }

        if (_btnCreateRoom != null && _btnCreateRoom.parent != null) _btnCreateRoom.parent.style.display = DisplayStyle.None;
        if (_btnGoToReconnect != null && _btnGoToReconnect.parent != null) _btnGoToReconnect.parent.style.display = DisplayStyle.None;

        if (_btnLeave != null && _btnLeave.parent != null) _btnLeave.parent.style.display = DisplayStyle.Flex;
        if (_btnStartGame != null && _btnStartGame.parent != null)
        {
            _btnStartGame.parent.style.display = DisplayStyle.Flex;
            _btnStartGame.SetEnabled(false);
        }

        string roomUrl = $"{joinRoomBaseUrl}{roomCode}";
        _ = LoadAndShowQRFromWeb(roomUrl);
    }

    private void OnAcceptLeaveClicked()
    {
        if (_leaveWarningContainer != null) _leaveWarningContainer.style.display = DisplayStyle.None;

        if (QuizNetworkManager.Instance != null)
        {
            QuizNetworkManager.Instance.LeaveAndDestroyRoom(() =>
            {
                SetupInitialState();
            });
        }
        else
        {
            SetupInitialState();
        }
    }

    public void UpdatePlayerCount(int count)
    {
        if (_lblPlayers != null)
        {
            bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
            _lblPlayers.text = (isEnglish ? "Players: " : "Jugadores: ") + $"{count} / 20";
        }

        if (_btnStartGame != null)
        {
            _btnStartGame.SetEnabled(count > 0);
        }
    }

    private async Task LoadAndShowQRFromWeb(string roomUrl)
    {
        try
        {
            string encodedRoomUrl = UnityWebRequest.EscapeURL(roomUrl);
            string qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size={qrSize}x{qrSize}&data={encodedRoomUrl}";

            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(qrUrl);
            var operation = request.SendWebRequest();

            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (_qrTexture != null) Destroy(_qrTexture);
                _qrTexture = DownloadHandlerTexture.GetContent(request);

                if (_imgQR != null)
                {
                    _imgQR.image = _qrTexture;
                    _imgQR.style.display = DisplayStyle.Flex;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("QR Error: " + ex.Message);
        }
    }
    public void SetUIState(bool isVisible)
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            uiDocument.rootVisualElement.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void OnStartGameClicked() { 
        if (QuizNetworkManager.Instance != null)
        {
            QuizGameManager.Instance.StartMatch(QuestionSO.DifficultyLevel.Easy, 1, QuizGameManager.Instance.AllCategoriesDatabase);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private UIDocument _uiDocument;

    // --- UI Elements ---
    private Button _playButton;
    private Button _joinButton;
    private TextField _codeInput;
    private VisualElement _menuesContainer;
    private VisualElement _clouds;



    private bool _watingResponse = false;

    private void OnEnable()
    {
        Debug.Log(Application.absoluteURL);
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null)
        {
            Debug.LogError("UIDocument component is missing from this GameObject.");
            return;
        }

        VisualElement root = _uiDocument.rootVisualElement;

        // 1. Initialize all elements using the generic helper
        _playButton = QueryElement<Button>(root, "PlayButton");
        _joinButton = QueryElement<Button>(root, "JoinButton");
        _codeInput = QueryElement<TextField>(root, "CodeInput");
        _menuesContainer = QueryElement<VisualElement>(root, "MenuesContainer");
        _clouds = QueryElement<VisualElement>(root, "Clouds");

        // 2. Hook up all interactions
        RegisterEvents();
        if (true) {
            //OnLoadedWithURL();
        }
    }

    private void OnDisable()
    {
        UnregisterEvents();
    }

    // --- Initialization Helpers ---

    /// <summary>
    /// Queries a UI element by name and logs a warning if it cannot be found.
    /// </summary>
    private T QueryElement<T>(VisualElement root, string elementName) where T : VisualElement
    {
        T element = root.Q<T>(elementName);
        if (element == null)
        {
            Debug.LogWarning($"UI Element with ID '{elementName}' was not found.");
        }
        return element;
    }

    // --- Event Management ---

    private void RegisterEvents()
    {
        if (_playButton != null) _playButton.clicked += OnPlayButtonClicked;
        if (_joinButton != null) _joinButton.clicked += OnJoinButtonClicked;

        if (_codeInput != null)
        {
            _codeInput.isDelayed = true;
            _codeInput.RegisterValueChangedCallback(OnCodeInputSubmitted);
            _codeInput.RegisterCallback<KeyDownEvent>(OnCodeInputEnter, TrickleDown.TrickleDown);
        }
    }

    private void UnregisterEvents()
    {
        if (_playButton != null) _playButton.clicked -= OnPlayButtonClicked;
        if (_joinButton != null) _joinButton.clicked -= OnJoinButtonClicked;

        if (_codeInput != null)
        {
            _codeInput.UnregisterValueChangedCallback(OnCodeInputSubmitted);
            _codeInput.UnregisterCallback<KeyDownEvent>(OnCodeInputEnter, TrickleDown.TrickleDown);
        }
    }

    // --- UI Event Handlers ---

    private void OnPlayButtonClicked()
    {
        Debug.Log("Play Button was clicked.");

        if (_menuesContainer != null && _clouds != null)
        {
            _menuesContainer.style.translate = new StyleTranslate(new Translate(0, new Length(120, LengthUnit.Percent), 0));
            _clouds.style.translate = new StyleTranslate(new Translate(0, 0, 0));
        }
    }
    private void OnLoadedWithURL()
    {
        Debug.Log("URL HAS CODE, AUTOMATICALLY REDIRECTING TO CODE MENU");

        if (_menuesContainer != null && _clouds != null)
        {
            _menuesContainer.style.transitionDuration = new List<TimeValue> { new TimeValue(0) };
            _clouds.style.transitionDuration = new List<TimeValue> { new TimeValue(0) };
            _menuesContainer.style.translate = new StyleTranslate(new Translate(0, new Length(120, LengthUnit.Percent), 0));
            _clouds.style.translate = new StyleTranslate(new Translate(0, 0, 0));
        }
    }

    private void OnJoinButtonClicked()
    {
        HandleRoomJoining();
    }

    private void OnCodeInputEnter(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
        {
            Debug.Log($"Physical Enter pressed. Text: {_codeInput.value}");
            HandleRoomJoining();
            evt.StopPropagation();
        }
    }

    private async void OnCodeInputSubmitted(ChangeEvent<string> evt)
    {
        Debug.Log($"Text submitted: {evt.newValue}");
        if (await HandleRoomJoining()) {
            Debug.Log("Joined room");
        }
    }

    private async Task<bool> HandleRoomJoining() {

        if (!_watingResponse)
        {
            Debug.Log("Trying to create a room");
            _watingResponse = true;
            bool result = await QuizNetworkManager.Instance.CreateRoom();
            Debug.Log($"Room creation result: {result}");
            _watingResponse = false;
            return result;
        }

        Debug.Log("Error joining the room, there is already a task trying to enter the room");
        return false;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class QuizNetworkManager : MonoBehaviour
{
    public static QuizNetworkManager Instance { get; private set; }

    [SerializeField] private GameObject _networkRunnerPrefab;

    public NetworkRunner _runner;
    private const string LAST_ROOM_KEY = "LastRoomCode";
    private const string GUIDE_TOKEN_KEY = "GuideToken";

    public bool IsReconnecting { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void CreateRoom(Action<bool, string> onComplete = null)
    {
        StartCoroutine(CreateRoomCoroutine(onComplete));
    }

    private IEnumerator CreateRoomCoroutine(Action<bool, string> onComplete)
    {
        int roomName = UnityEngine.Random.Range(1000, 9999);
        string roomCode = roomName.ToString();

        IsReconnecting = true;
        InitializeRunner();

        if (_runner == null)
        {
            onComplete?.Invoke(false, GetLocalizedError("Failed to initialize network runner.", "Fallo al inicializar la red."));
            yield break;
        }

        var joinTask = _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomCode,
            EnableClientSessionCreation = false
        });

        yield return new WaitUntil(() => joinTask.IsCompleted);

        if (!joinTask.IsFaulted && joinTask.Result.Ok)
        {
            Debug.LogWarning($"Room {roomCode} already exists! Rejecting creation.");

            _runner.Shutdown();
            yield return new WaitUntil(() => _runner == null || _runner.IsShutdown);

            // FIX: Removed accent 
            onComplete?.Invoke(false, GetLocalizedError("The game already exists. Please use the Join button instead.", "La partida ya existe. Por favor, usa el boton de Unirse."));
            yield break;
        }

        if (_runner != null && !_runner.IsShutdown)
        {
            _runner.Shutdown();
            yield return new WaitUntil(() => _runner == null || _runner.IsShutdown);
        }

        IsReconnecting = false;
        InitializeRunner();

        string guideToken = PlayerPrefs.GetString(GUIDE_TOKEN_KEY, string.Empty);

        if (string.IsNullOrEmpty(guideToken))
        {
            guideToken = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(GUIDE_TOKEN_KEY, guideToken);
            PlayerPrefs.Save();
        }

        var sessionProps = new Dictionary<string, SessionProperty> { { "GuideToken", guideToken } };

        var createTask = _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomCode,
            IsOpen = true,
            IsVisible = true,
            PlayerCount = 20,
            SessionProperties = sessionProps
        });

        yield return new WaitUntil(() => createTask.IsCompleted);

        if (createTask.IsFaulted)
        {
            onComplete?.Invoke(false, GetLocalizedError("Failed to communicate with server.", "Fallo al comunicarse con el servidor."));
            yield break;
        }

        var createResult = createTask.Result;

        if (createResult.Ok)
        {
            PlayerPrefs.SetString(LAST_ROOM_KEY, roomCode);
            PlayerPrefs.Save();
            onComplete?.Invoke(true, string.Empty);
        }
        else
        {
            _runner.Shutdown();
            yield return new WaitUntil(() => _runner == null || _runner.IsShutdown);
            onComplete?.Invoke(false, createResult.ShutdownReason.ToString());
        }
    }


    public void JoinRoom(string roomCode, bool asGuide = false, Action<bool, string> onComplete = null)
    {
        StartCoroutine(JoinRoomCoroutine(roomCode, asGuide, onComplete));
    }

    private IEnumerator JoinRoomCoroutine(string roomCode, bool asGuide, Action<bool, string> onComplete)
    {
        IsReconnecting = true;
        InitializeRunner();

        if (_runner == null)
        {
            IsReconnecting = false;
            onComplete?.Invoke(false, GetLocalizedError("Failed to initialize network runner.", "Fallo al inicializar la red."));
            yield break;
        }

        var joinTask = _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomCode,
            EnableClientSessionCreation = false,
        });

        yield return new WaitUntil(() => joinTask.IsCompleted);

        if (MainMenuController.Instance != null)
            MainMenuController.Instance.IsProcessing = false;

        if (joinTask.IsFaulted)
        {
            IsReconnecting = false;
            // FIX: Removed accents
            onComplete?.Invoke(false, GetLocalizedError("Critical network error. Please try again.", "Error critico de red. Por favor, intenta de nuevo."));
            yield break;
        }

        var result = joinTask.Result;

        if (result.Ok)
        {
            if (asGuide)
            {
                yield return new WaitForSeconds(0.5f);

                bool hasCredentials = IsOriginalGuide();
                bool guideAlreadyPresent = false;

                if (QuizGameManager.Instance != null)
                {
                    PlayerRef currentGuide = QuizGameManager.Instance.GuidePlayerRef;

                    if (currentGuide != PlayerRef.None && currentGuide != _runner.LocalPlayer && _runner.ActivePlayers.Contains(currentGuide))
                    {
                        guideAlreadyPresent = true;
                    }
                }

                if (!hasCredentials || guideAlreadyPresent)
                {
                    Debug.LogWarning("Guide validation failed: Missing credentials or Guide is already active in the room.");

                    _runner.Shutdown();
                    yield return new WaitUntil(() => _runner == null || _runner.IsShutdown);
                    _runner = null;

                    IsReconnecting = false;
                    // FIX: Removed accents
                    onComplete?.Invoke(false, GetLocalizedError("Invalid credentials or Guide is already active. Try creating a new room.", "Credenciales invalidas o el Guia ya esta activo. Intenta crear una nueva sala."));
                    yield break;
                }
            }
            else
            {
                PlayerPrefs.SetString(LAST_ROOM_KEY, roomCode);
                PlayerPrefs.Save();
            }

            Debug.Log("Player joined the room successfully");
            IsReconnecting = false;
            onComplete?.Invoke(true, string.Empty);
        }
        else
        {
            string specificError = GetUserFriendlyErrorMessage(result.ShutdownReason);

            _runner.Shutdown();
            yield return new WaitUntil(() => _runner == null || _runner.IsShutdown);
            _runner = null;

            IsReconnecting = false;
            onComplete?.Invoke(false, specificError);
        }
    }

    private string GetUserFriendlyErrorMessage(ShutdownReason reason)
    {
        // FIX: Removed all accents
        switch (reason)
        {
            case ShutdownReason.GameNotFound:
                return GetLocalizedError("Session does not exist. Please check the code.", "La sesion no existe. Revisa el codigo.");
            case ShutdownReason.GameIsFull:
                return GetLocalizedError("The session is currently full.", "La sesion esta llena.");
            case ShutdownReason.GameClosed:
                return GetLocalizedError("The session is closed.", "La sesion esta cerrada.");
            case ShutdownReason.ConnectionTimeout:
                return GetLocalizedError("Connection timed out.", "Tiempo de conexion agotado.");
            case ShutdownReason.ConnectionRefused:
                return GetLocalizedError("Connection was refused by server.", "Conexion rechazada por el servidor.");
            case ShutdownReason.InvalidRegion:
                return GetLocalizedError("Invalid server region.", "Region de servidor invalida.");
            default:
                return GetLocalizedError($"Connection failed: {reason}", $"Fallo de conexion: {reason}");
        }
    }

    public void LeaveAndDestroyRoom(Action onComplete = null)
    {
        StartCoroutine(LeaveAndDestroyRoomCoroutine(onComplete));
    }

    private IEnumerator LeaveAndDestroyRoomCoroutine(Action onComplete)
    {
        if (QuizGameManager.Instance != null && IsOriginalGuide())
        {
            Debug.Log("setting room to inactive");
            QuizGameManager.Instance.IsRoomActive = false;
        }
        else
        {
            Debug.Log("is not original guide");
        }

        yield return new WaitForSeconds(0.5f);

        if (_runner != null && !_runner.IsShutdown)
        {
            Debug.Log("runner shutdown initiated");
            _runner.Shutdown();

            yield return new WaitUntil(() => _runner == null || _runner.IsShutdown);
            Debug.Log("runner shutdown complete");
        }

        onComplete?.Invoke();
    }

    private void InitializeRunner()
    {
        if (_runner != null)
        {
            Destroy(_runner.gameObject);
        }

        GameObject runnerObj = Instantiate(_networkRunnerPrefab);
        runnerObj.transform.SetParent(this.transform);

        _runner = runnerObj.GetComponent<NetworkRunner>();

        NetworkEvents networkEvents = runnerObj.GetComponent<NetworkEvents>();

        if (networkEvents != null)
        {
            networkEvents.OnShutdown.RemoveListener(OnRunnerShutdown);
            networkEvents.OnShutdown.AddListener(OnRunnerShutdown);
        }
        else
        {
            Debug.LogError("NetworkEvents component is missing! Please add it to your NetworkRunner prefab in the Inspector.");
        }
    }

    private void OnRunnerShutdown(NetworkRunner runner, ShutdownReason reason)
    {
        if (IsReconnecting) return;

        Debug.LogWarning($"Runner shut down. Reason: {reason}. Returning to main menu.");

        if (MainMenuController_guide.Instance != null)
        {
            MainMenuController_guide.Instance.SetupInitialState();
        }

        if (MainMenuController.Instance != null)
        {
            MainMenuController.Instance.SetUIState(true);
            // FIX: Removed accent
            MainMenuController.Instance.ShowJoinError(GetLocalizedError("The session was closed.", "La sesion ha sido cerrada."));
        }

        if (TriviaGameUIController.Instance != null)
        {
            TriviaGameUIController.Instance.ResetUI();
        }
    }

    public string GetSessionName()
    {
        return _runner.SessionInfo.Name;
    }

    public bool IsOriginalGuide()
    {
        if (_runner == null || !_runner.IsRunning || !_runner.SessionInfo.IsValid)
        {
            Debug.Log("something went wrong");
            return false;
        }

        if (_runner.SessionInfo.Properties.TryGetValue("GuideToken", out var sessionProperty))
        {
            string sessionGuideToken = sessionProperty;
            string localGuideToken = PlayerPrefs.GetString(GUIDE_TOKEN_KEY, string.Empty);

            return !string.IsNullOrEmpty(localGuideToken) && sessionGuideToken == localGuideToken;
        }
        else
        {
            Debug.Log("GuideToken not found in session properties");

            return false;
        }
    }

    private string GetLocalizedError(string enMsg, string esMsg)
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english)
            return enMsg;
        return esMsg;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public void DevCreateAndPlay(Action<bool, string> onComplete = null)
    {
        StartCoroutine(DevCreateAndPlayCoroutine(onComplete));
    }

    private IEnumerator DevCreateAndPlayCoroutine(Action<bool, string> onComplete)
    {
        bool success = false;
        string error = string.Empty;

        yield return StartCoroutine(CreateRoomCoroutine((ok, err) =>
        {
            success = ok;
            error = err;
        }));

        if (!success)
        {
            onComplete?.Invoke(false, error);
            yield break;
        }

        PlayerPrefs.DeleteKey(GUIDE_TOKEN_KEY);
        PlayerPrefs.Save();

        float timeout = 10f;
        while (QuizGameManager.Instance == null && timeout > 0f)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        if (QuizGameManager.Instance == null)
        {
            onComplete?.Invoke(false, "QuizGameManager no encontrado.");
            yield break;
        }

        var spawner = FindFirstObjectByType<PlayerSpawner>();
        if (spawner != null)
            spawner.DevSpawnLocalPlayer();

        yield return new WaitForSeconds(0.5f);
        QuizGameManager.Instance.DevStartMatch();

        onComplete?.Invoke(true, string.Empty);
    }
#endif

}
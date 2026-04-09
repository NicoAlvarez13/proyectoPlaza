using System; // Required for Action
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq; // Ensure you have this at the top!

public class QuizNetworkManager : MonoBehaviour
{
    public static QuizNetworkManager Instance { get; private set; }

    [SerializeField] private GameObject _networkRunnerPrefab;

    public NetworkRunner _runner;
    private const string LAST_ROOM_KEY = "LastRoomCode";
    private const string GUIDE_TOKEN_KEY = "GuideToken";

    // Track if we are creating or reconnecting to avoid duplicate spawns
    public bool IsReconnecting { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep alive across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Change the signature to include an error message string
    public void CreateRoom(Action<bool, string> onComplete = null)
    {
        StartCoroutine(CreateRoomCoroutine(onComplete));
    }

    private IEnumerator CreateRoomCoroutine(Action<bool, string> onComplete)
    {
        int roomName = UnityEngine.Random.Range(1000, 9999); // Generate a random 4-digit code
        //int roomName = 0; // Harcoded for testing

        string roomCode = roomName.ToString();

        IsReconnecting = true;
        InitializeRunner();

        if (_runner == null)
        {
            onComplete?.Invoke(false, GetLocalizedError("Failed to initialize network runner.", "Fallo al inicializar la red."));
            yield break;
        }

        // 1. SECURITY CHECK: Try to join the room to see if it already exists
        var joinTask = _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomCode,
            EnableClientSessionCreation = false // Strictly attempt to join
        });

        yield return new WaitUntil(() => joinTask.IsCompleted);

        if (!joinTask.IsFaulted && joinTask.Result.Ok)
        {
            // FATAL: The room already exists! We don't want to let them join via the Create button.
            Debug.LogWarning($"Room {roomCode} already exists! Rejecting creation.");

            _runner.Shutdown();
            yield return new WaitUntil(() => _runner == null || _runner.IsShutdown);

            // Pass the error back to the UI
            onComplete?.Invoke(false, GetLocalizedError("The game already exists. Please use the Join button instead.", "La partida ya existe. Por favor, usa el botón de Unirse."));
            yield break;
        }

        // 2. Joining failed, which means the room is safe to create.
        if (_runner != null && !_runner.IsShutdown)
        {
            _runner.Shutdown();
            yield return new WaitUntil(() => _runner == null || _runner.IsShutdown);
        }

        IsReconnecting = false; // Flag as a new creation
        InitializeRunner();     // Re-initialize for the creation process

        string guideToken = PlayerPrefs.GetString(GUIDE_TOKEN_KEY, string.Empty);

        if (string.IsNullOrEmpty(guideToken))
        {
            guideToken = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(GUIDE_TOKEN_KEY, guideToken);
            PlayerPrefs.Save();
        }
        

        var sessionProps = new Dictionary<string, SessionProperty> { { "GuideToken", guideToken } };

        // 3. Create the room
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
            onComplete?.Invoke(true, string.Empty); // Success! No error message.
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

        // Catch low-level task failures
        if (joinTask.IsFaulted)
        {
            IsReconnecting = false;
            onComplete?.Invoke(false, GetLocalizedError("Critical network error. Please try again.", "Error crítico de red. Por favor, intenta de nuevo."));
            yield break;
        }

        var result = joinTask.Result;

        if (result.Ok)
        {
            // --- SECURITY VALIDATION FOR GUIDES ---
            if (asGuide)
            {
                // Wait briefly for network objects to spawn and sync locally
                yield return new WaitForSeconds(0.5f);

                bool hasCredentials = IsOriginalGuide();
                bool guideAlreadyPresent = false;

                if (QuizGameManager.Instance != null)
                {
                    PlayerRef currentGuide = QuizGameManager.Instance.GuidePlayerRef;

                    // Check if the Guide ref is NOT None, not us, and is currently an active player
                    if (currentGuide != PlayerRef.None && currentGuide != _runner.LocalPlayer && _runner.ActivePlayers.Contains(currentGuide))
                    {
                        guideAlreadyPresent = true;
                    }
                }

                if (!hasCredentials || guideAlreadyPresent)
                {
                    Debug.LogWarning("Guide validation failed: Missing credentials or Guide is already active in the room.");

                    // Kick the imposter out
                    _runner.Shutdown();
                    yield return new WaitUntil(() => _runner == null || _runner.IsShutdown);
                    _runner = null;

                    IsReconnecting = false;
                    onComplete?.Invoke(false, GetLocalizedError("Invalid credentials or Guide is already active. Try creating a new room.", "Credenciales inválidas o el Guía ya está activo. Intenta crear una nueva sala."));
                    yield break;
                }
            }
            else
            {
                // Save the room code for normal players so the Reconnect button works
                PlayerPrefs.SetString(LAST_ROOM_KEY, roomCode);
                PlayerPrefs.Save();
            }

            Debug.Log("Player joined the room successfully");
            IsReconnecting = false;
            onComplete?.Invoke(true, string.Empty); // Success
        }
        else
        {
            // SPECIFIC ERROR HANDLING: Translate the shutdown reason for the UI
            string specificError = GetUserFriendlyErrorMessage(result.ShutdownReason);

            _runner.Shutdown();
            yield return new WaitUntil(() => _runner == null || _runner.IsShutdown);
            _runner = null;

            IsReconnecting = false;
            onComplete?.Invoke(false, specificError);
        }
    }

    // Helper method to translate Fusion shutdown reasons into user-friendly English messages
    private string GetUserFriendlyErrorMessage(ShutdownReason reason)
    {
        switch (reason)
        {
            case ShutdownReason.GameNotFound:
                return GetLocalizedError("Session does not exist. Please check the code.", "La sesión no existe. Revisa el código.");
            case ShutdownReason.GameIsFull:
                return GetLocalizedError("The session is currently full.", "La sesión está llena.");
            case ShutdownReason.GameClosed:
                return GetLocalizedError("The session is closed.", "La sesión está cerrada.");
            case ShutdownReason.ConnectionTimeout:
                return GetLocalizedError("Connection timed out.", "Tiempo de conexión agotado.");
            case ShutdownReason.ConnectionRefused:
                return GetLocalizedError("Connection was refused by server.", "Conexión rechazada por el servidor.");
            case ShutdownReason.InvalidRegion:
                return GetLocalizedError("Invalid server region.", "Región de servidor inválida.");
            default:
                return GetLocalizedError($"Connection failed: {reason}", $"Fallo de conexión: {reason}");
        }
    }

    public void LeaveAndDestroyRoom(Action onComplete = null)
    {
        StartCoroutine(LeaveAndDestroyRoomCoroutine(onComplete));
    }

    private IEnumerator LeaveAndDestroyRoomCoroutine(Action onComplete)
    {
        // 1. Trigger the global shutdown signal if I am the guide
        if (QuizGameManager.Instance != null && IsOriginalGuide())
        {
            Debug.Log("setting room to inactive");
            QuizGameManager.Instance.IsRoomActive = false;
        }
        else
        {
            Debug.Log("is not original guide");
        }

        // 2. Wait a short moment to allow the shutdown signal to propagate over the network
        yield return new WaitForSeconds(0.5f);

        // 3. Shutdown the runner to leave the room safely
        if (_runner != null && !_runner.IsShutdown)
        {
            Debug.Log("runner shutdown initiated");
            _runner.Shutdown();
            

            // Wait until Fusion has completely cleaned up the network session
            yield return new WaitUntil(() => _runner == null || _runner.IsShutdown);
            Debug.Log("runner shutdown complete");
        }

        // 4. Notify the UI that it is safe to reset
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

        // --- Event-Driven Disconnection Handling ---
        // Grab the component directly from the prefab
        NetworkEvents networkEvents = runnerObj.GetComponent<NetworkEvents>();

        if (networkEvents != null)
        {
            // Remove the listener first to avoid duplicate fires if the runner is re-initialized
            networkEvents.OnShutdown.RemoveListener(OnRunnerShutdown);
            networkEvents.OnShutdown.AddListener(OnRunnerShutdown);
        }
        else
        {
            // Fallback warning just in case the component gets accidentally removed from the prefab later
            Debug.LogError("NetworkEvents component is missing! Please add it to your NetworkRunner prefab in the Inspector.");
        }
    }

    // Replace your current OnRunnerShutdown method with this updated version:
    // Replace your current OnRunnerShutdown method with this:
    private void OnRunnerShutdown(NetworkRunner runner, ShutdownReason reason)
    {
        // Ignore expected shutdowns when cycling the runner for creation/joining
        if (IsReconnecting) return;

        Debug.LogWarning($"Runner shut down. Reason: {reason}. Returning to main menu.");

        // Reset the Guide UI
        if (MainMenuController_guide.Instance != null)
        {
            MainMenuController_guide.Instance.SetupInitialState();
        }

        // Return normal players to the main menu
        if (MainMenuController.Instance != null)
        {
            MainMenuController.Instance.SetUIState(true);
            MainMenuController.Instance.ShowJoinError(GetLocalizedError("The session was closed.", "La sesión ha sido cerrada."));
        }

        // FIX: Completely wipe the Trivia UI so no old cards or screens carry over to the next room!
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

            //Debug.Log($"Session Guide Token: {sessionGuideToken}, Local Guide Token: {localGuideToken}");

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

}
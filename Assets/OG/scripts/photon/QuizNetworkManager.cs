using UnityEngine;
using Fusion;
using System.Threading.Tasks;
using Unity.VisualScripting;

public class QuizNetworkManager : MonoBehaviour
{
    // Singleton instance
    public static QuizNetworkManager Instance { get; private set; }

    [SerializeField] private GameObject _networkRunnerPrefab;

    private NetworkRunner _runner;

    private void Awake()
    {
        // Singleton pattern enforcement
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

    public async Task<bool> CreateRoom() {

        InitializeRunner();

        int roomName = Random.Range(1000, 9999);
        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomName.ToString(),
            IsOpen = true,
            IsVisible = true,
            PlayerCount = 20,
        });

        Debug.Log(result.Ok
            ? $"Room {_runner.SessionInfo.Name} created"
            : $"Couldn't create the room: {result.ShutdownReason}");
        return result.Ok;
    }

    public async Task<bool> JoinRoom(string roomName) {

        InitializeRunner();

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = roomName,
            EnableClientSessionCreation = false, 
        });

        if (result.Ok) Debug.Log("Player joined the room");
        else Debug.Log($"Player couldnt join the room: {result.ShutdownReason}");
        return result.Ok;
    }


    private void InitializeRunner()
    {
        // 1. If we already have a runner, destroy its entire GameObject
        if (_runner != null)
        {
            Destroy(_runner.gameObject);
        }

        GameObject runnerObj = Instantiate(_networkRunnerPrefab);
        runnerObj.transform.SetParent(this.transform);

        _runner = runnerObj.GetComponent<NetworkRunner>();
    }
}

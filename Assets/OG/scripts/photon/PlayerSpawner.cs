using Fusion;
using System;
using System.Linq;
using UnityEngine;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public NetworkPrefabRef PlayerPrefab;
    public NetworkPrefabRef QuizGameManagerPrefab;

    private const string USER_GUID_KEY = "UserGUID";

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            if (QuizNetworkManager.Instance.IsOriginalGuide())
            {
                if (!QuizNetworkManager.Instance.IsReconnecting)
                {
                    Runner.Spawn(QuizGameManagerPrefab, Vector3.zero, Quaternion.identity, player);
                }
                else
                {
                    Debug.Log("Guide is reconnecting. Waiting for existing QuizGameManager to sync...");
                }
            }
            else
            {
                ResumeOrSpawnPlayer();
            }
        }

        NotifyUI();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>Modo dev: nos spawnea como jugador aunque seamos el host.</summary>
    public void DevSpawnLocalPlayer() => StartCoroutine(ResumeOrSpawnPlayerRoutine());
#endif

    private void ResumeOrSpawnPlayer()
    {
        StartCoroutine(ResumeOrSpawnPlayerRoutine());
    }

    private System.Collections.IEnumerator ResumeOrSpawnPlayerRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (Runner == null || !Runner.IsRunning) yield break;

        string myGuid = PlayerPrefs.GetString(USER_GUID_KEY, string.Empty);
        if (string.IsNullOrEmpty(myGuid))
        {
            myGuid = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(USER_GUID_KEY, myGuid);
            PlayerPrefs.Save();
        }

        PlayerNetworkData existingPlayer = null;
        var allPlayers = FindObjectsByType<PlayerNetworkData>(FindObjectsSortMode.None);

        foreach (var playerData in allPlayers)
        {
            if (playerData.UserGUID.ToString() == myGuid)
            {
                existingPlayer = playerData;
                break;
            }
        }

        if (existingPlayer != null)
        {
            PlayerRef currentOwner = existingPlayer.Object.StateAuthority;
            PlayerRef guideRef = PlayerRef.None;

            if (QuizGameManager.Instance != null)
            {
                guideRef = QuizGameManager.Instance.GuidePlayerRef;
            }

            if (currentOwner != guideRef && currentOwner != Runner.LocalPlayer && Runner.ActivePlayers.Contains(currentOwner))
            {
                Debug.LogWarning("DUPLICATE DEVICE DETECTED: This device is already playing in another tab! Disconnecting clone...");
                ExecuteLocalShutdown();

                if (MainMenuController.Instance != null)
                {
                    bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
                    string errorMsg = isEnglish ? "You are already playing in another tab. Please close this one." : "Ya est�s jugando en otra pesta�a. Por favor, cierra esta.";
                    MainMenuController.Instance.ShowJoinError(errorMsg);
                }

                yield break;
            }

            Debug.Log($"Found existing player data for GUID {myGuid}. Requesting state authority...");
            existingPlayer.Object.RequestStateAuthority();
        }
        else
        {
            if (!QuizGameManager.Instance.GameStarted)
            {
                Debug.Log("No existing data found. Spawning new player object...");
                var obj = Runner.Spawn(PlayerPrefab, Vector3.zero, Quaternion.identity, Runner.LocalPlayer);

                var networkData = obj.GetComponent<PlayerNetworkData>();
                if (networkData != null)
                {
                    networkData.UserGUID = myGuid;
                }
            }
            else
            {
                Debug.LogWarning("Game already started. Cannot join to play. Try entering another room");
            }
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} left the session.");

        if (QuizGameManager.Instance == null) return;

        // FIX: Check if the QuizGameManager has been orphaned (StateAuthority is None).
        // Fusion clears the authority BEFORE this callback runs, so we check for None.
        if (QuizGameManager.Instance.Object.StateAuthority == PlayerRef.None)
        {
            if (!QuizGameManager.Instance.IsRoomActive)
            {
                // The Guide intentionally closed the room. 
                // We don't need to do anything here because QuizGameManager.OnStatusChanged() 
                // will automatically catch the IsRoomActive = false and shut down the Runner.
                Debug.Log("The room was closed. Waiting for automatic shutdown...");
                ExecuteLocalShutdown();
            }
            else
            {
                // The host crashed or disconnected by accident!
                Debug.LogWarning("The manager lost its State Authority! Claiming authority to keep match alive.");

                // All remaining clients will ask for authority. Fusion safely gives it to just one.
                QuizGameManager.Instance.Object.RequestStateAuthority();
            }
        }
        else
        {
            // If the manager still has an authority, it was just a normal player who left.
            NotifyUI();
        }
    }

    private async void ExecuteLocalShutdown()
    {
        if (Runner != null && Runner.IsRunning)
        {
            Debug.Log("executing local shutdown");
            await Runner.Shutdown();
        }
    }

    private void NotifyUI()
    {
        if (QuizNetworkManager.Instance.IsOriginalGuide() && MainMenuController_guide.Instance != null)
        {
            int count = Runner.ActivePlayers.Count() - 1;
            MainMenuController_guide.Instance.UpdatePlayerCount(Mathf.Max(0, count));
        }
    }
}
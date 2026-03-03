using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [Tooltip("The player prefab. Must have a NetworkObject and PlayerNetworkData component.")]
    public NetworkPrefabRef PlayerPrefab;

    public NetworkPrefabRef QuizGameManager;


    public void PlayerJoined(PlayerRef player)
    {
        // Only spawn the prefab for the local player running this specific instance
        if (player == Runner.LocalPlayer && !Runner.IsSharedModeMasterClient)
        {
            Runner.Spawn(PlayerPrefab, Vector3.zero, Quaternion.identity, player);
            Debug.Log("Spawned local player prefab.");
        }

        if (player == Runner.LocalPlayer && Runner.IsSharedModeMasterClient) {
            Runner.Spawn(QuizGameManager, Vector3.zero, Quaternion.identity, player);
        }

    }
}

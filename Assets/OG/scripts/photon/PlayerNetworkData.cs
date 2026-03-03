using UnityEngine;
using Fusion;

public class PlayerNetworkData : NetworkBehaviour
{
    // Networked variables synchronize automatically across all clients
    [Networked] public int Score { get; set; }
    [Networked] public int CurrentAnswerIndex { get; set; }
    [Networked] public NetworkString<_32> PlayerName { get; set; }
    [Networked, OnChangedRender(nameof(OnSpriteChanged))] public byte Sprite { get; set; }

    [SerializeField] private Sprite[] _spritesList;

    [SerializeField] private Sprite _playerSprite;


    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            // Initialize default values when the player spawns
            Score = 0;
            CurrentAnswerIndex = -1; // -1 means no answer selected yet
            PlayerName = "Visitor_" + Random.Range(1000, 9999);
        }
    }

    // Called by the player's UI buttons on their device
    public void SubmitAnswer(int answerIndex)
    {
        if (HasStateAuthority)
        {
            CurrentAnswerIndex = answerIndex;
            Debug.Log($"Answer submitted: {answerIndex}");
        }
    }

    public void ChangeSprite(byte iconNumber) {

        if (HasStateAuthority) { 
            Sprite = iconNumber;
        }
    }
    private void OnSpriteChanged()
    {
        _playerSprite = _spritesList[Sprite];
    }

    // Called by the Main Display when evaluating the round
    public void AddScore(int points)
    {
        // In Shared Mode, anyone can call RPCs on anyone. 
        // The Main Display can tell the player to update their score.
        RPC_UpdateScore(points);
    }



    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_UpdateScore(int points)
    {
        Score += points;
    }
}

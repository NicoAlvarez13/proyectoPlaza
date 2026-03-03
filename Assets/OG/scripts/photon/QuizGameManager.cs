using UnityEngine;
using Fusion;

public class QuizGameManager : NetworkBehaviour
{
    public static QuizGameManager Instance { get; private set; }

    [Networked] public int CurrentQuestionIndex { get; set; }
    [Networked] public float RoundTimer { get; set; }

    // Using a networked boolean to track if a question is currently active
    [Networked] public NetworkBool IsQuestionActive { get; set; }

    public override void Spawned()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Runner.Despawn(Object);
            return;
        }

        if (HasStateAuthority)
        {
            CurrentQuestionIndex = 0;
            IsQuestionActive = false;
        }
    }

    public override void FixedUpdateNetwork()
    {
        // Only the Main Display (which has State Authority over this object) ticks the timer
        if (HasStateAuthority && IsQuestionActive)
        {
            RoundTimer -= Runner.DeltaTime;

            if (RoundTimer <= 0)
            {
                EndRound();
            }
        }
    }

    // Called by the Main Display to start the next question
    public void StartNextQuestion(float timeLimit)
    {
        if (HasStateAuthority)
        {
            CurrentQuestionIndex++;
            RoundTimer = timeLimit;
            IsQuestionActive = true;
        }
    }

    private void EndRound()
    {
        IsQuestionActive = false;
        RoundTimer = 0;
        // Logic to evaluate PlayerNetworkData answers goes here
        Debug.Log("Round Ended. Evaluating answers...");
    }
}
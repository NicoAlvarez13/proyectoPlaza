using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System.Linq;

public class QuizGameManager : NetworkBehaviour, IStateAuthorityChanged
{
    public static QuizGameManager Instance { get; private set; }

    public enum GameState
    {
        Lobby,
        StartingMatch,
        QuestionPhase,
        ResultPhase,
        MatchEnded
    }

    [Header("Match Timers (Editable on Prefab)")]
    [SerializeField] private float _timeToStart = 5f;
    [SerializeField] private float _timeForQuestion = 15f;
    [SerializeField] private float _timeForResult = 3f;
    [SerializeField] private float _timeForEndgame = 60f;

    [Header("Scoring")]
    [SerializeField] private float _scoreMultiplier = 100f;

    [Networked] public float TimeToStart { get; set; }
    [Networked] public float TimeForQuestion { get; set; }
    [Networked] public float TimeForResult { get; set; }
    [Networked] public float TimeForEndgame { get; set; }
    [Networked] public float ScoreMultiplier { get; set; }

    [Networked, OnChangedRender(nameof(OnStatusChanged))]
    public NetworkBool IsRoomActive { get; set; } = true;

    [Networked] public bool GameStarted { get; set; }

    [Networked, OnChangedRender(nameof(OnGameStateChanged))]
    public GameState CurrentState { get; set; }

    [Networked] public float StateTimer { get; set; }
    [Networked] public int CurrentQuestionIndex { get; set; }
    [Networked] public PlayerRef GuidePlayerRef { get; set; }
    [Networked] public int TotalQuestions { get; set; }

    [Header("Game Database")]
    public List<CategorySO> AllCategoriesDatabase = new List<CategorySO>();

    [Networked, Capacity(20)]
    public NetworkArray<NetworkString<_64>> SelectedQuestionIDs { get; }

    public override void Spawned()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this)
        {
            Runner.Despawn(Object);
            return;
        }

        if (QuizNetworkManager.Instance.IsOriginalGuide())
        {
            bool guideIsAlive = GuidePlayerRef != PlayerRef.None && GuidePlayerRef != Runner.LocalPlayer && Runner.ActivePlayers.Contains(GuidePlayerRef);

            if (!HasStateAuthority && !guideIsAlive)
            {
                Object.RequestStateAuthority();
            }
            else if (HasStateAuthority && GuidePlayerRef == PlayerRef.None)
            {
                TimeToStart = _timeToStart;
                TimeForQuestion = _timeForQuestion;
                TimeForResult = _timeForResult;
                TimeForEndgame = _timeForEndgame;
                ScoreMultiplier = _scoreMultiplier;

                CurrentState = GameState.Lobby;
                CurrentQuestionIndex = 0;
                GuidePlayerRef = Runner.LocalPlayer;
            }

            if (GameStarted) SyncLocalUI();
        }

        bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
        TriviaGameUIController.Instance.UpdateRoomName(Runner.SessionInfo.Name);
        TriviaGameUIController.Instance.UpdateWaitingPlayersText(isEnglish ? "WAITING PLAYERS..." : "ESPERANDO JUGADORES...");

    }

    public void StartMatch(QuestionSO.DifficultyLevel selectedDifficulty, int questionsPerCategory, List<CategorySO> selectedCategories)
    {
        if (!HasStateAuthority || CurrentState != GameState.Lobby || Runner.LocalPlayer != GuidePlayerRef) return;

        List<string> matchQuestionIDs = new List<string>();

        foreach (CategorySO category in selectedCategories)
        {
            if (category == null) continue;

            List<QuestionSO> pickedQuestions = category.GetRandomQuestions(selectedDifficulty, questionsPerCategory);
            foreach (QuestionSO q in pickedQuestions)
            {
                if (q != null && !matchQuestionIDs.Contains(q.QuestionID))
                {
                    matchQuestionIDs.Add(q.QuestionID);
                }
            }
        }

        System.Random rng = new System.Random();
        matchQuestionIDs = matchQuestionIDs.OrderBy(x => rng.Next()).ToList();

        if (matchQuestionIDs.Count > 20)
        {
            matchQuestionIDs = matchQuestionIDs.Take(20).ToList();
        }

        TotalQuestions = matchQuestionIDs.Count;

        for (int i = 0; i < TotalQuestions; i++)
        {
            SelectedQuestionIDs.Set(i, matchQuestionIDs[i]);
        }

        CurrentQuestionIndex = 0;
        GameStarted = true;
        ChangeState(GameState.StartingMatch, TimeToStart);
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// Modo dev: arranca la partida saltando la validación de guide.
    /// Usa todas las categorías disponibles, dificultad fácil, 2 preguntas por categoría.
    /// </summary>
    public void DevStartMatch()
    {
        if (!HasStateAuthority || CurrentState != GameState.Lobby) return;

        var allCategories = AllCategoriesDatabase.Where(c => c != null).ToList();
        StartMatch(QuestionSO.DifficultyLevel.Easy, 2, allCategories);
    }
#endif

    public QuestionSO GetQuestionByID(string id)
    {
        foreach (var category in AllCategoriesDatabase)
        {
            foreach (var question in category.Questions)
            {
                if (question != null && question.QuestionID == id) return question;
            }
        }
        return null;
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority && CurrentState != GameState.Lobby)
        {
            // NEW: Early skip validation!
            if (CurrentState == GameState.QuestionPhase && StateTimer > 0)
            {
                if (CheckIfAllPlayersAnswered())
                {
                    StateTimer = 0; // Force immediate transition
                    HandleStateTransition(); // TRIGGER TRANSITION IMMEDIATELY
                    return; // Exit out of the loop for this frame
                }
            }

            if (StateTimer > 0)
            {
                StateTimer -= Runner.DeltaTime;
                if (StateTimer <= 0)
                {
                    StateTimer = 0;
                    HandleStateTransition();
                }
            }
        }
    }

    // NEW: Method to scan the room and verify if everyone is ready
    private bool CheckIfAllPlayersAnswered()
    {
        var players = FindObjectsByType<PlayerNetworkData>(FindObjectsSortMode.None);
        int validPlayersCount = 0;
        int answeredPlayersCount = 0;

        foreach (var p in players)
        {
            validPlayersCount++;

            // NUEVO: Comprobamos el bool Y que el �ndice de su respuesta coincida con el �ndice de la pregunta actual
            if (p.HasAnsweredCurrentQuestion && p.CurrentAnswerIndex == CurrentQuestionIndex)
            {
                answeredPlayersCount++;
            }
        }

        // Only fast-forward if there is at least 1 valid player, and ALL valid players have answered
        return validPlayersCount > 0 && validPlayersCount == answeredPlayersCount;
    }

    private void HandleStateTransition()
    {
        switch (CurrentState)
        {
            case GameState.StartingMatch:
                ChangeState(GameState.QuestionPhase, TimeForQuestion);
                break;
            case GameState.QuestionPhase:
                ChangeState(GameState.ResultPhase, TimeForResult);
                break;
            case GameState.ResultPhase:
                CurrentQuestionIndex++;
                if (CurrentQuestionIndex >= TotalQuestions)
                    ChangeState(GameState.MatchEnded, TimeForEndgame);
                else
                    ChangeState(GameState.QuestionPhase, TimeForQuestion);
                break;
            case GameState.MatchEnded:
                IsRoomActive = false;
                Runner.Shutdown();
                break;
        }
    }

    private void ChangeState(GameState newState, float timeLimit)
    {
        CurrentState = newState;
        StateTimer = timeLimit;
    }

    public void SyncLocalUI()
    {
        OnGameStateChanged();
    }

    private void OnGameStateChanged()
    {
        bool isGuide = QuizNetworkManager.Instance.IsOriginalGuide();

        // ── Audio (solo jugadores) ───────────────────────────────────────────
        if (!isGuide)
        {
            switch (CurrentState)
            {
                case GameState.QuestionPhase:
                    AudioManager.Instance?.StopMusic();
                    AudioManager.Instance?.PlayTimerSFX();
                    break;
                case GameState.ResultPhase:
                    AudioManager.Instance?.StopTimerSFX();
                    break;
                case GameState.MatchEnded:
                    AudioManager.Instance?.StopMusic();
                    break;
            }
        }
        // ─────────────────────────────────────────────────────────────────────
        bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;

        if (CurrentState != GameState.Lobby)
        {
            if (TriviaGameUIController.Instance != null) TriviaGameUIController.Instance.ForceHideAvatarSelection();
        }

        if (isGuide)
        {
            switch (CurrentState)
            {
                case GameState.StartingMatch:
                case GameState.QuestionPhase:
                    if (MainMenuController_guide.Instance != null) MainMenuController_guide.Instance.SetUIState(false);
                    TriviaGameUIController.Instance.SetUIState(true);
                    TriviaGameUIController.Instance.ShowGuideLiveLeaderboard();
                    break;
                case GameState.ResultPhase:
                    TriviaGameUIController.Instance.SortAndShowLeaderboard(false);
                    break;
                case GameState.MatchEnded:
                    TriviaGameUIController.Instance.SortAndShowLeaderboard(true, isGuide: true);
                    break;
            }
        }
        else
        {
            switch (CurrentState)
            {
                case GameState.StartingMatch:
                    int seconds = Mathf.CeilToInt(StateTimer);
                    string startingText = isEnglish ? "STARTING MATCH IN:" : "EMPEZANDO PARTIDA EN:";
                    TriviaGameUIController.Instance.ShowNextQuestionScreen(startingText, seconds > 0 ? seconds.ToString() : "0");
                    break;
                case GameState.QuestionPhase:
                    string qID = SelectedQuestionIDs.Get(CurrentQuestionIndex).ToString();
                    QuestionSO q = GetQuestionByID(qID);
                    if (q != null) TriviaGameUIController.Instance.SetupCurrentQuestionUI(q);
                    break;
                case GameState.ResultPhase:
                    string qIDRes = SelectedQuestionIDs.Get(CurrentQuestionIndex).ToString();
                    QuestionSO qRes = GetQuestionByID(qIDRes);
                    if (qRes != null) TriviaGameUIController.Instance.SetupCurrentQuestionUI(qRes);

                    TriviaGameUIController.Instance.ShowResultsUI();
                    break;
                case GameState.MatchEnded:
                    TriviaGameUIController.Instance.SortAndShowLeaderboard(true);
                    break;
            }
        }
    }

    private void OnStatusChanged()
    {
        if (!IsRoomActive)
        {
            if (Runner != null && Runner.IsRunning)
            {
                // Ensure room cleanup triggers locally
            }
        }
    }

    public void StateAuthorityChanged()
    {
        if (HasStateAuthority)
        {
            if (QuizNetworkManager.Instance.IsOriginalGuide())
            {
                GuidePlayerRef = Runner.LocalPlayer;

                if (GameStarted)
                {
                    SyncLocalUI();
                }
            }
        }
    }

    private void Update()
    {
        if (IsRoomActive && GameStarted && CurrentState != GameState.Lobby && CurrentState != GameState.MatchEnded)
        {
            if (CurrentState == GameState.StartingMatch)
            {
                int secondsRemaining = Mathf.CeilToInt(StateTimer);
                TriviaGameUIController.Instance.UpdateNextQuestionTime(secondsRemaining > 0 ? secondsRemaining.ToString() : "0");
            }
            else if (CurrentState == GameState.QuestionPhase)
            {
                if (StateTimer <= 0)
                {
                    TriviaGameUIController.Instance.SetTimerVisual(0f);
                    TriviaGameUIController.Instance.SetTimerValue("0.00");
                }
                else
                {
                    float fillPercentage = StateTimer / TimeForQuestion;
                    TriviaGameUIController.Instance.SetTimerVisual(fillPercentage);
                    TriviaGameUIController.Instance.SetTimerValue(StateTimer.ToString("0.00"));
                }
            }
        }
    }
}
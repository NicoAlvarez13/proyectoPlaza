using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerNetworkData : NetworkBehaviour, IStateAuthorityChanged
{
    [Networked] public NetworkString<_64> UserGUID { get; set; }
    [Networked, OnChangedRender(nameof(OnDataChanged))] public int Score { get; set; }
    [Networked] public int CurrentAnswerIndex { get; set; }
    [Networked, OnChangedRender(nameof(OnDataChanged))] public NetworkString<_32> PlayerName { get; set; }
    [Networked, OnChangedRender(nameof(OnDataChanged))] public byte SpriteIndex { get; set; }
    [Networked] public NetworkBool HasCompletedSetup { get; set; }

    // NEW: Networked flag so the Manager knows when this specific player is ready
    [Networked] public NetworkBool HasAnsweredCurrentQuestion { get; set; }

    [SerializeField] private Sprite[] _spritesList;
    public VisualElement UICard { get; private set; }

    private string _mySubmittedAnswer = "";
    private float _mySubmittedTime = 0f;
    private QuizGameManager.GameState _lastKnownState;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            if (MainMenuController.Instance != null) MainMenuController.Instance.SetUIState(false);
            TriviaGameUIController.Instance.SetUIState(true);

            bool isGameRunning = QuizGameManager.Instance != null && QuizGameManager.Instance.CurrentState != QuizGameManager.GameState.Lobby;

            if (HasCompletedSetup || isGameRunning)
            {
                if (isGameRunning && !HasCompletedSetup)
                {
                    if (string.IsNullOrEmpty(PlayerName.ToString()))
                    {
                        PlayerName = "Visitor_" + UnityEngine.Random.Range(1000, 9999);
                        SpriteIndex = (byte)UnityEngine.Random.Range(0, _spritesList.Length);
                    }
                    HasCompletedSetup = true;
                }

                TriviaGameUIController.Instance.ForceHideAvatarSelection();

                if (QuizGameManager.Instance != null && QuizGameManager.Instance.GameStarted)
                {
                    QuizGameManager.Instance.SyncLocalUI();
                }
                else
                {
                    TriviaGameUIController.Instance.SkipAvatarSelection();
                }
            }
            else
            {
                InitializeNewPlayer();
            }

            if (TriviaGameUIController.Instance != null)
            {
                TriviaGameUIController.Instance.OnLocalPlayerAnswered -= HandleLocalAnswerSubmitted;
                TriviaGameUIController.Instance.OnLocalPlayerAnswered += HandleLocalAnswerSubmitted;
            }

            if (QuizGameManager.Instance != null)
            {
                _lastKnownState = QuizGameManager.Instance.CurrentState;
            }
        }
        SetupPlayerCard();
    }

    private void InitializeNewPlayer()
    {
        Score = 0;
        CurrentAnswerIndex = -1;

        if (string.IsNullOrEmpty(PlayerName.ToString()))
        {
            // Set name depending on language
            bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
            string prefix = isEnglish ? "Visitor_" : "Visitante_";

            PlayerName = prefix + UnityEngine.Random.Range(1000, 9999);
            SpriteIndex = (byte)UnityEngine.Random.Range(0, _spritesList.Length);
        }

        TriviaGameUIController.Instance.SetupAvatarSelection(_spritesList, SpriteIndex, PlayerName.ToString());
        TriviaGameUIController.Instance.OnAvatarConfirmed += HandleAvatarConfirmed;
    }

    private void SetupPlayerCard()
    {
        Sprite mySprite = _spritesList[Mathf.Clamp(SpriteIndex, 0, _spritesList.Length - 1)];
        UICard = TriviaGameUIController.Instance.AddPlayerCard(PlayerName.ToString(), Score.ToString(), mySprite);
        UpdateUICardVisuals();
    }

    public void StateAuthorityChanged()
    {
        if (HasStateAuthority)
        {
            if (MainMenuController.Instance != null) MainMenuController.Instance.SetUIState(false);
            TriviaGameUIController.Instance.SetUIState(true);

            bool isGameRunning = QuizGameManager.Instance != null && QuizGameManager.Instance.CurrentState != QuizGameManager.GameState.Lobby;

            if (HasCompletedSetup || isGameRunning)
            {
                if (isGameRunning && !HasCompletedSetup)
                {
                    HasCompletedSetup = true;
                }

                TriviaGameUIController.Instance.ForceHideAvatarSelection();

                if (QuizGameManager.Instance != null && QuizGameManager.Instance.GameStarted)
                {
                    QuizGameManager.Instance.SyncLocalUI();
                }
                else
                {
                    TriviaGameUIController.Instance.SkipAvatarSelection();
                }
            }
            else
            {
                TriviaGameUIController.Instance.SetupAvatarSelection(_spritesList, SpriteIndex, PlayerName.ToString());
                TriviaGameUIController.Instance.OnAvatarConfirmed += HandleAvatarConfirmed;
            }

            if (TriviaGameUIController.Instance != null)
            {
                TriviaGameUIController.Instance.OnLocalPlayerAnswered -= HandleLocalAnswerSubmitted;
                TriviaGameUIController.Instance.OnLocalPlayerAnswered += HandleLocalAnswerSubmitted;
            }

            if (QuizGameManager.Instance != null)
            {
                _lastKnownState = QuizGameManager.Instance.CurrentState;
            }

            UpdateUICardVisuals();
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (HasStateAuthority && TriviaGameUIController.Instance != null)
        {
            TriviaGameUIController.Instance.OnAvatarConfirmed -= HandleAvatarConfirmed;
            TriviaGameUIController.Instance.OnLocalPlayerAnswered -= HandleLocalAnswerSubmitted;
        }

        bool matchStarted = QuizGameManager.Instance != null && QuizGameManager.Instance.CurrentState != QuizGameManager.GameState.Lobby;

        if (!matchStarted && UICard != null && TriviaGameUIController.Instance != null)
        {
            TriviaGameUIController.Instance.RemovePlayerCard(UICard);
        }

        if (HasStateAuthority && MainMenuController.Instance != null)
        {
            TriviaGameUIController.Instance.SetUIState(false);
            MainMenuController.Instance.SetUIState(true);
        }
    }

    private void HandleLocalAnswerSubmitted(string answerStr)
    {
        if (HasStateAuthority && QuizGameManager.Instance != null)
        {
            _mySubmittedAnswer = answerStr;
            _mySubmittedTime = QuizGameManager.Instance.StateTimer;

            // NUEVO: Guardamos el índice exacto de la pregunta que acabamos de responder
            CurrentAnswerIndex = QuizGameManager.Instance.CurrentQuestionIndex;
            HasAnsweredCurrentQuestion = true;
        }
    }

    private void HandleAvatarConfirmed(string chosenName, byte chosenSpriteIndex)
    {
        if (HasStateAuthority)
        {
            PlayerName = chosenName;
            SpriteIndex = chosenSpriteIndex;
            HasCompletedSetup = true;

            if (TriviaGameUIController.Instance != null)
            {
                TriviaGameUIController.Instance.OnAvatarConfirmed -= HandleAvatarConfirmed;
            }
        }
    }

    private void OnDataChanged()
    {
        UpdateUICardVisuals();
    }

    private void UpdateUICardVisuals()
    {
        if (UICard == null) return;

        Label nameLabel = UICard.Q<Label>("lblPlayerName");
        if (nameLabel != null) nameLabel.text = PlayerName.ToString();

        Label scoreLabel = UICard.Q<Label>("lblPlayerScore");
        if (scoreLabel != null)
        {
            bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
            string pointsWord = isEnglish ? "POINTS" : "PUNTOS";
            scoreLabel.text = $"{pointsWord}\n{Score}";
        }

        Image icon = UICard.Q<Image>("imgPlayerIcon");
        if (icon != null && SpriteIndex < _spritesList.Length)
        {
            icon.sprite = _spritesList[SpriteIndex];
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (QuizGameManager.Instance == null) return;

        if (HasStateAuthority && !HasCompletedSetup && QuizGameManager.Instance.CurrentState != QuizGameManager.GameState.Lobby)
        {
            HasCompletedSetup = true;
            if (TriviaGameUIController.Instance != null)
            {
                TriviaGameUIController.Instance.OnAvatarConfirmed -= HandleAvatarConfirmed;
            }
        }

        if (_lastKnownState != QuizGameManager.GameState.ResultPhase &&
            QuizGameManager.Instance.CurrentState == QuizGameManager.GameState.ResultPhase)
        {
            if (HasStateAuthority) CalculateMyScore();
        }

        // NEW: Unified Reset. Whenever a new question starts, clear everything safely.
        if (_lastKnownState != QuizGameManager.GameState.QuestionPhase &&
            QuizGameManager.Instance.CurrentState == QuizGameManager.GameState.QuestionPhase)
        {
            _mySubmittedAnswer = "";
            _mySubmittedTime = 0f;
            if (HasStateAuthority)
            {
                HasAnsweredCurrentQuestion = false;
                CurrentAnswerIndex = -1; // NUEVO: Reseteamos el índice para evitar falsos positivos
            }
        }

        _lastKnownState = QuizGameManager.Instance.CurrentState;
    }

    private void CalculateMyScore()
    {
        if (string.IsNullOrEmpty(_mySubmittedAnswer)) return;

        string currentQuestionID = QuizGameManager.Instance.SelectedQuestionIDs.Get(QuizGameManager.Instance.CurrentQuestionIndex).ToString();
        QuestionSO currentQuestion = QuizGameManager.Instance.GetQuestionByID(currentQuestionID);

        if (currentQuestion == null) return;

        bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
        string correctAnswerStr = "";

        // Check language to validate answer
        if (currentQuestion.Type == QuestionSO.QuestionType.MultipleChoice)
        {
            correctAnswerStr = isEnglish ? currentQuestion.CorrectAnswerEN : currentQuestion.CorrectAnswerES;
        }
        else
        {
            correctAnswerStr = isEnglish
                ? (currentQuestion.IsTrueStatement ? "TRUE" : "FALSE")
                : (currentQuestion.IsTrueStatement ? "VERDADERO" : "FALSO");
        }

        if (_mySubmittedAnswer == correctAnswerStr)
        {
            int pointsEarned = Mathf.RoundToInt(_mySubmittedTime * QuizGameManager.Instance.ScoreMultiplier);
            Score += pointsEarned;
        }
    }
}
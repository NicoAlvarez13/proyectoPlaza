using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TriviaGameUIController : MonoBehaviour
{
    public static TriviaGameUIController Instance { get; private set; }

    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private VisualTreeAsset _playerCardTemplate;

    [Header("Quiz UI Settings")]
    [SerializeField] private Sprite[] _quizBackgrounds = new Sprite[5];
    [SerializeField] private Color32[] _headerColors = new Color32[5];
    [SerializeField] private Sprite[] _categoriesIcon = new Sprite[5];

    [Header("Country Flags (alphabetical order)")]
    [SerializeField] private Sprite[] _countryFlags = new Sprite[21];

    private VisualElement _root;
    private Label _lblTitle;
    private Label _lblWaitingPlayers;
    private VisualElement _playersContainer;
    private ScrollView _playersScrollView;
    private Label _lblPersonalizeAvatar;
    private VisualElement _characterSelector;
    private Button _btnLeft;
    private Image _characterImage;
    private Button _btnRight;
    private TextField _playerNameSelector;
    private VisualElement _btnConfirmAvatarContainer;
    private Button _btnConfirmAvatar;

    private string _defaultName;
    public byte PlayerSelectedIndex;
    public Sprite[] SpritesList;

    public event Action<string, byte> OnAvatarConfirmed;
    public event Action<string> OnLocalPlayerAnswered;

    private VisualElement _quizBackground;
    private VisualElement _quizUI;
    private VisualElement _characterLayer;
    private VisualElement _headerFiller;
    private VisualElement _header;
    private Image _categoryIcon;
    private Label _categoryLabel;
    private Label _categoryName;
    private Label _timerLabel;
    private Label _timeValue;
    private VisualElement _timerVisual;
    private VisualElement _questionContainer;
    private Label _question;
    private VisualElement _multipleAnswersContainer;
    private VisualElement _trueFalseAnswersContainer;
    private VisualElement _quizUINextQuestion;
    private Label _labelNextIn;
    private Label _labelNextTime;


    // Localization Elements
    private Label _lblTimerTitle;
    private Label _lblTimeUnit;
    private Label _lblDataFormTitle;
    private Label _lblDataName;
    private Label _lblDataSurname;
    private Label _lblDataAge;
    private Label _lblDataCountry;
    private VisualElement _panelDatos;
    private VisualElement _panelGracias;
    private Button _btnSubmitData;
    private TextField _inputDataName;
    private TextField _inputDataSurname;
    private TextField _inputDataAge;
    private DropdownField _dropPaises;
    private Image _dropFlagIcon;

    // NEW: Tracker for the Thanks screen delay so we can cancel it if the game resets
    private IVisualElementScheduledItem _thanksScreenTask;

    private static readonly List<string> _countryNames = new List<string>
    {
        "Alemania", "Argentina", "Bolivia", "Brasil", "Chile",
        "Colombia", "Costa Rica", "Cuba", "Ecuador", "España",
        "Estados Unidos", "Francia", "Italia", "México",
        "Paraguay", "Perú", "Puerto Rico", "Reino Unido",
        "República Dominicana", "Uruguay", "Venezuela"
    };

    public class AnswerElement
    {
        public VisualElement Back;
        public Button Btn;

        public void ApplyState(AnswerColorState state)
        {
            if (Btn == null || Back == null) return;
            Btn.style.color = state.TextColor;
            Btn.style.backgroundColor = state.FrontColor;
            Btn.style.borderTopColor = state.BackAndBorderColor;
            Btn.style.borderBottomColor = state.BackAndBorderColor;
            Btn.style.borderLeftColor = state.BackAndBorderColor;
            Btn.style.borderRightColor = state.BackAndBorderColor;
            Back.style.backgroundColor = state.BackAndBorderColor;
        }

        public void SetInteractable(bool interactable)
        {
            if (Btn != null)
                Btn.pickingMode = interactable ? PickingMode.Position : PickingMode.Ignore;
        }
    }

    private AnswerElement[] _multipleChoiceAnswers = new AnswerElement[4];
    private AnswerElement[] _trueFalseAnswers = new AnswerElement[2];

    public struct AnswerColorState
    {
        public StyleColor TextColor;
        public StyleColor FrontColor;
        public StyleColor BackAndBorderColor;
        public AnswerColorState(Color32 text, Color32 front, Color32 backBorder)
        {
            TextColor = new StyleColor(text);
            FrontColor = new StyleColor(front);
            BackAndBorderColor = new StyleColor(backBorder);
        }
    }

    public static readonly AnswerColorState StateNormal = new AnswerColorState(new Color32(0, 0, 0, 255), new Color32(255, 255, 255, 255), new Color32(195, 195, 195, 255));
    public static readonly AnswerColorState StateSelected = new AnswerColorState(new Color32(255, 255, 255, 255), new Color32(40, 169, 255, 255), new Color32(0, 98, 164, 255));
    public static readonly AnswerColorState StateBlocked = new AnswerColorState(new Color32(87, 87, 87, 255), new Color32(195, 195, 195, 255), new Color32(159, 159, 159, 255));
    public static readonly AnswerColorState StateTrue = new AnswerColorState(new Color32(255, 255, 255, 255), new Color32(122, 224, 79, 255), new Color32(49, 118, 41, 255));
    public static readonly AnswerColorState StateFalse = new AnswerColorState(new Color32(255, 255, 255, 255), new Color32(255, 58, 49, 255), new Color32(152, 7, 0, 255));

    private int _localSelectedIndex = -1;
    private string _localCorrectAnswer = "";
    private bool _canAnswerLocal = false;

    private float _localSubmittedTime = 0f;
    private string _currentLoadedQuestionID = "";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Application.runInBackground = true;

        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;
        InitializeUI();
    }

    private void Start()
    {
        ApplyLocalization();
    }

    private void InitializeUI()
    {
        _root = _uiDocument.rootVisualElement;

        _lblTitle = _root.Q<Label>("lblTitle");
        _lblWaitingPlayers = _root.Q<Label>("lblWaitingPlayers");
        _playersContainer = _root.Q<VisualElement>("PlayersContainer");
        _playersScrollView = _root.Q<ScrollView>("PlayersScrollView");
        _lblPersonalizeAvatar = _root.Q<Label>("lblPersonalizeAvatar");
        _characterSelector = _root.Q<VisualElement>("CharacterSelector");
        _btnLeft = _root.Q<Button>("Left");
        _characterImage = _root.Q<Image>("CharacterImage");
        _btnRight = _root.Q<Button>("Right");
        _playerNameSelector = _root.Q<TextField>("PlayerNameSelector");
        _btnConfirmAvatarContainer = _root.Q<VisualElement>("btnCofirmAvatar");
        if (_btnConfirmAvatarContainer != null) _btnConfirmAvatar = _btnConfirmAvatarContainer.Q<Button>();

        if (_playersScrollView != null) _playersScrollView.contentContainer.Clear();

        SetInitialDisplayStates();

        _quizBackground = _root.Q<VisualElement>("QuizBackground");
        _quizUI = _root.Q<VisualElement>("QuizUI");

        _characterLayer = _root.Q<VisualElement>("CharacterLayer");
        _headerFiller = _root.Q<VisualElement>("HeaderFiller");
        _header = _root.Q<VisualElement>("Header");
        _categoryIcon = _root.Q<Image>("CategoryIcon");
        _categoryLabel = _root.Q<Label>("CategoryLabel");
        _categoryName = _root.Q<Label>("CategoryName");
        _timerLabel = _root.Q<Label>("TimerLabel");
        _timeValue = _root.Q<Label>("TimeValue");
        _timerVisual = _root.Q<VisualElement>("TimerVisual");
        _questionContainer = _root.Q<VisualElement>("QuestionContainer");
        _question = _root.Q<Label>("Question");
        _multipleAnswersContainer = _root.Q<VisualElement>("MultipleAnswersContainer");
        _trueFalseAnswersContainer = _root.Q<VisualElement>("TrueFalseAnswersContainer");
        _quizUINextQuestion = _root.Q<VisualElement>("QuizUINextQuestion");
        _labelNextIn = _root.Q<Label>("labelNextIn");
        _labelNextTime = _root.Q<Label>("labelNextTime");

        _lblTimerTitle = _root.Q<Label>("TimerLabel");
        _lblTimeUnit = _root.Q<Label>("TimeUnit");

        _panelDatos = _root.Q<VisualElement>("PanelDatos");
        _panelGracias = _root.Q<VisualElement>("PanelGracias");

        if (_panelDatos != null)
        {
            var allLabels = _panelDatos.Query<Label>().ToList();
            if (allLabels.Count > 0) _lblDataFormTitle = allLabels[0];

            _lblDataName = _panelDatos.Q<Label>("Nombre");
            _lblDataSurname = _panelDatos.Q<Label>("Apellido");

            var edadLabels = _panelDatos.Query<Label>("Edad").ToList();
            if (edadLabels.Count > 0) _lblDataAge = edadLabels[0];
            if (edadLabels.Count > 1) _lblDataCountry = edadLabels[1];

            _btnSubmitData = _panelDatos.Q<Button>("JoinButton");
            if (_btnSubmitData != null)
                _btnSubmitData.clicked += OnSubmitDataClicked;
        }

        var formInputs = _panelDatos?.Query<TextField>().ToList();
        if (formInputs != null && formInputs.Count >= 3)
        {
            _inputDataName = formInputs[0];
            _inputDataSurname = formInputs[1];
            _inputDataAge = formInputs[2];
        }

        _dropPaises = _panelDatos?.Q<DropdownField>("DropPaises");
        _dropFlagIcon = _panelDatos?.Q<Image>("SelectedFlagIcon");

        if (_dropPaises != null)
        {
            _dropPaises.choices = _countryNames;
            _dropPaises.index = 0;
            UpdateFlagIcon(0);
            _dropPaises.RegisterValueChangedCallback(evt =>
            {
                int idx = _countryNames.IndexOf(evt.newValue);
                UpdateFlagIcon(idx);
            });
            _dropPaises.RegisterCallback<PointerDownEvent>(_ =>
            {
                _dropPaises.schedule.Execute(LimitDropdownPopupHeight).StartingIn(50);
            });
        }

        for (int i = 0; i < 4; i++)
        {
            VisualElement container = _root.Q<VisualElement>($"Answer{i + 1}");
            if (container != null)
            {
                _multipleChoiceAnswers[i] = new AnswerElement { Back = container.Q<VisualElement>("Back"), Btn = container.Q<Button>() };
                int index = i;
                _multipleChoiceAnswers[i].Btn.clicked += () => OnAnswerButtonClicked(index, true, _multipleChoiceAnswers[index].Btn.text);
            }
        }

        VisualElement trueContainer = _root.Q<VisualElement>("AnswerTrue");
        if (trueContainer != null)
        {
            _trueFalseAnswers[0] = new AnswerElement { Back = trueContainer.Q<VisualElement>("Back"), Btn = trueContainer.Q<Button>() };
            _trueFalseAnswers[0].Btn.clicked += () => OnAnswerButtonClicked(0, false, _trueFalseAnswers[0].Btn.text);
        }

        VisualElement falseContainer = _root.Q<VisualElement>("AnswerFalse");
        if (falseContainer != null)
        {
            _trueFalseAnswers[1] = new AnswerElement { Back = falseContainer.Q<VisualElement>("Back"), Btn = falseContainer.Q<Button>() };
            _trueFalseAnswers[1].Btn.clicked += () => OnAnswerButtonClicked(1, false, _trueFalseAnswers[1].Btn.text);
        }

        ShowQuizUI(false);
        HideNextQuestionScreen();
        // Add these lines to force the panels to hide on startup
        if (_panelDatos != null) _panelDatos.style.display = DisplayStyle.None;
        if (_panelGracias != null) _panelGracias.style.display = DisplayStyle.None;

        SetUIState(false);
    }

    public void ResetUI()
    {
        // NEW: Ensure lingering tasks are canceled and panels are forced hidden
        _thanksScreenTask?.Pause();
        _thanksScreenTask = null;
        if (_panelDatos != null) _panelDatos.style.display = DisplayStyle.None;
        if (_panelGracias != null) _panelGracias.style.display = DisplayStyle.None;

        if (_playersScrollView != null) _playersScrollView.contentContainer.Clear();

        ShowQuizUI(false);
        HideNextQuestionScreen();
        ResetToAvatarSelection();

        _localSelectedIndex = -1;
        _canAnswerLocal = false;
        _localSubmittedTime = 0f;
        _currentLoadedQuestionID = "";
        SetUIState(false);
    }

    public void ForceHideAvatarSelection()
    {
        if (_lblPersonalizeAvatar != null) _lblPersonalizeAvatar.style.display = DisplayStyle.None;
        if (_characterSelector != null) _characterSelector.style.display = DisplayStyle.None;
        if (_playerNameSelector != null) _playerNameSelector.style.display = DisplayStyle.None;
        if (_btnConfirmAvatarContainer != null) _btnConfirmAvatarContainer.style.display = DisplayStyle.None;
    }

    private void SetInitialDisplayStates()
    {
        if (_playersContainer != null) _playersContainer.style.visibility = Visibility.Hidden;
        if (_lblPersonalizeAvatar != null) _lblPersonalizeAvatar.style.display = DisplayStyle.Flex;
        if (_characterSelector != null) _characterSelector.style.display = DisplayStyle.Flex;
        if (_playerNameSelector != null) _playerNameSelector.style.display = DisplayStyle.Flex;
        if (_btnConfirmAvatarContainer != null) _btnConfirmAvatarContainer.style.display = DisplayStyle.Flex;

        if (_btnConfirmAvatar != null) _btnConfirmAvatar.clicked += OnSaveAvatarButtonClicked;
        if (_btnLeft != null) _btnLeft.clicked += OnLeftButtonClicked;
        if (_btnRight != null) _btnRight.clicked += OnRightButtonClicked;
    }

    public void SetupCurrentQuestionUI(QuestionSO q)
    {
        if (_currentLoadedQuestionID == q.QuestionID) return;

        // NEW: Ensure forms are hidden when a question appears
        _thanksScreenTask?.Pause();
        _thanksScreenTask = null;
        if (_panelDatos != null) _panelDatos.style.display = DisplayStyle.None;
        if (_panelGracias != null) _panelGracias.style.display = DisplayStyle.None;

        _currentLoadedQuestionID = q.QuestionID;

        HideNextQuestionScreen();

        if (_playersContainer != null) _playersContainer.style.visibility = Visibility.Hidden;

        int categoryIndex = 0;
        string categoryNameStr = "AIRE";

        bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;

        if (QuizGameManager.Instance != null)
        {
            for (int i = 0; i < QuizGameManager.Instance.AllCategoriesDatabase.Count; i++)
            {
                var cat = QuizGameManager.Instance.AllCategoriesDatabase[i];
                if (cat.Questions.Any(ques => ques != null && ques.QuestionID == q.QuestionID))
                {
                    categoryNameStr = !string.IsNullOrWhiteSpace(cat.CategoryNameES) ? cat.CategoryNameES : cat.name;
                    string lowerName = categoryNameStr.ToLower();

                    if (lowerName.Contains("aire") || lowerName.Contains("air"))
                    {
                        categoryIndex = 0;
                        categoryNameStr = isEnglish ? "AIR" : "AIRE";
                    }
                    else if (lowerName.Contains("tierra") || lowerName.Contains("earth"))
                    {
                        categoryIndex = 1;
                        categoryNameStr = isEnglish ? "EARTH" : "TIERRA";
                    }
                    else if (lowerName.Contains("fuego") || lowerName.Contains("fire"))
                    {
                        categoryIndex = 2;
                        categoryNameStr = isEnglish ? "FIRE" : "FUEGO";
                    }
                    else if (lowerName.Contains("agua") || lowerName.Contains("water"))
                    {
                        categoryIndex = 3;
                        categoryNameStr = isEnglish ? "WATER" : "AGUA";
                    }
                    else if (lowerName.Contains("bonus"))
                    {
                        categoryIndex = 4;
                        categoryNameStr = "BONUS";
                    }
                    else
                    {
                        categoryIndex = i;
                    }

                    break;
                }
            }
        }

        if (categoryIndex >= 0 && categoryIndex < _quizBackgrounds.Length && _quizBackgrounds[categoryIndex] != null)
        {
            if (_quizBackground != null)
                _quizBackground.style.backgroundImage = new StyleBackground(_quizBackgrounds[categoryIndex]);
        }

        if (categoryIndex >= 0 && categoryIndex < _headerColors.Length)
        {
            Color32 c32 = _headerColors[categoryIndex];
            if (c32.a == 0) c32.a = 255;

            StyleColor headerCol = new StyleColor((Color)c32);
            if (_headerFiller != null) _headerFiller.style.backgroundColor = headerCol;
            if (_header != null) _header.style.backgroundColor = headerCol;
        }

        if (categoryIndex >= 0 && categoryIndex < _categoriesIcon.Length && _categoriesIcon[categoryIndex] != null)
        {
            if (_categoryIcon != null)
                _categoryIcon.sprite = _categoriesIcon[categoryIndex];
        }

        // FIX: Removed accent from "CATEGORIA"
        if (_categoryLabel != null)
        {
            _categoryLabel.text = isEnglish ? "CATEGORY" : "CATEGORIA";
        }

        if (_categoryName != null)
        {
            _categoryName.text = categoryNameStr.ToUpper();
        }

        if (TriviaGame_UICharcatersAnimations.Instance != null)
        {
            TriviaGame_UICharcatersAnimations.Instance.ShowCharacterForCategory(categoryNameStr);
        }

        _localSelectedIndex = -1;
        _localSubmittedTime = 0f;
        _canAnswerLocal = true;

        _question.text = isEnglish ? q.QuestionTextEN : q.QuestionTextES;

        if (q.Type == QuestionSO.QuestionType.MultipleChoice)
        {
            _localCorrectAnswer = isEnglish ? q.CorrectAnswerEN : q.CorrectAnswerES;
            DisplayAnswerType(true);

            List<string> options = isEnglish
                ? new List<string> { q.CorrectAnswerEN, q.IncorrectAnswer1EN, q.IncorrectAnswer2EN, q.IncorrectAnswer3EN }
                : new List<string> { q.CorrectAnswerES, q.IncorrectAnswer1ES, q.IncorrectAnswer2ES, q.IncorrectAnswer3ES };

            System.Random rng = new System.Random();
            options = options.OrderBy(a => rng.Next()).ToList();

            for (int i = 0; i < 4; i++)
            {
                _multipleChoiceAnswers[i].Btn.text = options[i];
                _multipleChoiceAnswers[i].ApplyState(StateNormal);
                _multipleChoiceAnswers[i].SetInteractable(true);
            }
        }
        else
        {
            _localCorrectAnswer = isEnglish ? (q.IsTrueStatement ? "TRUE" : "FALSE") : (q.IsTrueStatement ? "VERDADERO" : "FALSO");
            DisplayAnswerType(false);

            SetTrueFalseTexts(isEnglish ? "TRUE" : "VERDADERO", isEnglish ? "FALSE" : "FALSO");

            _trueFalseAnswers[0].ApplyState(StateTrue);
            _trueFalseAnswers[0].SetInteractable(true);

            _trueFalseAnswers[1].ApplyState(StateFalse);
            _trueFalseAnswers[1].SetInteractable(true);
        }

        ShowQuizUI(true);
    }

    private void OnAnswerButtonClicked(int index, bool isMultipleChoice, string text)
    {
        if (!_canAnswerLocal) return;
        _canAnswerLocal = false;

        _localSelectedIndex = index;
        _localSubmittedTime = QuizGameManager.Instance != null ? QuizGameManager.Instance.StateTimer : 0f;

        SelectAnswer(index, isMultipleChoice);
        OnLocalPlayerAnswered?.Invoke(text);
    }

    public void ShowResultsUI()
    {
        _canAnswerLocal = false;

        SetTimerVisual(0f);
        SetTimerValue("0.00");

        bool isMultipleChoice = _multipleAnswersContainer.style.display == DisplayStyle.Flex;

        string clickedText = "";
        if (_localSelectedIndex >= 0)
        {
            if (isMultipleChoice) clickedText = _multipleChoiceAnswers[_localSelectedIndex].Btn.text;
            else clickedText = _trueFalseAnswers[_localSelectedIndex].Btn.text;
        }

        bool isCorrect = (_localSelectedIndex != -1) && (clickedText == _localCorrectAnswer);

        if (_localSelectedIndex == -1 || !isCorrect)
            AudioManager.Instance?.PlayIncorrect();
        else
            AudioManager.Instance?.PlayCorrect();

        bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;

        if (_question != null)
        {
            if (_localSelectedIndex == -1)
            {
                _question.text = isEnglish ? "<color=#FF3A31>TIME OUT</color>" : "<color=#FF3A31>TIEMPO AGOTADO</color>";
            }
            else
            {
                if (isCorrect)
                {
                    int pointsEarned = 0;
                    if (QuizGameManager.Instance != null)
                    {
                        pointsEarned = Mathf.RoundToInt(_localSubmittedTime * QuizGameManager.Instance.ScoreMultiplier);
                    }

                    string correctStr = isEnglish ? "CORRECT" : "CORRECTO";
                    string pointsStr = isEnglish ? "Points" : "Puntos";
                    _question.text = $"<color=#7AE04F>{correctStr}\n+{pointsEarned} {pointsStr}</color>";
                }
                else
                {
                    _question.text = isEnglish ? "<color=#FF3A31>INCORRECT</color>" : "<color=#FF3A31>INCORRECTO</color>";
                }
            }
        }

        if (isMultipleChoice)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_multipleChoiceAnswers[i] == null || _multipleChoiceAnswers[i].Btn == null) continue;

                string btnText = _multipleChoiceAnswers[i].Btn.text;
                if (btnText == _localCorrectAnswer)
                {
                    _multipleChoiceAnswers[i].ApplyState(StateTrue);
                }
                else if (i == _localSelectedIndex && !isCorrect)
                {
                    _multipleChoiceAnswers[i].ApplyState(StateFalse);
                }
                else
                {
                    _multipleChoiceAnswers[i].ApplyState(StateBlocked);
                }
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                if (_trueFalseAnswers[i] == null || _trueFalseAnswers[i].Btn == null) continue;

                string btnText = _trueFalseAnswers[i].Btn.text;
                if (btnText == _localCorrectAnswer)
                {
                    _trueFalseAnswers[i].ApplyState(StateTrue);
                }
                else if (i == _localSelectedIndex && !isCorrect)
                {
                    _trueFalseAnswers[i].ApplyState(StateFalse);
                }
                else
                {
                    _trueFalseAnswers[i].ApplyState(StateBlocked);
                }
            }
        }
    }

    public void ShowNextQuestionScreen(string nextInText, string timeText)
    {
        // NEW: Ensure forms are hidden
        _thanksScreenTask?.Pause();
        _thanksScreenTask = null;
        if (_panelDatos != null) _panelDatos.style.display = DisplayStyle.None;
        if (_panelGracias != null) _panelGracias.style.display = DisplayStyle.None;

        if (_playersContainer != null) _playersContainer.style.visibility = Visibility.Hidden;

        if (_labelNextIn != null) _labelNextIn.text = nextInText;
        if (_labelNextTime != null) _labelNextTime.text = timeText;
        if (_quizUINextQuestion != null) _quizUINextQuestion.style.display = DisplayStyle.Flex;
        if (_quizUI != null) _quizUI.style.display = DisplayStyle.None;

        ResetAnswersToHiddenState();
    }

    private void ResetAnswersToHiddenState()
    {
        _localSelectedIndex = -1;
        _currentLoadedQuestionID = "";

        for (int i = 0; i < 4; i++)
        {
            if (_multipleChoiceAnswers[i] != null)
            {
                _multipleChoiceAnswers[i].ApplyState(StateNormal);
            }
        }

        for (int i = 0; i < 2; i++)
        {
            if (_trueFalseAnswers[i] != null)
            {
                _trueFalseAnswers[i].ApplyState(i == 0 ? StateTrue : StateFalse);
            }
        }
    }

    public void HideNextQuestionScreen()
    {
        if (_quizUINextQuestion != null) _quizUINextQuestion.style.display = DisplayStyle.None;
    }

    public void UpdateNextQuestionTime(string timeText)
    {
        if (_labelNextTime != null) _labelNextTime.text = timeText;
    }

    public void ShowGuideLiveLeaderboard()
    {
        // NEW: Ensure forms are hidden
        _thanksScreenTask?.Pause();
        _thanksScreenTask = null;
        if (_panelDatos != null) _panelDatos.style.display = DisplayStyle.None;
        if (_panelGracias != null) _panelGracias.style.display = DisplayStyle.None;

        ShowQuizUI(false);
        HideNextQuestionScreen();

        if (_playersContainer != null) _playersContainer.style.visibility = Visibility.Visible;

        ForceHideAvatarSelection();

        bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
        if (_lblTitle != null) _lblTitle.text = isEnglish ? "LIVE SCORES" : "PUNTAJES EN VIVO";
        if (_lblWaitingPlayers != null) _lblWaitingPlayers.text = "";

        SortAndShowLeaderboard(false);
    }

    public void SortAndShowLeaderboard(bool isFinal, bool isGuide = false)
    {
        bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
        string pointsStr = isEnglish ? "POINTS" : "PUNTOS";

        if (isFinal)
        {
            ShowQuizUI(false);
            HideNextQuestionScreen();
            ForceHideAvatarSelection();

            if (!isGuide && _panelDatos != null)
            {
                _panelDatos.style.display = DisplayStyle.Flex;
                return;
            }
        }

        if (_playersScrollView != null)
        {
            _playersScrollView.contentContainer.Clear();
            var players = FindObjectsByType<PlayerNetworkData>(FindObjectsSortMode.None)
                            .OrderByDescending(p => p.Score)
                            .ToList();

            foreach (var p in players)
            {
                if (p.UICard != null)
                {
                    Label scoreLabel = p.UICard.Q<Label>("lblPlayerScore");
                    if (scoreLabel != null)
                    {
                        scoreLabel.text = $"{pointsStr}\n{p.Score}";
                        scoreLabel.style.display = DisplayStyle.Flex;
                    }
                    _playersScrollView.Add(p.UICard);
                }
            }
        }
    }

    public void ShowQuizUI(bool isVisible)
    {
        DisplayStyle style = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
        if (_quizBackground != null) _quizBackground.style.display = style;
        if (_characterLayer != null) _characterLayer.style.display = style;
        if (_quizUI != null) _quizUI.style.display = style;
    }

    public void SetTimerVisual(float fillPercentage)
    {
        if (_timerVisual != null)
        {
            fillPercentage = Mathf.Clamp01(fillPercentage);
            _timerVisual.style.width = new StyleLength(new Length(fillPercentage * 100f, LengthUnit.Percent));
        }
    }

    public void SetTimerValue(string timeText)
    {
        if (_timeValue != null) _timeValue.text = timeText;
    }

    public void DisplayAnswerType(bool isMultipleChoice)
    {
        if (_multipleAnswersContainer != null) _multipleAnswersContainer.style.display = isMultipleChoice ? DisplayStyle.Flex : DisplayStyle.None;
        if (_trueFalseAnswersContainer != null) _trueFalseAnswersContainer.style.display = isMultipleChoice ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public void SetTrueFalseTexts(string trueText, string falseText)
    {
        if (_trueFalseAnswers[0]?.Btn != null) _trueFalseAnswers[0].Btn.text = trueText;
        if (_trueFalseAnswers[1]?.Btn != null) _trueFalseAnswers[1].Btn.text = falseText;
    }

    public void SelectAnswer(int selectedIndex, bool isMultipleChoice)
    {
        if (isMultipleChoice)
        {
            for (int i = 0; i < _multipleChoiceAnswers.Length; i++)
            {
                AnswerColorState stateToApply = (i == selectedIndex) ? StateSelected : StateBlocked;
                _multipleChoiceAnswers[i]?.ApplyState(stateToApply);
                _multipleChoiceAnswers[i]?.SetInteractable(false);
            }
        }
        else
        {
            for (int i = 0; i < _trueFalseAnswers.Length; i++)
            {
                AnswerColorState stateToApply = (i == selectedIndex) ? StateSelected : StateBlocked;
                _trueFalseAnswers[i]?.ApplyState(stateToApply);
                _trueFalseAnswers[i]?.SetInteractable(false);
            }
        }
    }

    public void SetupAvatarSelection(Sprite[] sprites, byte initialIndex, string defaultName)
    {
        ResetToAvatarSelection();
        SpritesList = sprites;
        PlayerSelectedIndex = initialIndex;
        _defaultName = defaultName;
        if (_playerNameSelector != null) _playerNameSelector.value = _defaultName;
        UpdateAvatarPreview();
    }

    public void SkipAvatarSelection()
    {
        ForceHideAvatarSelection();
        if (_playersContainer != null) _playersContainer.style.visibility = Visibility.Visible;
    }

    public void ResetToAvatarSelection()
    {
        // Add this line to hide the panel when returning to the lobby
        if (_panelDatos != null) _panelDatos.style.display = DisplayStyle.None;

        if (_playersContainer != null) _playersContainer.style.visibility = Visibility.Hidden;
        if (_lblPersonalizeAvatar != null) _lblPersonalizeAvatar.style.display = DisplayStyle.Flex;
        if (_characterSelector != null) _characterSelector.style.display = DisplayStyle.Flex;
        if (_playerNameSelector != null) _playerNameSelector.style.display = DisplayStyle.Flex;
        if (_btnConfirmAvatarContainer != null) _btnConfirmAvatarContainer.style.display = DisplayStyle.Flex;
    }

    private void OnSaveAvatarButtonClicked()
    {
        SkipAvatarSelection();
        string chosenName = _playerNameSelector != null ? _playerNameSelector.value : "";
        if (string.IsNullOrWhiteSpace(chosenName)) chosenName = _defaultName;
        OnAvatarConfirmed?.Invoke(chosenName, PlayerSelectedIndex);
    }

    private void OnLeftButtonClicked()
    {
        if (SpritesList == null || SpritesList.Length == 0) return;
        if (PlayerSelectedIndex == 0) PlayerSelectedIndex = (byte)(SpritesList.Length - 1);
        else PlayerSelectedIndex--;
        UpdateAvatarPreview();
    }

    private void OnRightButtonClicked()
    {
        if (SpritesList == null || SpritesList.Length == 0) return;
        if (PlayerSelectedIndex >= SpritesList.Length - 1) PlayerSelectedIndex = 0;
        else PlayerSelectedIndex++;
        UpdateAvatarPreview();
    }

    private void UpdateAvatarPreview()
    {
        if (_characterImage != null && SpritesList != null && PlayerSelectedIndex < SpritesList.Length)
        {
            _characterImage.sprite = SpritesList[PlayerSelectedIndex];
        }
    }

    public VisualElement AddPlayerCard(string playerName, string scoreText, Sprite avatarIcon)
    {
        if (_playerCardTemplate == null || _playersScrollView == null) return null;
        VisualElement newCard = _playerCardTemplate.Instantiate();
        newCard.style.flexShrink = 0;

        Label nameLabel = newCard.Q<Label>("lblPlayerName");
        if (nameLabel != null) nameLabel.text = playerName;

        Label scoreLabel = newCard.Q<Label>("lblPlayerScore");
        if (scoreLabel != null)
        {
            bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
            string pointsStr = isEnglish ? "POINTS" : "PUNTOS";
            scoreLabel.text = $"{pointsStr}\n{scoreText}";
            scoreLabel.style.display = DisplayStyle.None;
        }

        Image icon = newCard.Q<Image>("imgPlayerIcon");
        if (icon != null && avatarIcon != null) icon.sprite = avatarIcon;

        _playersScrollView.Add(newCard);
        return newCard;
    }

    public void RemovePlayerCard(VisualElement cardToRemove)
    {
        if (cardToRemove != null && _playersScrollView.contentContainer.Contains(cardToRemove))
        {
            _playersScrollView.Remove(cardToRemove);
        }
    }

    public void SetUIState(bool isVisible)
    {
        if (_root != null) _root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void UpdateRoomName(string roomName)
    {
        if (_lblTitle != null)
        {
            bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
            _lblTitle.text = (isEnglish ? "ROOM: " : "SALA: ") + roomName;
        }
    }

    public void UpdateWaitingPlayersText(string text)
    {
        if (_lblWaitingPlayers != null) _lblWaitingPlayers.text = text;
    }

    public void ApplyLocalization()
    {
        bool isEn = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;

        if (_lblPersonalizeAvatar != null) _lblPersonalizeAvatar.text = isEn ? "PERSONALIZE AVATAR" : "PERSONALIZAR AVATAR";
        if (_btnConfirmAvatar != null) _btnConfirmAvatar.text = isEn ? "Confirm" : "Confirmar";

        if (_lblTimerTitle != null) _lblTimerTitle.text = isEn ? "TIME LEFT" : "TIEMPO RESTANTE";
        if (_lblTimeUnit != null) _lblTimeUnit.text = isEn ? "SEC." : "SEG.";

        if (_lblDataFormTitle != null) _lblDataFormTitle.text = isEn ? "Fill in the details to see the results" : "Complete los datos para ver los resultados";
        if (_lblDataName != null) _lblDataName.text = isEn ? "Name" : "Nombre";
        if (_lblDataSurname != null) _lblDataSurname.text = isEn ? "Surname" : "Apellido";
        if (_lblDataAge != null) _lblDataAge.text = isEn ? "Age" : "Edad";
        if (_lblDataCountry != null) _lblDataCountry.text = isEn ? "Country" : "Pais";
        if (_btnSubmitData != null) _btnSubmitData.text = isEn ? "Submit Data" : "Enviar Datos";

        if (_playerNameSelector != null)
        {
            _playerNameSelector.textEdition.placeholder = isEn ? "Player Name..." : "Nombre del Jugador...";
        }

        if (_inputDataName != null)
            _inputDataName.textEdition.placeholder = isEn ? "Name..." : "Nombre...";

        if (_inputDataSurname != null)
            _inputDataSurname.textEdition.placeholder = isEn ? "Surname..." : "Apellido...";

        if (_inputDataAge != null)
            _inputDataAge.textEdition.placeholder = isEn ? "Age..." : "Edad...";

        if (_dropPaises != null)
            _dropPaises.label = string.Empty;
    }

    private void LimitDropdownPopupHeight()
    {
        Debug.Log("[Submit] Entrando a LimitDropdownPopupHeight");
        var panelRoot = _dropPaises?.panel?.visualTree;
        if (panelRoot == null) return;

        var container = panelRoot.Q(className: "unity-base-dropdown__container-outer");

        if (container != null)
        {
            // 1. LIMITAR ALTURA (Lo que ya funciona)
            container.style.maxHeight = 250; 
            container.style.height = StyleKeyword.Auto;

            // 2. CORREGIR POSICIÓN (El "Anclaje")
            // Obtenemos dónde está parado el dropdown en la pantalla
            var dropdownBounds = _dropPaises.worldBound;

            // Forzamos que el popup empiece justo donde termina el dropdown
            // y que tenga el mismo ancho para que no se vea finito
            container.style.position = Position.Absolute;
            container.style.top = dropdownBounds.yMax; 
            container.style.left = dropdownBounds.xMin;
            container.style.width = dropdownBounds.width;

            // 3. FUENTE Y SCROLL (Para mantener lo anterior)
            var scrollView = container.Q<ScrollView>();
            if (scrollView != null) {

                // Desactiva la visibilidad de las barras
                scrollView.verticalScrollerVisibility = ScrollerVisibility.Hidden;
                scrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
    
                // Opcional: Forzar el modo de scroll para que sea más fluido en táctil
                scrollView.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
                scrollView.style.maxHeight = 250;   
            }

            var itemLabels = container.Query<Label>(className: "unity-base-dropdown__item-label").ToList();
            if (_btnSubmitData != null && itemLabels.Count > 0)
            {
                var customFont = _btnSubmitData.resolvedStyle.unityFontDefinition;
                foreach (var label in itemLabels)
                {
                    label.style.unityFontDefinition = customFont;
                    label.style.fontSize = 20;
                    label.style.color = Color.black;
                }
            }
            Debug.Log("[Submit] Altura y posición corregidas");

            container.style.opacity = 1;
        }
    }

    private void UpdateFlagIcon(int index)
    {
        if (_dropFlagIcon == null) return;
        if (index >= 0 && index < _countryFlags.Length && _countryFlags[index] != null)
        {
            _dropFlagIcon.sprite = _countryFlags[index];
            _dropFlagIcon.style.display = DisplayStyle.Flex;
        }
        else
        {
            _dropFlagIcon.style.display = DisplayStyle.None;
        }
    }

    public string GetSelectedCountry()
    {
        return _dropPaises != null ? _dropPaises.value : string.Empty;
    }

    private void OnSubmitDataClicked()
    {
        string nombre = _inputDataName != null ? _inputDataName.value.Trim() : "";
        string apellido = _inputDataSurname != null ? _inputDataSurname.value.Trim() : "";
        string edadStr = _inputDataAge != null ? _inputDataAge.value.Trim() : "0";
        string pais = _dropPaises != null ? _dropPaises.value : "";

        if (!int.TryParse(edadStr, out int edad)) edad = 0;

        var localData = FindFirstObjectByType<PlayerNetworkData>();
        foreach (var pd in FindObjectsByType<PlayerNetworkData>(FindObjectsSortMode.None))
        {
            if (pd.Object != null && pd.Object.HasInputAuthority)
            {
                localData = pd;
                break;
            }
        }

        string usuario = localData != null ? localData.PlayerName.ToString() : "";
        int puntos = localData != null ? localData.Score : 0;

        if (SupabaseService.Instance == null)
        {
            Debug.LogError("[Submit] SupabaseService no encontrado en escena.");
            return;
        }

        _btnSubmitData.SetEnabled(false);

        SupabaseService.Instance.InsertJugador(
            usuario, nombre, apellido, edad, pais, puntos,
            onSuccess: () =>
            {
                Debug.Log("[Submit] Datos enviados correctamente.");
                if (_panelDatos != null) _panelDatos.style.display = DisplayStyle.None;

                bool isEnglish = GameManager.Instance != null && GameManager.Instance.CurrentLanguage == GameManager.GameLanguage.english;
                if (_playersContainer != null) _playersContainer.style.visibility = Visibility.Visible;
                if (_lblTitle != null) _lblTitle.text = isEnglish ? "RESULTS" : "RESULTADOS";
                if (_lblWaitingPlayers != null) _lblWaitingPlayers.text = "";
                SortAndShowLeaderboard(false);

                // NEW: Use the tracker variable to store the scheduled task
                _thanksScreenTask = _root.schedule.Execute(ShowThanksScreen);
                _thanksScreenTask.StartingIn(10000);
            },
            onError: err =>
            {
                Debug.LogError($"[Submit] Fallo al enviar: {err}");
                _btnSubmitData.SetEnabled(true);
            }
        );
    }

    public VisualElement GetCharacterLayer() => _characterLayer;

    public void ShowThanksScreen()
    {
        if (_root != null) _root.style.display = DisplayStyle.Flex;
        if (_panelGracias != null) _panelGracias.style.display = DisplayStyle.Flex;
    }

    [ContextMenu("Debug: Show PanelDatos")]
    public void Debug_ShowPanelDatos()
    {
        if (_root != null)
            _root.style.display = DisplayStyle.Flex;
        if (_panelDatos != null)
            _panelDatos.style.display = DisplayStyle.Flex;
    }
}
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuestion", menuName = "Quiz/Question")]
public class QuestionSO : ScriptableObject
{
    // Auto-generated unique ID for network syncing
    public string QuestionID;

    public DifficultyLevel Difficulty;
    public QuestionType Type;

    [TextArea(2, 3)] public string QuestionTextEN;
    [TextArea(2, 3)] public string QuestionTextES;

    // --- MULTIPLE CHOICE DATA ---
    public string CorrectAnswerEN;
    public string CorrectAnswerES;

    public string IncorrectAnswer1EN;
    public string IncorrectAnswer1ES;

    public string IncorrectAnswer2EN;
    public string IncorrectAnswer2ES;

    public string IncorrectAnswer3EN;
    public string IncorrectAnswer3ES;

    // --- TRUE/FALSE DATA ---
    // If TrueFalse is selected, we just need to know if the statement is true.
    // The UI can hardcode the localized words for "True" and "False".
    public bool IsTrueStatement;

    public enum DifficultyLevel
    {
        Easy,
        Medium,
        Hard
    }

    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse
    }

    // This Unity function runs automatically in the Editor when the asset is created or changed.
    // It ensures every question gets a permanent, unique ID automatically.
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(QuestionID))
        {
            QuestionID = System.Guid.NewGuid().ToString();

            // Marks the asset as dirty so Unity saves the new ID
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
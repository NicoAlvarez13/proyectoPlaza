using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCategory", menuName = "Quiz/Category")]
public class CategorySO : ScriptableObject
{
    [Header("Category Info")]
    public string CategoryNameEN;
    public string CategoryNameES;

    [Header("Questions Pool")]
    [Tooltip("Drag and drop all the QuestionSOs that belong to this category here.")]
    public List<QuestionSO> Questions = new List<QuestionSO>();

    // --- Helper Methods for the Game Manager ---

    // Returns a list of questions that match the requested difficulty
    public List<QuestionSO> GetQuestionsByDifficulty(QuestionSO.DifficultyLevel difficulty)
    {
        List<QuestionSO> filteredQuestions = new List<QuestionSO>();

        foreach (QuestionSO question in Questions)
        {
            if (question != null && question.Difficulty == difficulty)
            {
                filteredQuestions.Add(question);
            }
        }

        return filteredQuestions;
    }

    // Returns a specific amount of random questions of a certain difficulty without duplicates
    public List<QuestionSO> GetRandomQuestions(QuestionSO.DifficultyLevel difficulty, int amountToPick)
    {
        List<QuestionSO> availableQuestions = GetQuestionsByDifficulty(difficulty);
        List<QuestionSO> pickedQuestions = new List<QuestionSO>();

        // Failsafe: If we ask for more questions than we have, just cap it to what we have
        if (amountToPick > availableQuestions.Count)
        {
            amountToPick = availableQuestions.Count;
            Debug.LogWarning($"Not enough {difficulty} questions in {CategoryNameEN}. Pulling all available ({amountToPick}).");
        }

        // Randomly pick questions and remove them from the temporary list to avoid duplicates
        for (int i = 0; i < amountToPick; i++)
        {
            int randomIndex = Random.Range(0, availableQuestions.Count);
            pickedQuestions.Add(availableQuestions[randomIndex]);
            availableQuestions.RemoveAt(randomIndex);
        }

        return pickedQuestions;
    }
}
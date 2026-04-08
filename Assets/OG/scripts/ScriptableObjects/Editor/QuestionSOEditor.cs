using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuestionSO))]
public class QuestionSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Update the serialized object
        serializedObject.Update();

        QuestionSO question = (QuestionSO)target;

        // --- NEW: ID Field with a Regenerate Button ---
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = false; // Lock the text field so it can't be typed in manually
        EditorGUILayout.PropertyField(serializedObject.FindProperty("QuestionID"));
        GUI.enabled = true;

        if (GUILayout.Button("Regenerate", GUILayout.Width(80)))
        {
            // Generate a fresh ID and force Unity to save the change
            question.QuestionID = System.Guid.NewGuid().ToString();
            EditorUtility.SetDirty(question);
        }

        EditorGUILayout.EndHorizontal();
        // ----------------------------------------------

        EditorGUILayout.Space(5);

        // Core Settings
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Difficulty"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Type"));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Question Text", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("QuestionTextEN"), new GUIContent("English"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("QuestionTextES"), new GUIContent("Spanish"));

        EditorGUILayout.Space(15);

        // Conditionally show fields based on the selected Type
        if (question.Type == QuestionSO.QuestionType.MultipleChoice)
        {
            EditorGUILayout.LabelField("Multiple Choice Answers", EditorStyles.boldLabel);

            // Correct Answer
            GUI.color = new Color(0.7f, 1f, 0.7f); // Light green
            EditorGUILayout.HelpBox("Correct Answer", MessageType.None);
            GUI.color = Color.white;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CorrectAnswerEN"), new GUIContent("EN"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CorrectAnswerES"), new GUIContent("ES"));
            EditorGUILayout.Space(5);

            // Incorrect Answers
            GUI.color = new Color(1f, 0.7f, 0.7f); // Light red
            EditorGUILayout.HelpBox("Incorrect Answer 1", MessageType.None);
            GUI.color = Color.white;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IncorrectAnswer1EN"), new GUIContent("EN"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IncorrectAnswer1ES"), new GUIContent("ES"));
            EditorGUILayout.Space(5);

            GUI.color = new Color(1f, 0.7f, 0.7f);
            EditorGUILayout.HelpBox("Incorrect Answer 2", MessageType.None);
            GUI.color = Color.white;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IncorrectAnswer2EN"), new GUIContent("EN"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IncorrectAnswer2ES"), new GUIContent("ES"));
            EditorGUILayout.Space(5);

            GUI.color = new Color(1f, 0.7f, 0.7f);
            EditorGUILayout.HelpBox("Incorrect Answer 3", MessageType.None);
            GUI.color = Color.white;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IncorrectAnswer3EN"), new GUIContent("EN"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IncorrectAnswer3ES"), new GUIContent("ES"));
        }
        else if (question.Type == QuestionSO.QuestionType.TrueFalse)
        {
            EditorGUILayout.LabelField("True / False Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IsTrueStatement"), new GUIContent("Is the statement True?"));
        }

        // Apply changes
        serializedObject.ApplyModifiedProperties();
    }
}
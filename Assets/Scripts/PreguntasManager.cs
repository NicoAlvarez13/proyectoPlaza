using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreguntasManager : MonoBehaviour
{
    // ... (Tus listas de categorías siguen igual) ...
    [Header("Categoría 1 (Ej: Aire)")]
    public List<GameObject> category1Canvases;
    [Header("Categoría 2 (Ej: Tierra)")]
    public List<GameObject> category2Canvases;
    [Header("Categoría 3 (Ej: Fuego)")]
    public List<GameObject> category3Canvases;
    [Header("Categoría 4 (Ej: Agua)")]
    public List<GameObject> category4Canvases;
    [Header("Categoría 5 (Ej: Bonus)")]
    public List<GameObject> category5Canvases;

    // --- Listas temporales ---
    private List<GameObject> availableCat1, availableCat2, availableCat3, availableCat4, availableCat5;
    private List<List<GameObject>> allAvailablePools;

    // --- Variables de Estado ---
    private int currentCategoryIndex = 0;
    private int questionsAskedFromCategory = 0;
    private const int QUESTIONS_PER_CATEGORY = 2;

    // NUEVO: Variable para recordar qué pregunta está en pantalla y poder borrarla
    private GameObject preguntaActualActiva;

    public void StartQuiz()
    {
        // Inicialización de listas (igual que antes)
        availableCat1 = new List<GameObject>(category1Canvases);
        availableCat2 = new List<GameObject>(category2Canvases);
        availableCat3 = new List<GameObject>(category3Canvases);
        availableCat4 = new List<GameObject>(category4Canvases);
        availableCat5 = new List<GameObject>(category5Canvases);

        allAvailablePools = new List<List<GameObject>> { availableCat1, availableCat2, availableCat3, availableCat4, availableCat5 };

        currentCategoryIndex = 0;
        questionsAskedFromCategory = 0;
        preguntaActualActiva = null; // Reseteamos

        HideAllCanvases();
        ShowNextQuestion();
    }

    public void ShowNextQuestion()
    {
        // 1. NUEVO: Si hay una pregunta mostrándose, la ocultamos primero
        if (preguntaActualActiva != null)
        {
            preguntaActualActiva.SetActive(false);
        }

        // --- Lógica de Cambio de Categoría ---
        if (questionsAskedFromCategory >= QUESTIONS_PER_CATEGORY)
        {
            currentCategoryIndex++;
            questionsAskedFromCategory = 0;
        }

        // --- Fin del Juego ---
        if (currentCategoryIndex >= allAvailablePools.Count)
        {
            Debug.Log("¡JUEGO TERMINADO!");
            // Aquí llamarías a tu pantalla final
            return;
        }

        // --- Selección de Pregunta ---
        List<GameObject> currentPool = allAvailablePools[currentCategoryIndex];

        if (currentPool.Count == 0)
        {
            Debug.LogError("Se acabaron las preguntas de la categoría " + (currentCategoryIndex + 1));
            // Forzar avance para evitar bloqueo
            questionsAskedFromCategory = QUESTIONS_PER_CATEGORY;
            ShowNextQuestion();
            return;
        }

        int randomIndex = Random.Range(0, currentPool.Count);
        GameObject nextQuestion = currentPool[randomIndex];
        currentPool.RemoveAt(randomIndex);

        // 2. Mostrar la nueva y guardarla como "Activa"
        nextQuestion.SetActive(true);
        preguntaActualActiva = nextQuestion;

        questionsAskedFromCategory++;
    }

    private void HideAllCanvases()
    {
        foreach (GameObject canvas in category1Canvases) if (canvas) canvas.SetActive(false);
        foreach (GameObject canvas in category2Canvases) if (canvas) canvas.SetActive(false);
        foreach (GameObject canvas in category3Canvases) if (canvas) canvas.SetActive(false);
        foreach (GameObject canvas in category4Canvases) if (canvas) canvas.SetActive(false);
        foreach (GameObject canvas in category5Canvases) if (canvas) canvas.SetActive(false);
    }
}

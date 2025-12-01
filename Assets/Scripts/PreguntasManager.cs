using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreguntasManager : MonoBehaviour
{
    // --- ¡CAMBIO! ---
    // Ahora tenemos 4 listas maestras, una para cada categoría.
    // Asumiré que tienes 12 canvases por categoría (total 48)
    [Header("Categoría 1 (Ej: Geografía)")]
    public List<GameObject> category1Canvases;

    [Header("Categoría 2 (Ej: Historia)")]
    public List<GameObject> category2Canvases;

    [Header("Categoría 3 (Ej: Ciencia)")]
    public List<GameObject> category3Canvases;

    [Header("Categoría 4 (Ej: Deportes)")]
    public List<GameObject> category4Canvases;

    [Header("Categoría 5 (Ej: Bonus)")]
    public List<GameObject> category5Canvases;


    // --- Listas de "disponibles" ---
    // También necesitamos 4 listas temporales
    private List<GameObject> availableCat1, availableCat2, availableCat3, availableCat4, availableCat5;

    // --- Variables de estado del juego ---
    private List<List<GameObject>> allAvailablePools; // Una lista que contiene las otras listas
    private int currentCategoryIndex = 0;             // 0=Cat1, 1=Cat2, 2=Cat3, 3=Cat4
    private int questionsAskedFromCategory = 0;       // Contador de preguntas por categoría
    private const int QUESTIONS_PER_CATEGORY = 2;     // ¡Constante mágica!


    // 1. ESTA FUNCIÓN LA LLAMA TU "CuentaRegresiva.cs"
    public void StartQuiz()
    {
        // Preparamos las 4 listas para la partida
        availableCat1 = new List<GameObject>(category1Canvases);
        availableCat2 = new List<GameObject>(category2Canvases);
        availableCat3 = new List<GameObject>(category3Canvases);
        availableCat4 = new List<GameObject>(category4Canvases);
        availableCat5 = new List<GameObject>(category5Canvases);

        // Creamos una "lista de listas" para acceder a ellas por índice
        allAvailablePools = new List<List<GameObject>>
        {
            availableCat1,
            availableCat2,
            availableCat3,
            availableCat4,
            availableCat5
        };

        // Reseteamos los contadores
        currentCategoryIndex = 0;
        questionsAskedFromCategory = 0;

        // Ocultamos TODOS los canvases de preguntas
        HideAllCanvases();

        // Mostramos la primera pregunta (que será de la Categoría 1)
        ShowNextQuestion();
    }

    // 2. ESTA FUNCIÓN LA LLAMAN LOS BOTONES DE RESPUESTA
    public void ShowNextQuestion()
    {
        // --- Lógica de Avance ---
        // ¿Ya hicimos las 2 preguntas de esta categoría?
        if (questionsAskedFromCategory >= QUESTIONS_PER_CATEGORY)
        {
            currentCategoryIndex++;               // Pasamos a la siguiente categoría
            questionsAskedFromCategory = 0;       // Reseteamos el contador
        }

        // --- Chequeo de Fin de Juego ---
        // ¿Ya pasamos la categoría 5 (índice 4)?
        if (currentCategoryIndex >= allAvailablePools.Count)
        {
            Debug.Log("¡JUEGO TERMINADO! Has completado todas las categorías.");
            // Aquí muestras el panel de "Fin del Juego" o "Puntaje Final"
            return;
        }

        // --- Selección de Pregunta ---

        // 1. Obtenemos el "mazo" de la categoría actual
        List<GameObject> currentPool = allAvailablePools[currentCategoryIndex];

        // 2. Chequeo de seguridad: ¿Hay preguntas en este mazo?
        if (currentPool.Count == 0)
        {
            // Esto no debería pasar si tienes 12 preguntas y solo pides 2,
            // pero es una buena práctica tenerlo.
            Debug.LogError("¡ERROR! Se acabaron las preguntas de la categoría " + (currentCategoryIndex + 1));
            // Forzamos el paso a la siguiente categoría
            questionsAskedFromCategory = QUESTIONS_PER_CATEGORY;
            ShowNextQuestion(); // Volvemos a llamar a la función
            return;
        }

        // 3. Elegir un Canvas al azar de ese mazo
        int randomIndex = Random.Range(0, currentPool.Count);
        GameObject nextQuestion = currentPool[randomIndex];

        // 4. Quitarlo del mazo para que no se repita
        currentPool.RemoveAt(randomIndex);

        // 5. ¡Mostrarlo!
        nextQuestion.SetActive(true);

        // 6. Actualizar el contador
        questionsAskedFromCategory++;
    }

    // Función de utilidad para ocultar todo al inicio
    private void HideAllCanvases()
    {
        // Oculta todos los canvases de todas las listas maestras
        foreach (GameObject canvas in category1Canvases) canvas.SetActive(false);
        foreach (GameObject canvas in category2Canvases) canvas.SetActive(false);
        foreach (GameObject canvas in category3Canvases) canvas.SetActive(false);
        foreach (GameObject canvas in category4Canvases) canvas.SetActive(false);
        foreach (GameObject canvas in category5Canvases) canvas.SetActive(false);
    }
}

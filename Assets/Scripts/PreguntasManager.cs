using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreguntasManager : MonoBehaviour
{
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

    // --- YA NO NECESITAS LA REFERENCIA AL TEMPORIZADOR AQUÍ ---
    // El temporizador vive dentro de cada Canvas y funciona automático.

    // --- Listas temporales ---
    private List<GameObject> availableCat1, availableCat2, availableCat3, availableCat4, availableCat5;
    private List<List<GameObject>> allAvailablePools;

    // --- Variables de Estado ---
    private int currentCategoryIndex = 0;
    private int questionsAskedFromCategory = 0;
    private const int QUESTIONS_PER_CATEGORY = 2;
    private GameObject preguntaActualActiva;

    public void StartQuiz()
    {
        // Inicialización de listas
        availableCat1 = new List<GameObject>(category1Canvases);
        availableCat2 = new List<GameObject>(category2Canvases);
        availableCat3 = new List<GameObject>(category3Canvases);
        availableCat4 = new List<GameObject>(category4Canvases);
        availableCat5 = new List<GameObject>(category5Canvases);

        allAvailablePools = new List<List<GameObject>> { availableCat1, availableCat2, availableCat3, availableCat4, availableCat5 };

        currentCategoryIndex = 0;
        questionsAskedFromCategory = 0;
        preguntaActualActiva = null;

        HideAllCanvases();
        ShowNextQuestion();
    }

    public void ShowNextQuestion()
    {
        // 1. Si había una pregunta activa, la apagamos.
        // ALERTA: Al hacer SetActive(false), el script del temporizador
        // dentro de ese canvas se apaga solo (gracias a OnDisable).
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
            // Aquí iría tu lógica de fin de juego (pantalla de puntaje, etc.)
            return;
        }

        // --- Selección de Pregunta ---
        List<GameObject> currentPool = allAvailablePools[currentCategoryIndex];

        if (currentPool.Count == 0)
        {
            // Manejo de error si se acaban las preguntas (evitar bloqueo)
            questionsAskedFromCategory = QUESTIONS_PER_CATEGORY;
            ShowNextQuestion();
            return;
        }

        int randomIndex = Random.Range(0, currentPool.Count);
        GameObject nextQuestion = currentPool[randomIndex];
        currentPool.RemoveAt(randomIndex);

        // 2. Activamos la nueva pregunta.
        // ALERTA: Al hacer SetActive(true), el script del temporizador
        // dentro de este canvas se enciende y arranca solo (gracias a OnEnable).
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

    // --- ESTA FUNCIÓN ES LA QUE LLAMAN LOS TEMPORIZADORES ---
    public void RespuestaIncorrectaPorTiempo()
    {
        Debug.Log("Se acabó el tiempo - Respuesta Incorrecta");

        // Opcional: Sonido de error
        // audioSource.PlayOneShot(sonidoError);

        StartCoroutine(EsperarYAvanzar());
    }

    private System.Collections.IEnumerator EsperarYAvanzar()
    {
        // Esperamos 2 segundos para que el jugador vea el texto rojo o se lamente
        yield return new WaitForSeconds(2f);

        // Pasamos a la siguiente
        ShowNextQuestion();
    }
}

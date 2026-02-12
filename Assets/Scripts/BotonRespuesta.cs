using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class BotonRespuesta : MonoBehaviour
{
    [Header("Configuración Manual")]
    // Hacemos esto público para que puedas marcar la casilla en el Inspector
    public bool esLaRespuestaCorrecta = false;

    [Header("Referencias Visuales")]
    public Image imagenDelBorde;
    public Button miBoton;

    [Header("Configuración de Colores")]
    public Color colorCorrecto = new Color(0.1f, 0.8f, 0.1f);
    public Color colorIncorrecto = new Color(0.9f, 0.1f, 0.1f);
    private Color colorOriginal;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sonidoAcierto;
    public AudioClip sonidoError;

    [Header("Tiempos")]
    public float tiempoDeEspera = 1.0f; // Tiempo para ver el color antes de cambiar

    private PreguntasManager preguntasManager; // Referencia al manager

    void Awake()
    {
        if (imagenDelBorde != null) colorOriginal = imagenDelBorde.color;

        // Buscamos el PreguntasManager automáticamente en la escena
        preguntasManager = FindFirstObjectByType<PreguntasManager>();
    }

    // Este método lo vinculas al evento OnClick del botón en Unity
    public void AlHacerClick()
    {
        // 1. Bloqueamos el botón para evitar doble click
        if (miBoton != null) miBoton.interactable = false;

        // --- CORRECCIÓN AQUÍ ---
        // En lugar de llamar a preguntasManager.DetenerReloj()...
        // Buscamos el temporizador que está ACTIVO en la pantalla ahora mismo.
        // Como solo hay un canvas de pregunta activo, FindFirstObjectByType encontrará el correcto.

        TemporizadorIndividual timerActivo = Object.FindFirstObjectByType<TemporizadorIndividual>();

        if (timerActivo != null)
        {
            timerActivo.DetenerReloj();
        }
        // -----------------------

        // 3. Iniciamos la animación de feedback
        StartCoroutine(SecuenciaFeedback());
    }

    IEnumerator SecuenciaFeedback()
    {
        // --- Feedback Visual y Sonoro ---
        if (esLaRespuestaCorrecta)
        {
            imagenDelBorde.color = colorCorrecto;
            if (audioSource && sonidoAcierto) audioSource.PlayOneShot(sonidoAcierto);
            Debug.Log("Respuesta Correcta");
            // Aquí podrías sumar puntos
        }
        else
        {
            imagenDelBorde.color = colorIncorrecto;
            if (audioSource && sonidoError) audioSource.PlayOneShot(sonidoError);
            Debug.Log("Respuesta Incorrecta");
        }

        // --- Espera ---
        yield return new WaitForSeconds(tiempoDeEspera);

        // --- Avanzar ---
        // Llamamos a la función de TU script PreguntasManager
        if (preguntasManager != null)
        {
            preguntasManager.ShowNextQuestion();
        }
        else
        {
            Debug.LogError("No encontré el script PreguntasManager en la escena");
        }

        // Reset (por si reutilizas la escena sin recargar)
        imagenDelBorde.color = colorOriginal;
        miBoton.interactable = true;
    }
}

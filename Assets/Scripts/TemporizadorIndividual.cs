using UnityEngine;
using TMPro;

public class TemporizadorIndividual : MonoBehaviour
{
    [Header("Configuración")]
    public float tiempoInicial = 15f;
    public Color colorNormal = Color.black;
    public Color colorAlerta = Color.red;

    [Header("Referencias")]
    public TextMeshProUGUI textoDisplay; // El texto que cambia números
    public PreguntasManager miManager;   // A quién avisar si pierdo

    private float tiempoRestante;
    private bool estaCorriendo = false;

    // --- MAGIA DE UNITY ---
    // Esta función se ejecuta AUTOMÁTICAMENTE cada vez que
    // haces SetActive(true) en el Canvas o en este objeto.
    void OnEnable()
    {
        ReiniciarYArrancar();
    }

    // Esta se ejecuta cuando el objeto se apaga SetActive(false)
    void OnDisable()
    {
        estaCorriendo = false;
        // Al desactivarse, el timer deja de consumir recursos automáticamente
    }

    void ReiniciarYArrancar()
    {
        tiempoRestante = tiempoInicial;
        estaCorriendo = true;

        if (textoDisplay != null)
            textoDisplay.color = colorNormal;
    }

    void Update()
    {
        // Si el canvas está apagado o pausado, no hacemos nada
        if (!estaCorriendo) return;

        tiempoRestante -= Time.deltaTime;

        // Actualizamos visuales
        ActualizarTexto(tiempoRestante);

        // Chequeo de fin de tiempo
        if (tiempoRestante <= 0)
        {
            tiempoRestante = 0;
            estaCorriendo = false; // Frenamos para no spammear
            ActualizarTexto(0);

            Debug.Log("Tiempo agotado en pregunta actual");

            // Avisamos al Manager
            if (miManager != null)
            {
                miManager.RespuestaIncorrectaPorTiempo();
            }
            else
            {
                // Intento de emergencia por si olvidaste arrastrar el manager
                FindFirstObjectByType<PreguntasManager>().RespuestaIncorrectaPorTiempo();
            }
        }
    }

    public void DetenerReloj()
    {
        estaCorriendo = false;
    }

    void ActualizarTexto(float tiempo)
    {
        if (textoDisplay == null) return;

        float segundos = Mathf.FloorToInt(tiempo % 60);
        float milisegundos = (tiempo * 100) % 100;

        textoDisplay.text = string.Format("{0:00}:{1:00}", segundos, milisegundos);

        if (tiempo <= 5f)
        {
            textoDisplay.color = colorAlerta;
        }
    }
}

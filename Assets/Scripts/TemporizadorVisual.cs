using UnityEngine;
using TMPro;

public class TemporizadorVisual : MonoBehaviour
{
    [Header("Configuración UI")]
    public TextMeshProUGUI textoCronometro; // Arrastra aquí el texto 00:00
    public float tiempoMaximo = 15f;

    [Header("Referencia al Manager")]
    // Esta referencia es clave: le avisaremos a ESTE manager cuando se acabe el tiempo
    public PreguntasManager miManager;

    private float tiempoActual;
    private bool contando = false;

    void Update()
    {
        if (contando)
        {
            tiempoActual -= Time.deltaTime;

            if (tiempoActual > 0)
            {
                ActualizarTexto(tiempoActual);
            }
            else
            {
                // SE ACABÓ EL TIEMPO
                tiempoActual = 0;
                ActualizarTexto(0);
                contando = false;

                // Avisamos al manager que asignaste en el inspector
                if (miManager != null)
                {
                    miManager.RespuestaIncorrectaPorTiempo();
                }
            }
        }
    }

    public void IniciarReloj()
    {
        tiempoActual = tiempoMaximo;
        contando = true;
        textoCronometro.color = Color.black; // Resetear color
        ActualizarTexto(tiempoActual);
    }

    public void DetenerReloj()
    {
        contando = false;
    }

    void ActualizarTexto(float tiempo)
    {
        // Segundos y Milisegundos
        float segundos = Mathf.FloorToInt(tiempo % 60);
        float milisegundos = (tiempo * 100) % 100;

        textoCronometro.text = string.Format("{0:00}:{1:00}", segundos, milisegundos);

        // Feedback Rojo cuando queda poco tiempo
        if (tiempo <= 5f)
        {
            textoCronometro.color = Color.red;
        }
    }
}

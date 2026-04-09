using UnityEngine;
using TMPro;

public class TemporizadorPregunta : MonoBehaviour
{
    [Header("Configuración")]
    public float tiempoPorPregunta = 15f;
    public TextMeshProUGUI textoCronometro; // Arrastra aquí el texto de UI (00:00)

    [Header("Referencias")]
    public GameManager gameManager; // Arrastra aquí tu objeto que tiene el GameManager

    private float tiempoRestante;
    private bool contando = false;

    void Update()
    {
        if (contando)
        {
            tiempoRestante -= Time.deltaTime;

            if (tiempoRestante > 0)
            {
                ActualizarTextoTimer(tiempoRestante);
            }
            else
            {
                // SE ACABÓ EL TIEMPO
                tiempoRestante = 0;
                ActualizarTextoTimer(0);
                TiempoAgotado();
            }
        }
    }

    public void IniciarCuenta()
    {
        tiempoRestante = tiempoPorPregunta;
        contando = true;
        textoCronometro.color = Color.white; // Color normal
    }

    public void DetenerCuenta()
    {
        contando = false;
    }

    void ActualizarTextoTimer(float tiempo)
    {
        // Calcula segundos y milisegundos (2 dígitos)
        float segundos = Mathf.FloorToInt(tiempo % 60);
        float milisegundos = (tiempo * 100) % 100;

        // Formato 15:00
        textoCronometro.text = string.Format("{0:00}:{1:00}", segundos, milisegundos);

        // Feedback visual: Poner rojo si faltan menos de 5 segundos
        if (tiempo <= 5f)
        {
            textoCronometro.color = Color.red;
        }
    }

    void TiempoAgotado()
    {
        contando = false;
        Debug.Log("¡Tiempo Agotado!");

        // Llamamos a la función especial que crearemos en el paso 2
        //gameManager.RespuestaIncorrectaPorTiempo();
    }
}

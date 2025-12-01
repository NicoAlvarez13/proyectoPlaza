using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CategoriaController : MonoBehaviour
{
    [Header("Canvas de preguntas de esta categoría (12 en total)")]
    public GameObject[] canvasPreguntas; // Asignar en el Inspector

    private List<int> preguntasMostradas = new List<int>();
    private int preguntasMostradasContador = 0;
    private int ultimaPreguntaActiva = -1;

    void Start()
    {
        // Ocultar todas al inicio
        foreach (var canvas in canvasPreguntas)
            canvas.SetActive(false);

        // Mostrar la primera pregunta (Canvas 0)
        MostrarPregunta(0);
    }

    public void ResponderPregunta() // Este método lo llamas desde los botones de respuesta
    {
        preguntasMostradasContador++;

        // Desactivar la anterior
        if (ultimaPreguntaActiva != -1)
            canvasPreguntas[ultimaPreguntaActiva].SetActive(false);

        // Si ya se respondieron 4 preguntas, fin
        if (preguntasMostradasContador >= 4)
        {
            Debug.Log("Fin de las 4 preguntas.");
            return;
        }

        // Elegir una nueva pregunta al azar que no se haya mostrado
        int siguienteIndex = ObtenerPreguntaAleatoriaDisponible();

        if (siguienteIndex == -1)
        {
            Debug.Log("No quedan preguntas disponibles.");
            return;
        }

        MostrarPregunta(siguienteIndex);
    }

    private void MostrarPregunta(int index)
    {
        canvasPreguntas[index].SetActive(true);
        preguntasMostradas.Add(index);
        ultimaPreguntaActiva = index;
    }

    private int ObtenerPreguntaAleatoriaDisponible()
    {
        List<int> indicesDisponibles = new List<int>();

        for (int i = 0; i < canvasPreguntas.Length; i++)
        {
            if (!preguntasMostradas.Contains(i))
                indicesDisponibles.Add(i);
        }

        if (indicesDisponibles.Count == 0)
            return -1;

        int randomIndex = Random.Range(0, indicesDisponibles.Count);
        return indicesDisponibles[randomIndex];
    }
}

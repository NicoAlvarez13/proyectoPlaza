using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List <selectorPersonajePRUEBA> personajes;

    private void Awake()
    {
        if(GameManager.Instance == null)
        {
            GameManager.Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RespuestaIncorrectaPorTiempo()
    {
        Debug.Log("¡Se acabó el tiempo!");

        // AQUI ES DONDE NECESITAS CONECTAR CON TU LÓGICA DE AVANZAR
        // Como no tengo tu código completo, tienes que poner aquí la misma 
        // línea que usas cuando alguien responde mal.

        // OPCIÓN A: Si usas escenas para cada pregunta:
        // UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);

        // OPCIÓN B: Si tienes una función para generar preguntas:
        // GenerarPregunta(); 

        // OPCIÓN C: Si tu lógica está en los botones, quizás tengas una función pública aquí:
        // ShowNextQuestion();
    }
}

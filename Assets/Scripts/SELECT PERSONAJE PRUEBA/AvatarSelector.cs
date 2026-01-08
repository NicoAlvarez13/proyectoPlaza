using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarSelector : MonoBehaviour
{
    [Header("Configuración UI")]
    public Image avatarDisplay;          // La imagen central que cambia
    public GameObject panelSeleccion;    // El panel donde elegimos
    public GameObject panelEspera;       // El panel de "Esperando al guía"

    [Header("Datos de Avatares")]
    public Sprite[] avataresDisponibles; // Arrastra aquí tus 4 sprites (0 a 3)

    private int indiceActual = 0;        // Para saber cuál estamos viendo

    void Start()
    {
        // Al iniciar, mostramos el primer avatar y aseguramos que los paneles estén correctos
        ActualizarImagen();
        panelSeleccion.SetActive(true);
        panelEspera.SetActive(false);
    }

    // Función para el botón FLECHA DERECHA
    public void CambiarSiguiente()
    {
        indiceActual++;

        // Si nos pasamos del último, volvemos al primero (Loop)
        if (indiceActual >= avataresDisponibles.Length)
        {
            indiceActual = 0;
        }

        ActualizarImagen();
    }

    // Función para el botón FLECHA IZQUIERDA
    public void CambiarAnterior()
    {
        indiceActual--;

        // Si bajamos de 0, vamos al último de la lista
        if (indiceActual < 0)
        {
            indiceActual = avataresDisponibles.Length - 1;
        }

        ActualizarImagen();
    }

    // Actualiza la UI con el sprite correspondiente al índice actual
    private void ActualizarImagen()
    {
        if (avataresDisponibles.Length > 0)
        {
            avatarDisplay.sprite = avataresDisponibles[indiceActual];
        }
    }

    // Función para el botón CONFIRMAR / LISTO
    public void ConfirmarSeleccion()
    {
        // 1. Guardar la elección. 
        // Como es un juego online, aquí guardarías el dato para enviarlo luego por red.
        // Por ahora, lo guardamos en una variable estática o PlayerPrefs para usarlo en la partida.
        PlayerPrefs.SetInt("AvatarSeleccionado", indiceActual);

        Debug.Log("Avatar seleccionado ID: " + indiceActual);

        // 2. Cambiar de interfaz
        panelSeleccion.SetActive(false);
        panelEspera.SetActive(true);

        // AQUÍ IRÍA TU LÓGICA DE RED:
        // Ejemplo: NetworkManager.EnviarMensaje("JugadorListo", indiceActual);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swimming : MonoBehaviour
{
    public Vector2 pointA;
    public Vector2 pointB;
    public float horizontalSpeed = 2f;

    public float floatAmplitude = 0.5f; // cuánto sube y baja
    public float floatFrequency = 2f;   // qué tan rápido sube y baja

    private Vector2 startPos;
    private float journeyLength;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
        startPos = transform.position;
        journeyLength = Vector2.Distance(pointA, pointB);
    }

    void Update()
    {
        // Movimiento horizontal ida y vuelta (PingPong)
        float t = Mathf.PingPong((Time.time - startTime) * horizontalSpeed, 1f);
        Vector2 horizontalPos = Vector2.Lerp(pointA, pointB, t);

        // Movimiento vertical tipo flotación
        float floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        Vector2 floatPos = new Vector2(horizontalPos.x, horizontalPos.y + floatOffset);

        // Aplicar posición combinada
        transform.position = floatPos;
    }
}

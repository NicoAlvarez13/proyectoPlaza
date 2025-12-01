using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balanceo : MonoBehaviour
{
    [Header("Balanceo")]
    [Tooltip("Amplitud en grados")]
    public float amplitude = 10f;
    [Tooltip("Ciclos por segundo")]
    public float speed = 0.8f;
    [Tooltip("Desfase temporal opcional")]
    public float timeOffset = 0f;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        float angle = Mathf.Sin((Time.time + timeOffset) * Mathf.PI * 2f * speed) * amplitude;
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }
}

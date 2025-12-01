using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharcterFloating : MonoBehaviour
{
    public float amplitude = 10f; // Altura del movimiento
    public float frequency = 1f;  // Velocidad del movimiento

    private Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float y = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = startPos + new Vector3(0, y, 0);
    }
}

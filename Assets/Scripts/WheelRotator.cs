// File: WheelRotator.cs
using UnityEngine;

public class WheelRotator : MonoBehaviour
{
    [Tooltip("Speed of rotation in degrees per second.")]
    public float rotationSpeed = 100f; // Kecepatan putar default, bisa diatur di Inspector

    void Update()
    {
        // Melakukan rotasi di sekitar sumbu Z (axis ke luar layar untuk 2D)
        // Time.deltaTime memastikan rotasi tetap sama di berbagai frame rate
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
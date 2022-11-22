using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class NavigationPoint
{
    [SerializeField] private float latitude = 0.0f;
    [SerializeField] private float longitude = 0.0f;
    [SerializeField] private int accuracy = 5;

    public float Latitude { get => latitude; set => latitude = value; }
    public float Longitude { get => longitude; set => longitude = value; }
    public int Accuracy { get => accuracy; set => accuracy = value; }
}

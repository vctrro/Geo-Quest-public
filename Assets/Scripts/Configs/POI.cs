using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class POI
{
    [SerializeField] private string name = "";
    [SerializeField] private string imageUrl = "";
    [SerializeField] private string color = "";
    [SerializeField] private NavigationPoint coordinates;

    public string Name { get => name; set => name = value; }
    public string ImageUrl { get => imageUrl; set => imageUrl = value; }
    public string Color { get => color; set => color = value; }
    public NavigationPoint Coordinates { get => coordinates; set => coordinates = value; }
}
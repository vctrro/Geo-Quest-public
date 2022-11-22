using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class Map
{
    [SerializeField] private string mapImageUrl = "";
    [SerializeField] private float mapScale = 100;
    [SerializeField] private NavigationPoint leftCornerCoords;
    [SerializeField] private POI[] pOIs;

    public string MapImageUrl { get => mapImageUrl; set => mapImageUrl = value; }
    public float MapScale { get => mapScale; set => mapScale = value; }
    public NavigationPoint LeftCornerCoords { get => leftCornerCoords; set => leftCornerCoords = value; }
    public POI[] POIs { get => pOIs; set => pOIs = value; }
}
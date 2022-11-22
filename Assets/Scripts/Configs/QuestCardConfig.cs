using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class QuestCardConfig
{
    [SerializeField] private string id = "";
    [SerializeField] private string name = "";
    [SerializeField] private bool timerGame = false;
    [SerializeField] private string description = "";
    [SerializeField] private string fullDescription = "";
    [SerializeField] private string age = "";
    [SerializeField] private int complexity = 0;
    [SerializeField] private float approximateTime = 0.0f; //in hour
    [SerializeField] private float approximateDistance; //in km
    [SerializeField] private string logoUrl = "";
    [SerializeField] private string fullVersionUrl = "";

    public string Id { get => id; set => id = value; }
    public string Name { get => name; set => name = value; }
    public bool TimerGame { get => timerGame; set => timerGame = value; }
    public string Description { get => description; set => description = value; }
    public string FullDescription { get => fullDescription; set => fullDescription = value; }
    public string Age { get => age; set => age = value; }
    public int Complexity { get => complexity; set => complexity = value; }
    public float ApproximateTime { get => approximateTime; set => approximateTime = value; }
    public float ApproximateDistance { get => approximateDistance; set => approximateDistance = value; }
    public string LogoUrl { get => logoUrl; set => logoUrl = value; }
    public string FullVersionUrl { get => fullVersionUrl; set => fullVersionUrl = value; }
}

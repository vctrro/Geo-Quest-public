using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable] public enum QuestMode
{
    Route,
    Game
}
[System.Serializable] public class QuestConfig
{
    [SerializeField] private string questId = "";
    [SerializeField] private string name = "";
    [SerializeField] private QuestMode questMode;
    [SerializeField] private bool timerGame = false;
    [SerializeField] private string description = "";
    [SerializeField] private string fullDescription = "";
    [SerializeField] private string descriptionImageUrl = "";
    [SerializeField] private int ageMin = 3;
    [SerializeField] private int ageMax = 120;
    [SerializeField] private int complexity = 0;
    [SerializeField] private float approximateTime = 0.0f; //in hour
    [SerializeField] private float approximateDistance = 0.0f; //in km
    [SerializeField] private float[] startPosition = new float[0];
    [SerializeField] private string logoUrl = "";
    [SerializeField] private List<string> tags;
    [SerializeField] private string finishTitle = "Игра окончена!";
    [SerializeField] private Map map = new Map();
    [SerializeField] private StepConfig[] steps = {new StepConfig()};
    [SerializeField] private QuestStatistic questStatistic;

    public string QuestId { get => questId; set => questId = value; }
    public string Name { get => name; set => name = value; }
    public QuestMode QuestMode { get => questMode; set => questMode = value; }
    public bool TimerGame { get => timerGame; set => timerGame = value; }
    public string Description { get => description; set => description = value; }
    public string FullDescription { get => fullDescription; set => fullDescription = value; }
    public string DescriptionImageUrl { get => descriptionImageUrl; set => descriptionImageUrl = value; }
    public int AgeMin { get => ageMin; set => ageMin = value; }
    public int AgeMax { get => ageMax; set => ageMax = value; }
    public int Complexity { get => complexity; set => complexity = value; }
    public float ApproximateTime { get => approximateTime; set => approximateTime = value; }
    public float ApproximateDistance { get => approximateDistance; set => approximateDistance = value; }
    public float[] StartPosition { get => startPosition; set => startPosition = value; }
    public string LogoUrl { get => logoUrl; set => logoUrl = value; }
    public List<string> Tags { get => tags; set => tags = value; }
    public string FinishTitle { get => finishTitle; set => finishTitle = value; }
    public Map Map { get => map; set => map = value; }
    public StepConfig[] Steps { get => steps; set => steps = value; }
    public QuestStatistic QuestStatistic { get => questStatistic; set => questStatistic = value; }
}

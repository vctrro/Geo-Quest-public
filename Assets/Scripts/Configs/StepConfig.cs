using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public enum StepMode
{
    Info,
    Navigation,
    Question
}

[System.Serializable] public class StepConfig
{
    [SerializeField] private int stepId = 0;
    [SerializeField] private StepMode stepMode;
    [SerializeField] private string name = "";
    [SerializeField] private string description = "";
    [SerializeField] private string buttonCaption = "";
    [SerializeField] private string imageUrl = "";
    [SerializeField] private string audioUrl = "";
    [SerializeField] private int timeToFinish = 0;
    [SerializeField] private int pointsForTime = 0;
    [SerializeField] private int penaltyPointsForTime = 0;
    [SerializeField] private bool skipEnabled = false;
    [SerializeField] private int penaltyPointsForSkip = 0;
    [SerializeField] private bool finish = false;
    [SerializeField] private int nextStep = 0;
    [SerializeField] private NavigationPoint[] navigationRoute = {new NavigationPoint()};
    [SerializeField] private QuestionConfig questionConfig;
    [SerializeField] private bool stepStarting = false;
    [SerializeField] private string startTimerTime;
    [SerializeField] private int currentNavPoint = 0;

    public int StepId { get => stepId; set => stepId = value; }
    public StepMode StepMode { get => stepMode; set => stepMode = value; }
    public string Name { get => name; set => name = value; }
    public string Description { get => description; set => description = value; }
    public string ButtonCaption { get => buttonCaption; set => buttonCaption = value; }
    public string ImageUrl { get => imageUrl; set => imageUrl = value; }
    public string AudioUrl { get => audioUrl; set => audioUrl = value; }
    public int TimeToFinish { get => timeToFinish; set => timeToFinish = value; }
    public int PointsForTime { get => pointsForTime; set => pointsForTime = value; }
    public int PenaltyPointsForTime { get => penaltyPointsForTime; set => penaltyPointsForTime = value; }
    public bool SkipEnabled { get => skipEnabled; set => skipEnabled = value; }
    public int PenaltyPointsForSkip { get => penaltyPointsForSkip; set => penaltyPointsForSkip = value; }
    public NavigationPoint[] NavigationRoute { get => navigationRoute; set => navigationRoute = value; }
    public QuestionConfig QuestionConfig { get => questionConfig; set => questionConfig = value; }
    public bool StepStarting { get => stepStarting; set => stepStarting = value; }
    public string StartTimerTime { get => startTimerTime; set => startTimerTime = value; }
    public bool Finish { get => finish; set => finish = value; }
    public int NextStep { get => nextStep; set => nextStep = value; }
    public int CurrentNavPoint { get => currentNavPoint; set => currentNavPoint = value; }
}

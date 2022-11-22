using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class QuestStatistic
{
    [SerializeField] private int currentStep = 0;
    [SerializeField] private int points = 0;
    [SerializeField] private int wrongAnswers = 0;
    [SerializeField] private string startTime = "";
    [SerializeField] private string finishTime = "";
    [SerializeField] private float spentTime = 0;
    [SerializeField] private int completedSteps = 0;
    [SerializeField] private int skippedSteps = 0;
    [SerializeField] private bool finish = false;

    public int CurrentStep { get => currentStep; set => currentStep = value; }
    public int Points { get => points; set => points = value; }
    public int WrongAnswers { get => wrongAnswers; set => wrongAnswers = value; }
    public string FinishTime { get => finishTime; set => finishTime = value; }
    public string StartTime { get => startTime; set => startTime = value; }
    public float SpentTime { get => spentTime; set => spentTime = value; }
    public int CompletedSteps { get => completedSteps; set => completedSteps = value; }
    public int SkippedSteps { get => skippedSteps; set => skippedSteps = value; }
    public bool Finish { get => finish; set => finish = value; }
}

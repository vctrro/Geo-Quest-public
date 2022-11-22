using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public enum AnswerMode
{
    Text,
    Choice,
    Table,
    Branch
}

[System.Serializable] public class QuestionConfig
{
    [SerializeField] private AnswerMode answerMode;
    [SerializeField] private int points = 0;
    [SerializeField] private int penaltyPoints = 0;
    //Text answer
    [SerializeField] private List<string> textCorrectAnswers;
    [SerializeField] private List<int> choiceCorrectAnswers;
    //Choice and branch answer
    [SerializeField] private string[] valuesOfVariants;
    [SerializeField] private int[] linksToGo;
    [SerializeField] private int[] pointsOfVariants;
    [SerializeField] private int[] penaltyPointsOfVariants;
    //Table answer
    [SerializeField] private List<int> tableCorrectAnswers;
    [SerializeField] private int numberOfColumns = 0;
    [SerializeField] private string[] valuesOfColumns;
    [SerializeField] private int numberOfRows = 0;
    [SerializeField] private string[] valuesOfRows;

    public AnswerMode AnswerMode { get => answerMode; set => answerMode = value; }
    public int Points { get => points; set => points = value; }
    public int PenaltyPoints { get => penaltyPoints; set => penaltyPoints = value; }
    public List<string> TextCorrectAnswers { get => textCorrectAnswers; set => textCorrectAnswers = value; }
    public List<int> ChoiceCorrectAnswers { get => choiceCorrectAnswers; set => choiceCorrectAnswers = value; }
    public string[] ValuesOfVariants { get => valuesOfVariants; set => valuesOfVariants = value; }
    public int[] LinksToGo { get => linksToGo; set => linksToGo = value; }
    public int[] PointsOfVariants { get => pointsOfVariants; set => pointsOfVariants = value; }
    public int[] PenaltyPointsOfVariants { get => penaltyPointsOfVariants; set => penaltyPointsOfVariants = value; }
    public List<int> TableCorrectAnswers { get => tableCorrectAnswers; set => tableCorrectAnswers = value; }
    public int NumberOfColumns { get => numberOfColumns; set => numberOfColumns = value; }
    public string[] ValuesOfColumns { get => valuesOfColumns; set => valuesOfColumns = value; }
    public int NumberOfRows { get => numberOfRows; set => numberOfRows = value; }
    public string[] ValuesOfRows { get => valuesOfRows; set => valuesOfRows = value; }
}

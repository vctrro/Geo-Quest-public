using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] public class GameConfig
{
    [SerializeField] private string userName = "Искатель";
    [SerializeField] private int userAge = 6;
    [SerializeField] private string currentSection;
    [SerializeField] private string currentQuest;

    public string UserName { get => userName; set => userName = value; }
    public int UserAge { get => userAge; set => userAge = value; }
    public string CurrentSection { get => currentSection; set => currentSection = value; }
    public string CurrentQuest { get => currentQuest; set => currentQuest = value; }
}

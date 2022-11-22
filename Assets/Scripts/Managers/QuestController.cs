using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using TMPro;

public class QuestController : MonoBehaviour
{
    [NonSerialized] public QuestConfig questConfig;
    [SerializeField] private GameObject stepPanel;
    [SerializeField] private GameObject infoPrefab;
    [SerializeField] private GameObject navigationPrefab;
    [SerializeField] private GameObject questionPrefab;
    private GameManager gameManager;
    private GameConfig gameConfig;
    private QuestStatistic questStatistic;

    private void Awake()
    {
        gameManager = GameManager.Instance;     
        gameConfig = gameManager.GameConfig;
        if (PlayerPrefs.HasKey($"{gameConfig.CurrentQuest}"))
        {
            questConfig = JsonUtility.FromJson<QuestConfig>(PlayerPrefs.GetString($"{gameConfig.CurrentQuest}"));
        }
        else
        {
            questConfig = new QuestConfig();
        }
        questStatistic = questConfig.QuestStatistic;
    }
    private void Start()
    {
        if (questStatistic.StartTime == "") questStatistic.StartTime = System.DateTime.Now.ToString();

        // Debug.Log($"{gameConfig.CurrentSection}-{gameConfig.CurrentQuest}-{questStatistic.CurrentStep} : {questStatistic.StartTime}");
        // Debug.Log(PlayerPrefs.GetString($"{gameConfig.CurrentQuest}"));
        LoadStep();
    }

    private void LoadStep()
    {
        var stepMode = questConfig.Steps[questStatistic.CurrentStep].StepMode;
        switch (stepMode)
        {
            case StepMode.Info:
                GameObject info = Instantiate(infoPrefab, stepPanel.transform);
                // info.name = infoPrefab.name;
                break;
            case StepMode.Navigation:
                GameObject navi = Instantiate(navigationPrefab, stepPanel.transform);
                // navi.name = navigationPrefab.name;
                break;
            case StepMode.Question:
                GameObject ques = Instantiate(questionPrefab, stepPanel.transform);
                // ques.name = questionPrefab.name;
                break;
        }
    }

    public void NextStep()
    {
        StartCoroutine(LoadNextStep());
    }

    private IEnumerator LoadNextStep()
    {
        stepPanel.GetComponent<Animator>().Play("StepPanelUp");
        yield return new WaitForSeconds(0.6f);
        Destroy(stepPanel.transform.GetChild(0).gameObject);
        yield return new WaitForSeconds(0.3f);
        
        if (questStatistic.Finish || questStatistic.CurrentStep >= questConfig.Steps.Length)
        {
            Finish();
            yield break;
        }
        LoadStep();
    }

    private void Finish()
    {
        var win = GameObject.Find("Canvas").transform.Find("Win");
        win.GetChild(0).GetComponent<TextMeshProUGUI>().text = questConfig.FinishTitle;

        questStatistic.FinishTime = System.DateTime.Now.ToString();
        
        win.GetChild(1).GetComponentInChildren<TextMeshProUGUI>().text = PrintStatistic(questStatistic);
        //Save statistic
        // QuestStatistic statisticToSave;
        // if (PlayerPrefs.HasKey($"Stat-{gameConfig.CurrentQuest}"))
        // {
        //     statisticToSave = JsonUtility.FromJson<QuestStatistic>(PlayerPrefs.GetString($"Stat-{gameConfig.CurrentQuest}"));
        // }
        // else
        // {
        //     statisticToSave = questStatistic;
        // }
        PlayerPrefs.SetString($"Stat-{questConfig.QuestId}", JsonUtility.ToJson(questStatistic));
        //Clear statistic
        questConfig.QuestStatistic = new QuestStatistic();

        win.gameObject.SetActive(true);
    }

    private string PrintStatistic(QuestStatistic qStat)
    {
        string resultString;
        var timeFromStart = System.DateTime.Parse(qStat.FinishTime) - System.DateTime.Parse(qStat.StartTime);

        string dayFromStart = "", hourFromStart = "", minFromStart = "", secFromStart = "";
        if (timeFromStart.Days > 0) dayFromStart = $"{timeFromStart.Days} д. ";
        if (timeFromStart.Hours > 0) hourFromStart = $"{timeFromStart.Hours} ч. ";
        if (timeFromStart.Minutes > 0) minFromStart = $"{timeFromStart.Minutes} мин. ";
        if (timeFromStart.Seconds > 0) secFromStart = $"{timeFromStart.Seconds} сек. ";

        string dayElapsed = "", hourElapsed = "", minElapsed = "", secElapsed = "";
        int spentDays = (int)(qStat.SpentTime/86400), spentHours = (int)((qStat.SpentTime%86400)/3600), 
            spentMinutes = (int)(((qStat.SpentTime%86400)%3600)/60), spentSeconds = (int)(((qStat.SpentTime%86400)%3600)%60);
        if (spentDays > 0) dayElapsed = $"{spentDays} д. ";
        if (spentHours > 0) hourElapsed = $"{spentHours} ч. ";
        if (spentMinutes > 0) minElapsed = $"{spentMinutes} мин. ";
        if (spentSeconds > 0) secElapsed = $"{spentSeconds} сек. ";

        string colorPoints;
        if (qStat.Points > 0) colorPoints = "#0D840D";
        else colorPoints = "red";
        string colorCompletedSteps;
        if (qStat.CompletedSteps > 0) colorCompletedSteps = "#0D840D";
        else colorCompletedSteps = "red";
        string colorSkippedSteps;
        if (qStat.SkippedSteps > 0) colorSkippedSteps = "red";
        else colorSkippedSteps = "#0D840D";
        string colorAnswers;
        if (qStat.WrongAnswers > 0) colorAnswers = "red";
        else colorAnswers = "#0D840D";

        resultString = 
            $"Затрачено времени: \n<color=red>{dayFromStart}{hourFromStart}{minFromStart}{secFromStart}</color>\n" +
            $"Время выполнения заданий: \n<color=red>{dayElapsed}{hourElapsed}{minElapsed}{secElapsed}</color>\n" +
            // $"Пройденное расстояние: <color=red>{dayElapsed}{hourElapsed}{minElapsed}{secElapsed}</color>\n" +
            $"Набрано очков:   <color={colorPoints}>{qStat.Points}</color>\n" +
            $"Пройдено этапов:   <color={colorCompletedSteps}>{qStat.CompletedSteps}</color>\n" +
            $"Пропущено этапов:   <color={colorSkippedSteps}>{qStat.SkippedSteps}</color>\n" +
            $"Неправильных ответов:   <color={colorAnswers}><b>{qStat.WrongAnswers}</b></color>";

        return resultString;
    }
    
    private void OnDestroy()
    {
        PlayerPrefs.SetString($"{gameConfig.CurrentQuest}", JsonUtility.ToJson(questConfig));
    }

}

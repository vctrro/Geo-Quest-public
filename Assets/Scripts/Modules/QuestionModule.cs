using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class QuestionModule : MonoBehaviour
{
    [System.Serializable] public class WrongAnswerEvent : UnityEvent<List<int>> {}
    public WrongAnswerEvent OnWrongAnswer;
    [System.Serializable] public class AnswerSelected : UnityEvent<int> {}
    public AnswerSelected OnAnswerSelected;
    public UnityEvent OnBranchMode;
    public UnityEvent OnCheckAnswer;
    public UnityEvent OnCorrectAnswer;
    public UnityEvent OnNotCompleteAnswer;
    public UnityEvent OnReloadStep;
    [SerializeField] private Transform answerPanel;
    [SerializeField] private GameObject answerTextPrefab;
    [SerializeField] private GameObject answerChoicePrefab;
    [SerializeField] private GameObject answerTablePrefab;
    [SerializeField] private GameObject selectableButtonPrefab;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private Sprite iconBranch;
    [SerializeField] private Sprite buttonFinish;

    [NonSerialized] public bool multiAnswer;
    private QuestController questController;
    private QuestStatistic questStatistic;
    private NetManager netManager = new NetManager();
    private StepConfig stepConfig;
    public QuestionConfig questionConfig;
    private AudioSource stepAudio;
    private TMP_InputField textAnswerValue;
    private List<int> answerList = new List<int>();
    private List<int> wrongAnswerList = new List<int>();
    private DateTime startTimerTime;
    private int timeToFinish = 0;
    private bool finishStep = false;

    private IEnumerator Start()
    {
        questController = GameObject.Find("QuestController").GetComponent<QuestController>();
        questStatistic = questController.questConfig.QuestStatistic;
        stepConfig = questController.questConfig.Steps[questStatistic.CurrentStep];
        questionConfig = questController.questConfig.Steps[questStatistic.CurrentStep].QuestionConfig;
        stepAudio = GetComponent<AudioSource>();

        transform.Find("Name").GetComponent<TextMeshProUGUI>().text = stepConfig.Name;
        var content = transform.Find("Description").GetChild(0);
        if (stepConfig.TimeToFinish > 0)
        {
            content.GetComponent<VerticalLayoutGroup>().padding.top = 100;
        }
        content.Find("Content").GetComponent<TextMeshProUGUI>().text = stepConfig.Description;
        //User button caption
        if (stepConfig.ButtonCaption != "")
        {
            var buttonGo = transform.Find("ButtonGo");
            buttonGo.GetChild(0).gameObject.SetActive(false);
            var caption1 = buttonGo.GetChild(1).GetComponent<TextMeshProUGUI>();
            var caption2 = buttonGo.GetChild(2).GetComponent<TextMeshProUGUI>();
            caption1.text = caption2.text = stepConfig.ButtonCaption + " ";
            caption1.gameObject.SetActive(true);
            caption2.gameObject.SetActive(true);
        }

        OnAnswerSelected.AddListener(SetAnswer);
        OnReloadStep.AddListener(ReloadStep);

        var skipButton = transform.Find("ButtonSkip").GetComponent<Button>();
        var skipPanel = transform.Find("Skip");

        switch (questionConfig.AnswerMode)
        {
            case AnswerMode.Text:
                GameObject answerText = Instantiate(answerTextPrefab, answerPanel);
                answerText.name = answerTextPrefab.name;
                textAnswerValue = answerText.GetComponentInChildren<TMP_InputField>();
                break;

            case AnswerMode.Choice:
                GameObject answerChoice = Instantiate(answerChoicePrefab, answerPanel);
                for (int i = 0; i < questionConfig.ValuesOfVariants.Length; i++)
                {
                    GameObject button = Instantiate(selectableButtonPrefab, answerChoice.transform);
                    button.GetComponentInChildren<TextMeshProUGUI>().text = questionConfig.ValuesOfVariants[i];
                }
                multiAnswer = (questionConfig.ChoiceCorrectAnswers.Count > 1);
                break;

            case AnswerMode.Table:
                Instantiate(answerTablePrefab, answerPanel);
                break;
                
            case AnswerMode.Branch:
                OnBranchMode.Invoke();
                GameObject answerBranch = Instantiate(answerChoicePrefab, answerPanel);
                for (int i = 0; i < questionConfig.ValuesOfVariants.Length; i++)
                {
                    GameObject button = Instantiate(selectableButtonPrefab, answerBranch.transform);
                    button.GetComponentInChildren<TextMeshProUGUI>().text = questionConfig.ValuesOfVariants[i];
                }
                skipButton.image.sprite = buttonFinish;
                var skipTitle = skipPanel.Find("SkipTitle").GetComponent<TextMeshProUGUI>();
                switch (questController.questConfig.QuestMode)
                {
                    case QuestMode.Game:
                        skipTitle.text = "Завершить игру?";
                        break;
                    case QuestMode.Route:
                        skipTitle.text = "Завершить маршрут?";
                        break;
                }
                break;
        }

        //Load image
        var imageMask = content.Find("ImageMask");
        var image = imageMask.GetChild(0).GetComponent<Image>();
        if (questionConfig.AnswerMode == AnswerMode.Branch) image.sprite = iconBranch;

        if (stepConfig.ImageUrl != "" && stepConfig.ImageUrl != null)
        {
            yield return StartCoroutine(netManager.GetTexture(stepConfig.ImageUrl));
            
            image.sprite = netManager.sprite;
            image.SetNativeSize();
            imageMask.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(image.sprite.texture.width, image.sprite.texture.height);
            // if (image.sprite.texture.width > 940)
            // {
            //     float aspectRatio = 940.0f/(float)image.sprite.texture.width;
            //     float height = (float)image.sprite.texture.height * aspectRatio;
            //     image.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(940, height);
            // }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.gameObject.GetComponent<RectTransform>());

        if (stepConfig.SkipEnabled)
        {
            skipButton.image.color = Color.white;
            skipButton.interactable = true;            
            if (stepConfig.PenaltyPointsForSkip > 0)
            {
                var skipPoints = skipPanel.Find("SkipPoints").gameObject;
                skipPoints.GetComponent<TextMeshProUGUI>().text = "- " + stepConfig.PenaltyPointsForSkip;
                skipPanel.Find("SkipImageMask").GetComponent<Image>().enabled = false;
                skipPoints.SetActive(true);
            }
            skipPanel.Find("ButtonOk").GetComponent<Button>().onClick.AddListener(SkipStep);
        }

        transform.parent.GetComponent<Animator>().Play("StepPanelDown");
        yield return new WaitForSeconds(0.6f);
        //Load audio
        StartCoroutine(PlayAudio());
        yield return new WaitForSeconds(0.1f);
        StartTimer();
    }

    private IEnumerator PlayAudio()
    {
        var audioUrl = stepConfig.AudioUrl;
        if (audioUrl != "" && audioUrl != null)
        {
            var buttonAudio = transform.Find("ButtonAudio").GetComponent<Button>();
            buttonAudio.interactable = true;
            buttonAudio.gameObject.GetComponent<Image>().color = Color.white;
            transform.Find("AudioPanelMask").GetComponentInChildren<Button>().onClick.AddListener(()=>{stepAudio.Stop(); stepAudio.Play();});
            var togglePlay = transform.Find("AudioPanelMask").GetComponentInChildren<Toggle>();
            buttonAudio.onClick.AddListener(()=>{
                if (stepAudio.isPlaying) togglePlay.isOn = true;
                else togglePlay.isOn = false;
                });
            togglePlay.onValueChanged.AddListener((input)=>{
                if (input) stepAudio.UnPause();
                else stepAudio.Pause();
                });

            string fileType = audioUrl.Substring(audioUrl.Length-4);
            yield return StartCoroutine(netManager.GetAudioClip(audioUrl, fileType));
            stepAudio.clip = netManager.audioClip;
            stepAudio.Play();
        }
    }

    private void StartTimer()
    {
        timeToFinish = stepConfig.TimeToFinish;

        if (stepConfig.StepStarting)
        {
            startTimerTime = System.DateTime.Parse(stepConfig.StartTimerTime);
        }
        else
        {
            stepConfig.StepStarting = true;
            startTimerTime = System.DateTime.Now;
            stepConfig.StartTimerTime = startTimerTime.ToString();
        }
            
        if (timeToFinish > 0)
        {
            transform.Find("Timer").gameObject.SetActive(true);
            StartCoroutine(Timer());
        }
    }

    private IEnumerator Timer()
    {        
        while (!finishStep)
        {
            float dt = timeToFinish - (float) (System.DateTime.Now - startTimerTime).TotalSeconds;
            int minutes = Mathf.Abs((int)dt) / 60;
            if (dt < 0) 
            {
                timer.color = Color.red;
            }

            if (minutes < 30)
            {
                int seconds = (int) Mathf.Abs(dt) % 60;
                timer.text = $"{minutes}:{seconds.ToString("00")}";
                yield return new WaitForSeconds(1f);
            }
            else
            {
                string days = "", hours = "", mins = "";
                int daysLeft = minutes/1440, hoursLeft = (minutes%1440)/60, minsLeft = (minutes%1440)%60;

                if (daysLeft > 0) days = $"{daysLeft} д." ;
                if (hoursLeft > 0) hours = $"{hoursLeft} ч." ;
                if (minsLeft > 0) mins = $"{minsLeft} мин." ;

                timer.text = $"{days} {hours} {mins}";
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private void SetAnswer(int answer)
    {
        if (multiAnswer)
        {
            if (answerList.Contains(answer)) answerList.Remove(answer);
            else answerList.Add(answer);
        }
        else
        {
            if (answerList.Contains(answer))
            {
                answerList.Clear();
            }                
            else
            {
                answerList.Clear();
                answerList.Add(answer);
            }
        }
    }

    public void CheckAnswer()
    {
        switch (questionConfig.AnswerMode)
        {
            case AnswerMode.Text:
                AnswetText();
                break;
            case AnswerMode.Choice:
                AnswerChoice();
                break;
            case AnswerMode.Table:
                AnswerChoice();
                break;
            case AnswerMode.Branch:
                AnswerBranch();
                break;
        }
    }

    private void AnswerBranch()
    {
        if (answerList.Count < 1) return;
        OnCheckAnswer.Invoke();
        
        questStatistic.CurrentStep = questionConfig.LinksToGo[answerList[0]];
        if (questionConfig.PointsOfVariants.Length == questionConfig.LinksToGo.Length)
        questStatistic.Points += questionConfig.PointsOfVariants[answerList[0]];
        if (questionConfig.PenaltyPointsOfVariants.Length == questionConfig.LinksToGo.Length)
        questStatistic.Points -= questionConfig.PenaltyPointsOfVariants[answerList[0]];
        
        finishStep = true;
        stepConfig.StepStarting = false;
        var spentTime = (float) (System.DateTime.Now - startTimerTime).TotalSeconds;
        questStatistic.SpentTime += spentTime;
        if ((float)timeToFinish > spentTime)
        {
            questStatistic.Points += stepConfig.PointsForTime;
        }
        else
        {
            questStatistic.Points -= stepConfig.PenaltyPointsForTime;
        }

        questController.NextStep();
    }

    private void AnswerChoice()
    {
        if (answerList.Count < 1) return;
        OnCheckAnswer.Invoke();

        wrongAnswerList.Clear();
        for (int i = 0; i < answerList.Count; i++)
        {
            if (!questionConfig.ChoiceCorrectAnswers.Contains(answerList[i])) wrongAnswerList.Add(answerList[i]);
        }

        if (wrongAnswerList.Count > 0)
        {
            WrongAnswer();
        }
        else if (answerList.Count < questionConfig.ChoiceCorrectAnswers.Count)
        {
            OnNotCompleteAnswer.Invoke();
        } 
        else
        {
            CorrectAnswer();
        }
    }

    private void AnswetText()
    {
        if (textAnswerValue.text == "") return;
        OnCheckAnswer.Invoke();

        textAnswerValue.interactable = false;
        string answer = textAnswerValue.text.ToLower().Trim();

        foreach (var item in questionConfig.TextCorrectAnswers)
        {
            item.ToLower().Trim();
        }


        if (questionConfig.TextCorrectAnswers.Contains(answer))
        {
            textAnswerValue.transform.GetChild(0).Find("Text").GetComponent<TextMeshProUGUI>().color = new Color(0, 0.7843137f, 0);
            CorrectAnswer();
        }
        else
        {
            textAnswerValue.transform.GetChild(0).Find("Text").GetComponent<TextMeshProUGUI>().color = Color.red;
            WrongAnswer();
        }
    }

    private void SkipStep()
    {
        finishStep = true;
        stepConfig.StepStarting = false;
        var spentTime = (float) (System.DateTime.Now - startTimerTime).TotalSeconds;
        questStatistic.SpentTime += spentTime;
        if ((float)timeToFinish > spentTime)
        {
            
        }
        else
        {
            questStatistic.Points -= stepConfig.PenaltyPointsForTime;
        }
        questStatistic.SkippedSteps++;
        questStatistic.Finish = stepConfig.Finish;
        if (questionConfig.AnswerMode == AnswerMode.Branch) questStatistic.Finish = true;
        else if (stepConfig.NextStep == 0) questStatistic.CurrentStep++;
        else questStatistic.CurrentStep = stepConfig.NextStep;
        questController.NextStep();
    }

    private void CorrectAnswer()
    {
        OnCorrectAnswer.Invoke();
        finishStep = true;
        stepConfig.StepStarting = false;
        var spentTime = (float) (System.DateTime.Now - startTimerTime).TotalSeconds;
        questStatistic.SpentTime += spentTime;
        if ((float)timeToFinish > spentTime)
        {
            questStatistic.Points += stepConfig.PointsForTime;
        }
        else
        {
            questStatistic.Points -= stepConfig.PenaltyPointsForTime;
        }
        questStatistic.Points += questionConfig.Points;
        questStatistic.CompletedSteps++;
        questStatistic.Finish = stepConfig.Finish;
        if (stepConfig.NextStep == 0) questStatistic.CurrentStep++;
        else questStatistic.CurrentStep = stepConfig.NextStep;

        var skipButton = transform.Find("ButtonSkip").GetComponent<Button>();
            skipButton.image.color = new Color(1f,1f,1f,0.5f);
            skipButton.interactable = false;
    }

    private void WrongAnswer()
    {
        OnWrongAnswer.Invoke(wrongAnswerList);
        questStatistic.Points -= questionConfig.PenaltyPoints;
        questStatistic.WrongAnswers++;
    }

    private void ReloadStep()
    {
        if (questionConfig.AnswerMode == AnswerMode.Text)
        {
            textAnswerValue.interactable = true;
            textAnswerValue.text = "";
            textAnswerValue.transform.GetChild(0).Find("Text").GetComponent<TextMeshProUGUI>().color = Color.black;
        }
        else
        {            
            answerList.Clear();
            wrongAnswerList.Clear();
        }
    }

    private void OnDestroy() {
        OnAnswerSelected.RemoveAllListeners();
        OnCheckAnswer.RemoveAllListeners();
        OnReloadStep.RemoveAllListeners();
        OnCorrectAnswer.RemoveAllListeners();
    }
}

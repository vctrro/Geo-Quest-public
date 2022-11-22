using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.IO;
using System;

public class QuestCardFilling : MonoBehaviour
{
    
    [SerializeField] private Sprite cloudComplete;
    [SerializeField] private Sprite questComplete;
    private Sprite cardLogo;
    private bool isDownloading;
    private Sprite descriptionImage;
    private AudioClip descriptionAudioClip;
    private GameManager gameManager;
    private AudioManager audioManager;
    private NetManager netManager = new NetManager();
    private GameConfig gameConfig;
    private QuestConfig questConfig;
    private QuestStatistic questStatistic;
    private SectionController sectionController;
    private Transform fullDescription;
    private Transform questCardButtons;
    private GameObject loading;
    private GameObject buttonRun, buttonStop, buttonInformation, buttonDownload, buttonStatistic, buttonRemove;

    private void Awake()
    {
        gameManager = GameManager.Instance;
        gameConfig = gameManager.GameConfig;
        audioManager = gameManager.AudioManager;
        sectionController = GameObject.Find("Controller").GetComponent<SectionController>();
        sectionController.OnCardClicked.AddListener(ShowMenu);
        sectionController.OnClearCardList.AddListener(DestroyCard);
        fullDescription = GameObject.Find("Canvas").transform.Find("FullDescription");
        loading = transform.Find("Loading").gameObject;
    }

    public IEnumerator Initialize(QuestConfig qConfig, GameObject qCardButtons, bool local = false)
    {
        questConfig = qConfig;
        questCardButtons = qCardButtons.transform;
        questStatistic = questConfig.QuestStatistic;
        buttonRun = questCardButtons.Find("ButtonRun").gameObject;
        buttonStop = questCardButtons.Find("ButtonStop").gameObject;
        buttonInformation = questCardButtons.Find("ButtonInfo").gameObject;
        buttonDownload = questCardButtons.Find("ButtonDownload").gameObject;
        buttonStatistic = questCardButtons.Find("ButtonStatistic").gameObject;
        buttonRemove = questCardButtons.Find("ButtonRemove").gameObject;
        
        if (PlayerPrefs.HasKey($"Stat-{questConfig.QuestId}"))
        {
            transform.Find("Complete").gameObject.GetComponent<Image>().sprite = questComplete;
            buttonStatistic.SetActive(true);
        }
        transform.Find("QuestName").gameObject.GetComponent<TextMeshProUGUI>().text = questConfig.Name;
        FillTheCard(transform);

        buttonRun.GetComponent<Button>().onClick.AddListener(RunQuest);
        buttonInformation.GetComponent<Button>().onClick.AddListener(ShowFullDescription);
        buttonStatistic.GetComponent<Button>().onClick.AddListener(ShowStatistic);
        buttonStop.GetComponent<Button>().onClick.AddListener(StopQuest);
        buttonDownload.GetComponent<Button>().onClick.AddListener(()=>{
            StartCoroutine(DownloadToDevice());
            });
        buttonRemove.GetComponent<Button>().onClick.AddListener(RemoveQuest);

        if (local)
        {
            transform.Find("Cloud").GetComponent<Image>().sprite = cloudComplete;
            buttonRun.SetActive(true);
            buttonDownload.SetActive(false);
            buttonRemove.SetActive(true);
            if (questStatistic.StartTime != "")
            {
                transform.Find("ImageBorder").GetComponent<Image>().color = new Color(0, 0.8f, 0);
                buttonStatistic.SetActive(false);
                buttonStop.SetActive(true);
            }
        }

        var imageMask = transform.Find("ImageMask");
        if (questConfig.DescriptionImageUrl != "" && questConfig.DescriptionImageUrl != null)
        {
            yield return StartCoroutine(netManager.GetTexture(questConfig.DescriptionImageUrl));
            descriptionImage = netManager.sprite;
        }
        if (questConfig.LogoUrl != "" && questConfig.LogoUrl != null)
        {
            yield return StartCoroutine(netManager.GetTexture(questConfig.LogoUrl));
            cardLogo = netManager.sprite;
            if (cardLogo != null)
            imageMask.GetChild(0).gameObject.GetComponent<Image>().sprite = cardLogo;
        }
        imageMask.GetChild(1).gameObject.SetActive(false);
        gameObject.GetComponent<Button>().onClick.AddListener(()=>{sectionController.OnCardClicked.Invoke(questCardButtons.GetSiblingIndex());});
    }

    private void FillTheCard(Transform cardToFill)
    {
        cardToFill.Find("QuestDetails").gameObject.GetComponent<TextMeshProUGUI>().text =
        $"Возраст: {questConfig.AgeMin}-{questConfig.AgeMax} лет\nСложность: {questConfig.Complexity}/10\nВремя: ~{questConfig.ApproximateTime} ч\nРасстояние: {questConfig.ApproximateDistance} км\n{questConfig.Description}";
        // if (questConfig.TimerGame) cardToFill.Find("ImageBorder").Find("TimerGame").gameObject.SetActive(true);
        
        if (cardLogo != null)
        cardToFill.Find("ImageMask").GetChild(0).gameObject.GetComponent<Image>().sprite = cardLogo;
    }

    private void ShowFullDescription()
    {
        fullDescription.Find("QuestName").GetComponent<TextMeshProUGUI>().text = questConfig.Name;
        var content = fullDescription.Find("Content").GetChild(0).GetChild(0);
        FillTheCard(content.Find("Card"));

        if (descriptionImage != null)
        {
            var imageMask = content.Find("ImageMask");
            var image = imageMask.GetChild(0).GetComponent<Image>();
            image.sprite = descriptionImage;
            image.SetNativeSize();
            imageMask.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(image.sprite.texture.width, image.sprite.texture.height);
            if (image.sprite.texture.width > 940)
            {
                float aspectRatio = 940.0f/(float)image.sprite.texture.width;
                float height = (float)image.sprite.texture.height * aspectRatio;
                image.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(940, height);
                imageMask.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(940, height);
            }
            imageMask.gameObject.SetActive(true);
        }
        
        content.Find("Description").GetComponent<TextMeshProUGUI>().text = questConfig.FullDescription;
        fullDescription.gameObject.SetActive(true);
        var contentRect = content.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        contentRect.anchoredPosition = new Vector2(0, 361);
    }

    private void ShowStatistic()
    {
        fullDescription.Find("QuestName").GetComponent<TextMeshProUGUI>().text = questConfig.Name;
        var content = fullDescription.Find("Content").GetChild(0).GetChild(0);
        FillTheCard(content.Find("Card"));
        
        var imageMask = content.Find("ImageMask");
        var image = imageMask.GetChild(0).GetComponent<Image>();
        image.SetNativeSize();
        imageMask.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(image.sprite.texture.width, image.sprite.texture.height);        
        imageMask.gameObject.SetActive(true);
        
        content.Find("Description").GetComponent<TextMeshProUGUI>().text = PrintStatistic(JsonUtility.FromJson<QuestStatistic>(PlayerPrefs.GetString($"Stat-{questConfig.QuestId}")));
        fullDescription.gameObject.SetActive(true);
        var contentRect = content.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        contentRect.anchoredPosition = new Vector2(0, 361);
    }

    private void ShowMenu(int index)
    {
        if (questCardButtons.GetSiblingIndex() == index)
        questCardButtons.gameObject.SetActive(!questCardButtons.gameObject.activeSelf);
        else questCardButtons.gameObject.SetActive(false);
    }

    private IEnumerator DownloadToDevice()
    {
        isDownloading = true;
        loading.SetActive(true);
        questConfig = JsonUtility.FromJson<QuestConfig>(JsonUtility.ToJson(questConfig));
        var loadingLine = transform.Find("Separator");
        var LoadingProgress = loadingLine.GetComponent<RectTransform>();
        loadingLine.GetComponent<Image>().color = new Color(0, 0.8f, 0);
        float loadingLineHeight = 8;
        float loadingLineWidth = 850;
        float loadingStep = loadingLineWidth/(((1 + questConfig.Steps.Length) * 2) + 1 + questConfig.Map.POIs.Length); // (questCard + Steps)*2 + map + POIs
        loadingLineWidth = 0;
        LoadingProgress.sizeDelta = new Vector2(loadingLineWidth, loadingLineHeight);
        buttonDownload.GetComponent<Button>().interactable = false;

        string path = Path.Combine(Application.persistentDataPath, questConfig.QuestId);
        Directory.CreateDirectory(path);

        if (questConfig.LogoUrl != "" && questConfig.LogoUrl != null)
        {
            var pictureUrl = questConfig.LogoUrl;
            string fileType = pictureUrl.Substring(pictureUrl.Length-4);
            yield return StartCoroutine(netManager.GetTexture(pictureUrl, true));

            if (netManager.imageData == null)
            {
                questConfig.LogoUrl = "";
            }
            else
            {
                string fileName = Path.Combine(path, questConfig.QuestId + fileType);
                SaveToFile(netManager.imageData, fileName);
                questConfig.LogoUrl = "file://" + fileName;
            }
            Debug.Log($"Quest {questConfig.QuestId}: save Logo to {questConfig.LogoUrl}");
        }
        loadingLineWidth += loadingStep;
        LoadingProgress.sizeDelta = new Vector2(loadingLineWidth, loadingLineHeight);

        if (questConfig.DescriptionImageUrl != "" && questConfig.DescriptionImageUrl != null)
        {
            var pictureUrl = questConfig.DescriptionImageUrl;
            string fileType = pictureUrl.Substring(pictureUrl.Length-4);
            yield return StartCoroutine(netManager.GetTexture(pictureUrl, true));

            if (netManager.imageData == null)
            {
                questConfig.DescriptionImageUrl = "";
            }
            else
            {
                string fileName = Path.Combine(path, questConfig.QuestId + "-1" + fileType);
                SaveToFile(netManager.imageData, fileName);
                questConfig.DescriptionImageUrl = "file://" + fileName;
            }
            Debug.Log($"Quest {questConfig.QuestId}: save DescImage to {questConfig.DescriptionImageUrl}");
        }
        loadingLineWidth += loadingStep;
        LoadingProgress.sizeDelta = new Vector2(loadingLineWidth, loadingLineHeight);

        for (int i = 0; i < questConfig.Steps.Length; i++)
        {
            var pictureUrl = questConfig.Steps[i].ImageUrl;
            if (pictureUrl != "" && pictureUrl != null)
            {
                string fileType = pictureUrl.Substring(pictureUrl.Length-4);
                yield return StartCoroutine(netManager.GetTexture(pictureUrl, true));

                if (netManager.imageData == null)
                {
                    questConfig.Steps[i].ImageUrl = "";
                }
                else
                {
                    string fileName = Path.Combine(path, i.ToString() + fileType);
                    SaveToFile(netManager.imageData, fileName);
                    questConfig.Steps[i].ImageUrl = "file://" + fileName;
                }
                Debug.Log($"Step {i}: save {pictureUrl} to {questConfig.Steps[i].ImageUrl}");
            }
            loadingLineWidth += loadingStep;
            LoadingProgress.sizeDelta = new Vector2(loadingLineWidth, loadingLineHeight);

            var audioUrl = questConfig.Steps[i].AudioUrl;
            if (audioUrl != "" && audioUrl != null)
            {
                string fileType = audioUrl.Substring(audioUrl.Length-4);
                yield return StartCoroutine(netManager.GetAudioClip(audioUrl, fileType, true));

                if (netManager.audioClipData == null)
                {
                    questConfig.Steps[i].AudioUrl = "";
                }
                else
                {
                    string fileName = Path.Combine(path, i.ToString() + fileType);
                    SaveToFile(netManager.audioClipData, fileName);
                    questConfig.Steps[i].AudioUrl = "file://" + fileName;                    
                }
                Debug.Log($"Step {i}: save {audioUrl} to {questConfig.Steps[i].AudioUrl}");
            }
            loadingLineWidth += loadingStep;
            LoadingProgress.sizeDelta = new Vector2(loadingLineWidth, loadingLineHeight);
        }
        if (questConfig.Map.MapImageUrl != "" && questConfig.Map.MapImageUrl != null)
        {
            var pictureUrl = questConfig.Map.MapImageUrl;
            string fileType = pictureUrl.Substring(pictureUrl.Length-4);
            yield return StartCoroutine(netManager.GetTexture(pictureUrl, true));

            if (netManager.imageData == null)
            {
                questConfig.Map.MapImageUrl = "";
            }
            else
            {
                string fileName = Path.Combine(path, questConfig.QuestId + "-map" + fileType);
                SaveToFile(netManager.imageData, fileName);
                questConfig.Map.MapImageUrl = "file://" + fileName;
            }
            Debug.Log($"Quest {questConfig.QuestId}: save MapImageUrl to {questConfig.Map.MapImageUrl}");
        }
        for (int i = 0; i < questConfig.Map.POIs.Length; i++)
        {
            var pictureUrl = questConfig.Map.POIs[i].ImageUrl;
            if (pictureUrl != "" && pictureUrl != null)
            {
                string fileType = pictureUrl.Substring(pictureUrl.Length-4);
                yield return StartCoroutine(netManager.GetTexture(pictureUrl, true));

                if (netManager.imageData == null)
                {
                    questConfig.Map.POIs[i].ImageUrl = "";
                }
                else
                {
                    string fileName = Path.Combine(path, "POI-" + i.ToString() + fileType);
                    SaveToFile(netManager.imageData, fileName);
                    questConfig.Map.POIs[i].ImageUrl = "file://" + fileName;
                }
                Debug.Log($"Step {i}: save {pictureUrl} to {questConfig.Map.POIs[i].ImageUrl}");
                loadingLineWidth += loadingStep;
                LoadingProgress.sizeDelta = new Vector2(loadingLineWidth, loadingLineHeight);
            }
        }
        loadingLineWidth += loadingStep;
        LoadingProgress.sizeDelta = new Vector2(loadingLineWidth, loadingLineHeight);
        transform.Find("Cloud").GetComponent<Image>().sprite = cloudComplete;
        // transform.Find("ImageBorder").GetComponent<Image>().color = new Color(0, 0.8f, 0);

        //setting buttons panel
        buttonRun.SetActive(true);
        buttonDownload.SetActive(false);
        buttonRemove.SetActive(true);

        PlayerPrefs.SetString($"{questConfig.QuestId}", JsonUtility.ToJson(questConfig));
        sectionController.sectionConfig.LocalQuests.Add(questConfig.QuestId);

        audioManager.Completed.Play();
        loading.SetActive(false);
        isDownloading = false;
    }

    private void RunQuest()
    {
        sectionController.sectionConfig.LocalQuests.Remove(questConfig.QuestId);
        sectionController.sectionConfig.LocalQuests.Add(questConfig.QuestId);
        gameConfig.CurrentQuest = questConfig.QuestId;
        gameManager.LoadScene($"Quest");
    }

    private void StopQuest()
    {
        questConfig.Steps[questStatistic.CurrentStep].StepStarting = false;
        questConfig.QuestStatistic = new QuestStatistic();
        PlayerPrefs.SetString(questConfig.QuestId, JsonUtility.ToJson(questConfig));
        buttonStop.SetActive(false);
        if (PlayerPrefs.HasKey($"Stat-{questConfig.QuestId}")) buttonStatistic.SetActive(true);
        transform.Find("ImageBorder").GetComponent<Image>().color = new Color(0.3647059f, 0.2156863f, 0);
    }

    private void RemoveQuest()
    {
        transform.Find("Loading").gameObject.SetActive(true);
        int index = "file://".Length;

        if (!String.IsNullOrEmpty(questConfig.LogoUrl))
        {
            string filePath = questConfig.LogoUrl.Substring(index);
            if (File.Exists(filePath)) File.Delete(filePath);
        }
        if (questConfig.DescriptionImageUrl != "" && questConfig.DescriptionImageUrl != null)
        {
            string filePath = questConfig.DescriptionImageUrl.Substring(index);
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        for (int i = 0; i < questConfig.Steps.Length; i++)
        {
            var pictureUrl = questConfig.Steps[i].ImageUrl;
            if (pictureUrl != "" && pictureUrl != null)
            {
                string filePath = pictureUrl.Substring(index);
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            var audioUrl = questConfig.Steps[i].AudioUrl;
            if (audioUrl != "" && audioUrl != null)
            {
                string filePath = audioUrl.Substring(index);
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }
        if (!String.IsNullOrEmpty(questConfig.Map.MapImageUrl))
        {
            string filePath = questConfig.Map.MapImageUrl.Substring(index);
            if (File.Exists(filePath)) File.Delete(filePath);
        }
        for (int i = 0; i < questConfig.Map.POIs.Length; i++)
        {
            var pictureUrl = questConfig.Map.POIs[i].ImageUrl;
            if (pictureUrl != "" && pictureUrl != null)
            {
                string filePath = pictureUrl.Substring(index);
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }
        string path = Path.Combine(Application.persistentDataPath, questConfig.QuestId);
        Directory.Delete(path);
        PlayerPrefs.DeleteKey(questConfig.QuestId);
        sectionController.sectionConfig.LocalQuests.Remove(questConfig.QuestId);
        StartCoroutine(WaitAndDestroy(questCardButtons.gameObject, 1f));
        StartCoroutine(WaitAndDestroy(gameObject, 1f));
    }

    private IEnumerator WaitAndDestroy(GameObject gObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gObject);
    }

    private void DestroyCard()
    {
        if (!isDownloading)
        {
            GameObject.Destroy(questCardButtons.gameObject);
            GameObject.Destroy(gameObject);
        }
    }

    private void SaveToFile(byte[] bytes, string path)
    {
        File.WriteAllBytes(path, bytes);
    }
    private void SaveToPNG(Sprite imageToSave, string path)
    {
        byte[] bytes = imageToSave.texture.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }

    private string PrintStatistic(QuestStatistic qStat)
    {
        string resultString;
        var timeFromStart = System.DateTime.Parse(qStat.FinishTime) - System.DateTime.Parse(qStat.StartTime);

        string dayFromStart = "", hourFromStart = "", minFromStart = "", secFromStart = "";
        if (timeFromStart.Days > 0) dayFromStart = $"{timeFromStart.Days} д. " ;
        if (timeFromStart.Hours > 0) hourFromStart = $"{timeFromStart.Hours} ч. " ;
        if (timeFromStart.Minutes > 0) minFromStart = $"{timeFromStart.Minutes} мин. " ;
        if (timeFromStart.Seconds > 0) secFromStart = $"{timeFromStart.Seconds} сек. ";

        string dayElapsed = "", hourElapsed = "", minElapsed = "", secElapsed = "";
        int spentDays = (int)(qStat.SpentTime/86400), spentHours = (int)((qStat.SpentTime%86400)/3600), 
            spentMinutes = (int)(((qStat.SpentTime%86400)%3600)/60), spentSeconds = (int)(((qStat.SpentTime%86400)%3600)%60);
        if (spentDays > 0) dayElapsed = $"{spentDays} д. " ;
        if (spentHours > 0) hourElapsed = $"{spentHours} ч. " ;
        if (spentMinutes > 0) minElapsed = $"{spentMinutes} мин. " ;
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
}

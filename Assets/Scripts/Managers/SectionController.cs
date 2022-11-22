using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System;

public class SectionController : MonoBehaviour
{
    [System.Serializable] public class CardClickedEvent : UnityEvent<int> {}
    public CardClickedEvent OnCardClicked;
    public UnityEvent OnClearCardList;
    [System.Serializable] private class QuestLinks
    {
        public QuestConfig[] questConfigList;
    }
    [SerializeField] private Transform questPanel;
    [SerializeField] private Transform searchPanel;
    [SerializeField] private Transform searchNext;
    [SerializeField] private GameObject questCardPrefab;
    [SerializeField] private GameObject questCardButtonsPrefab;
    [SerializeField] private GameObject searching;
    [SerializeField] private GameObject searchArrow;
    [SerializeField] private GameObject fullDescription;
    [SerializeField] private TextMeshProUGUI information;
    [SerializeField] private string initializeUrl = "http://xn----7sbbhbhhbizvqncl9ckh0o.xn--p1ai/wp-content/uploads/2020/06/";
    [SerializeField] private int numberOfSearchResults = 5;
    
    // private GameObject questCard;
    [NonSerialized] public SectionConfig sectionConfig;
    private TMP_InputField tagsForSearchInput, nearbyRadiusInput;
    private TMP_InputField[] ageInput, timeInput, distanceInput, complexityInput;
    private Toggle nearbyCheckInput;
    private GameManager gameManager;
    private NetManager netManager = new NetManager();
    private GPSManager gpsManager;
    private GameConfig gameConfig;
    private List<QuestConfig> foundQuestConfigs;
    private QuestLinks questLinks;
    private int searchStartIndex;
    private int numberOfFoundQuests = 0;
    private List<string> tagsForSearch = new List<string>();
    private int ageMin = 3, ageMax = 120;
    private int timeMin = 0, timeMax = 720;
    private int distanceMin = 0, distanceMax = 65000;
    private int complexityMin = 1, complexityMax = 10;
    private bool nearbyCheck;
    private int nearbyRadiusMin = 100, nearbyRadiusMax = 100000;
    private int ageMinValue, ageMaxValue;
    private int timeMinValue, timeMaxValue;
    private int distanceMinValue, distanceMaxValue;
    private int complexityMinValue, complexityMaxValue;
    private int nearbyRadiusValue;
    private bool tagSearch, ageSearch, timeSearch, distanceSearch, complexitySearch, nearbySearch;
    private bool configLoading;

    private void Awake()
    {
        gameManager = GameManager.Instance;
        gameConfig = gameManager.GameConfig;
        gpsManager = gameObject.AddComponent<GPSManager>();
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey($"{gameConfig.CurrentSection}"))
        {
            sectionConfig = JsonUtility.FromJson<SectionConfig>(PlayerPrefs.GetString($"{gameConfig.CurrentSection}"));
        }
        else
        {
            sectionConfig = new SectionConfig();
        }

        if (gameConfig.CurrentSection == "games") information.text = "НЕТ ЗАГРУЖЕННЫХ ИГР";

        tagsForSearchInput = searchPanel.GetChild(0).GetComponent<TMP_InputField>();
        ageInput = searchPanel.GetChild(1).GetComponentsInChildren<TMP_InputField>();
        timeInput = searchPanel.GetChild(2).GetComponentsInChildren<TMP_InputField>();
        distanceInput = searchPanel.GetChild(3).GetComponentsInChildren<TMP_InputField>();
        complexityInput = searchPanel.GetChild(4).GetComponentsInChildren<TMP_InputField>();
        nearbyCheckInput = searchPanel.GetChild(5).GetComponentInChildren<Toggle>();
        nearbyRadiusInput = searchPanel.GetChild(6).GetComponentInChildren<TMP_InputField>();

        ageMinValue = ageMin; ageMaxValue = ageMax;
        timeMinValue = timeMin; timeMaxValue = timeMax;
        distanceMinValue = distanceMin; distanceMaxValue = distanceMax;
        complexityMinValue = complexityMin; complexityMaxValue = complexityMax;
        nearbyRadiusValue = nearbyRadiusMin;

        AddInputListeners();
        
        LoadLocalQuests();

        initializeUrl += gameConfig.CurrentSection + ".txt";

        searchPanel.parent.Find("ButtonSearch").GetComponent<Button>().onClick.AddListener(()=>{StartCoroutine(DownloadQuestConfigList());});
        searchPanel.parent.Find("ButtonBack").GetComponent<Button>().onClick.AddListener(BackButton);

        StartCoroutine(DownloadQuestConfigList());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) BackButton();
    }

    private void BackButton()
    {
        var searchAnimator = searchPanel.GetComponentInParent<Animator>();
        if (fullDescription.activeSelf) fullDescription.transform.Find("ButtonClose").GetComponent<Button>().onClick.Invoke();
        else if (searchAnimator.GetCurrentAnimatorStateInfo(0).IsName("SearchPanelUp")) searchAnimator.SetTrigger("trigger");
        else GameManager.Instance.LoadScene("MainMenu");
    }

    public void LoadLocalQuests()
    {
        var searchAnimator = searchPanel.GetComponentInParent<Animator>();
        if (searchAnimator.GetCurrentAnimatorStateInfo(0).IsName("SearchPanelUp")) searchAnimator.SetTrigger("trigger");

        // for (int i = 1; i < questPanel.childCount-1; i++)
        // {
        //     GameObject.Destroy(questPanel.GetChild(i).gameObject);
        // }
        OnClearCardList.Invoke();
        questPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        if (sectionConfig.LocalQuests.Count == 0)
        {
            information.gameObject.SetActive(true);
            searchArrow.SetActive(true);
        }
        for (int i = sectionConfig.LocalQuests.Count-1; i >= 0 ; i--)
        {
            QuestConfig questConfig = JsonUtility.FromJson<QuestConfig>(PlayerPrefs.GetString($"{sectionConfig.LocalQuests[i]}"));
            GameObject questCard = Instantiate(questCardPrefab, questPanel);
            GameObject questCardButtons = Instantiate(questCardButtonsPrefab, questPanel);

            StartCoroutine(questCard.GetComponent<QuestCardFilling>().Initialize(questConfig, questCardButtons, true));
        }

        searchNext.SetSiblingIndex(questPanel.childCount-1);
        searchNext.gameObject.SetActive(false);
    }

    private IEnumerator DownloadQuestConfigList()
    {
        if (searchPanel.GetComponentInParent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("SearchPanelUp")) yield break;
        if (configLoading) yield break;
        configLoading = true;

        var searchButton = searchPanel.Find("SearchButton");
        var button = searchButton.Find("Button").gameObject;
        var information = searchButton.Find("Information").gameObject;        
        var loading = searchButton.Find("Loading").gameObject;

        if (questLinks == null)
        {
            button.SetActive(false);
            information.SetActive(false);
            nearbyCheckInput.interactable = false;

            loading.SetActive(true);
            yield return StartCoroutine(netManager.GetRequest(initializeUrl));
            questLinks = JsonUtility.FromJson<QuestLinks>(netManager.requestString);
            loading.SetActive(false);

            if (questLinks == null)
            {
                information.GetComponent<TextMeshProUGUI>().text = "НЕТ СОЕДИНЕНИЯ";
                information.SetActive(true);
            }
            else
            {
                // Debug.Log(JsonUtility.ToJson(questLinks));
                Debug.Log(questLinks.questConfigList.Length);
                searchStartIndex = questLinks.questConfigList.Length-1;
                nearbyCheckInput.interactable = true;
                button.SetActive(true);
            }
        }
        else
        {
            information.SetActive(false);
            nearbyCheckInput.interactable = true;
            button.SetActive(true);
        }
        // if (questLinks.questConfigList == null) yield break;
        configLoading = false;
    }

    public void NewSearch()
    {
        searchStartIndex = questLinks.questConfigList.Length-1;
        information.gameObject.SetActive(false);
        //clear QuestPanel
        // for (int i = 1; i < questPanel.childCount-1; i++)
        // {
        //     GameObject.Destroy(questPanel.GetChild(i).gameObject);
        // }

        Search();
    }

    public void Search()
    {
        OnClearCardList.Invoke();
        searchNext.gameObject.SetActive(false);
        searching.SetActive(true);
        questPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        foundQuestConfigs = new List<QuestConfig>();
        numberOfFoundQuests = 0;

        for (int i = searchStartIndex; i >= 0 ; i--)
        {
            searchStartIndex--;

            if (sectionConfig.LocalQuests.Contains(questLinks.questConfigList[i].QuestId)) continue;

            if (!SearchComplexity(questLinks.questConfigList[i])) continue;
            if (!SearchTime(questLinks.questConfigList[i])) continue;
            if (!SearchDistance(questLinks.questConfigList[i])) continue;
            if (!SearchAge(questLinks.questConfigList[i])) continue;
            if (!SearchTags(questLinks.questConfigList[i])) continue;
            if (!SearchNearby(questLinks.questConfigList[i])) continue;

            foundQuestConfigs.Add(questLinks.questConfigList[i]);
            numberOfFoundQuests++;
            // Debug.Log(i + "-" + numberOfFoundQuests);
            if (numberOfFoundQuests == numberOfSearchResults) break;
        }
        Debug.Log($"FoundQuests {numberOfFoundQuests}");

        StartCoroutine(ShowFoundQuests());        
    }    

    private IEnumerator ShowFoundQuests()
    {
        for (int i = 0; i < foundQuestConfigs.Count ; i++)
        {
            GameObject questCard = Instantiate(questCardPrefab, questPanel);
            GameObject questCardButtons = Instantiate(questCardButtonsPrefab, questPanel);
            
            var qConfig = foundQuestConfigs[i];
            var qCardButtons = questCardButtons;
            StartCoroutine(questCard.GetComponent<QuestCardFilling>().Initialize(qConfig, qCardButtons));
        }

        searchNext.SetSiblingIndex(questPanel.childCount-1);
        if (searchStartIndex >= 0)
        {
            searchNext.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(2f);
        searching.SetActive(false);
        gameManager.AudioManager.Completed.Play();
        if (numberOfFoundQuests == 0)
        {
            information.text = "НЕТ ПОДХОДЯЩИХ РЕЗУЛЬТАТОВ";
            information.gameObject.SetActive(true);
        }

    }

    private int ParseInt(string input, int min, int max)
    {
        int value = 0;
        if (input != "") value = Int32.Parse(input);
        return Mathf.Clamp(value, min, max);
    }

    private bool SearchTags(QuestConfig questConfig)
    {
        if (tagSearch)
        {
            Debug.Log("TagSearch");
            foreach (var tag in tagsForSearch)
            {
                if (questConfig.Name.ToLower().Contains(tag)) return true;
                foreach (var item in questConfig.Tags)
                {
                    // Debug.Log(item + " = " + tag);
                    if (item.ToLower().Contains(tag)) return true;
                }
                // if (questConfig.Tags.Contains(tag)) return true;
            }
            return false;
        }
        else
        {
            return true;
        }
    }

    private bool SearchAge(QuestConfig questConfig)
    {
        if (ageSearch)
        {
            Debug.Log($"AgeSearch: min - {ageMinValue}, max - {ageMaxValue}");
            if (questConfig.AgeMin <= ageMaxValue && questConfig.AgeMax >= ageMinValue) return true;
            else return false;
        }
        else
        {
            return true;
        }
    }

    private bool SearchTime(QuestConfig questConfig)
    {
        if (timeSearch)
        {
            Debug.Log($"TimeSearch: min - {timeMinValue}, max - {timeMaxValue}");
            if (questConfig.ApproximateTime >= timeMinValue && questConfig.ApproximateTime <= timeMaxValue) return true;
            else return false;
        }
        else
        {
            return true;
        }
    }

    private bool SearchDistance(QuestConfig questConfig)
    {
        if (distanceSearch)
        {
            Debug.Log($"DistanceSearch: min - {distanceMinValue}, max - {distanceMaxValue}");
            if (questConfig.ApproximateDistance >= distanceMinValue && questConfig.ApproximateDistance <= distanceMaxValue) return true;
            else return false;
        }
        else
        {
            return true;
        }
    }

    private bool SearchComplexity(QuestConfig questConfig)
    {
        if (complexitySearch)
        {
            Debug.Log($"ComplexitySearch: min - {complexityMinValue}, max - {complexityMaxValue}");
            if (questConfig.Complexity >= complexityMinValue && questConfig.Complexity <= complexityMaxValue) return true;
            else return false;
        }
        else
        {
            return true;
        }
    }

    private bool SearchNearby(QuestConfig questConfig)
    {
        if (nearbySearch)
        {
            Debug.Log($"NearbySearch: {nearbyRadiusValue} m.");
            if (questConfig.StartPosition.Length < 2) return false;
            var distance = gpsManager.GetDistanceTo(questConfig.StartPosition[0], questConfig.StartPosition[1]);
            if (distance <= nearbyRadiusValue) return true;
            else return false;
        }
        else
        {
            return true;
        }
    }

    private void AddInputListeners()
    {
        tagsForSearchInput.onEndEdit.AddListener((input)=>{
            if (input == "")
            {
                tagSearch = false;
                return;
            }
            tagsForSearch = new List<string>();
            var splitText = input.Split(',');
            foreach (var item in splitText)
            {
                tagsForSearch.Add(item.ToLower().Trim());
            }
            tagSearch = true;
        });
        
        ageInput[0].onEndEdit.AddListener((input)=>{
            if (input =="")
            {
                if (ageInput[1].text == "") ageSearch = false;
                ageMinValue = ageMin;
            }
            else
            {
                ageMinValue = ParseInt(input, ageMin, ageMax);
                ageSearch = true;
            }
        });
        ageInput[1].onEndEdit.AddListener((input)=>{
            if (input =="")
            {
                if (ageInput[0].text == "") ageSearch = false;
                ageMaxValue = ageMax;
            }
            else
            {
                ageMaxValue = ParseInt(input, ageMin, ageMax);
                ageSearch = true;
            }
        });
        timeInput[0].onEndEdit.AddListener((input)=>{
            if (input =="")
            {
                if (timeInput[1].text == "") timeSearch = false;
                timeMinValue = timeMin;
            }
            else
            {
                timeMinValue = ParseInt(input, timeMin, timeMax);
                timeSearch = true;
            }
        });
        timeInput[1].onEndEdit.AddListener((input)=>{
            if (input =="")
            {
                if (timeInput[0].text == "") timeSearch = false;
                timeMaxValue = timeMax;
            }
            else
            {
                timeMaxValue = ParseInt(input, timeMin, timeMax);
                timeSearch = true;
            }
        });
        distanceInput[0].onEndEdit.AddListener((input)=>{
            if (input =="")
            {
                if (distanceInput[1].text == "") distanceSearch = false;
                distanceMinValue = distanceMin;
            }
            else
            {
                distanceMinValue = ParseInt(input, distanceMin, distanceMax);
                distanceSearch = true;
            }
        });
        distanceInput[1].onEndEdit.AddListener((input)=>{
            if (input =="")
            {
                if (distanceInput[0].text == "") distanceSearch = false;
                distanceMaxValue = distanceMax;
            }
            else
            {
                distanceMaxValue = ParseInt(input, distanceMin, distanceMax);
                distanceSearch = true;
            }
        });
        complexityInput[0].onEndEdit.AddListener((input)=>{
            if (input =="")
            {
                if (complexityInput[1].text == "") complexitySearch = false;
                complexityMinValue = complexityMin;
            }
            else
            {
                complexityMinValue = ParseInt(input, complexityMin, complexityMax);
                complexitySearch = true;
            }
        });
        complexityInput[1].onEndEdit.AddListener((input)=>{
            if (input =="")
            {
                if (complexityInput[0].text == "") complexitySearch = false;
                complexityMaxValue = complexityMax;
            }
            else
            {
                complexityMaxValue = ParseInt(input, complexityMin, complexityMax);
                complexitySearch = true;
            }
        });
        nearbyRadiusInput.onEndEdit.AddListener((input)=>{
            if (input =="")
            {
                nearbySearch = false;
            }
            else
            {
                nearbyRadiusValue = ParseInt(input, nearbyRadiusMin, nearbyRadiusMax);                
            }
        });
        nearbyCheckInput.onValueChanged.AddListener((input)=>{
            nearbySearch = input;
            if (input)
            {
                StartCoroutine(StartGPS());
            }
            else
            {
                StopGPS();
            }
        });
    }

    private IEnumerator StartGPS()
    {
        var searchButton = searchPanel.Find("SearchButton");
        var button = searchButton.Find("Button").gameObject;
        var information = searchButton.Find("Information").gameObject;
        var infoText = information.GetComponent<TextMeshProUGUI>();
        var loading = searchButton.Find("Loading").gameObject;

        button.SetActive(false);
        loading.SetActive(true);
        yield return StartCoroutine(gpsManager.StartGPS());

        switch (gpsManager.GetGPSStatus)
        {
            case GPSStatus.Disabled:
                infoText.text = "GPS ОТКЛЮЧЕН";
                loading.SetActive(false);
                information.SetActive(true);
                break;
            case GPSStatus.NoSignal:
                infoText.text = "НЕТ СИГНАЛА GPS";
                loading.SetActive(false);
                information.SetActive(true);
                break;
            case GPSStatus.Error:
                infoText.text = "ОШИБКА GPS";
                loading.SetActive(false);
                information.SetActive(true);
                break;
            case GPSStatus.OK:
                loading.SetActive(false);
                button.SetActive(true);
                break;
        }
    }

    private void StopGPS()
    {
        // StopCoroutine(StartGPS());
        gpsManager.StopGPS();
        var searchButton = searchPanel.Find("SearchButton");
        searchButton.Find("Information").gameObject.SetActive(false);
        searchButton.Find("Loading").gameObject.SetActive(false);
        searchButton.Find("Button").gameObject.SetActive(true);
        
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetString($"{gameConfig.CurrentSection}", JsonUtility.ToJson(sectionConfig));
    }
}
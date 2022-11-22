using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class NavigationModule : MonoBehaviour
{
    [System.Serializable] public class GPSDataEvent : UnityEvent<float, float, float, float> {}
    public GPSDataEvent OnGPSDataUpdate;
    [SerializeField] private GameObject arrival;
    [SerializeField] private Transform mapPanel;
    [SerializeField] private Transform arrow;
    [SerializeField] private Transform compass;
    [SerializeField] private TextMeshProUGUI distanceValue;
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private TextMeshProUGUI gpsLocation; //temp
    [SerializeField] private TMP_InputField inputLat; //temp
    [SerializeField] private TMP_InputField inputLon; //temp

    private QuestController questController;
    private QuestStatistic questStatistic;
    private NetManager netManager = new NetManager();
    private GPSManager gpsManager;
    private StepConfig stepConfig;
    private MapController mapController;
    private AudioSource stepAudio;
    private bool finish;
    private float pointLatitude;
    private float pointLongitude;
    private float pointRadius = 5f;
    private int distance;
    private DateTime startTimerTime;
    private float timeToFinish;
    float compasBalanceSpeed = 0.002f;

    private void Awake() 
    {
        gpsManager = gameObject.AddComponent<GPSManager>();
    }


    private IEnumerator Start()
    {
        questController = GameObject.Find("QuestController").GetComponent<QuestController>();
        questStatistic = questController.questConfig.QuestStatistic;
        stepConfig = questController.questConfig.Steps[questStatistic.CurrentStep];
        mapController = GetComponentInChildren<MapController>();
        stepAudio = GetComponent<AudioSource>();

        inputLat.onEndEdit.AddListener(LatChange);
        inputLon.onEndEdit.AddListener(LonChange);

        transform.Find("Name").GetComponent<TextMeshProUGUI>().text = stepConfig.Name;
        var infoScreen = transform.Find("Info");
        infoScreen.Find("Name").GetComponent<TextMeshProUGUI>().text = stepConfig.Name;
        var content = infoScreen.Find("Description").GetChild(0);
        content.GetComponentInChildren<TextMeshProUGUI>().text = stepConfig.Description;
        
        var buttonGo = transform.Find("ButtonGo");
        if (stepConfig.ButtonCaption != "")
        {
            buttonGo.GetChild(0).gameObject.SetActive(false);
            var caption1 = buttonGo.GetChild(1).GetComponent<TextMeshProUGUI>();
            var caption2 = buttonGo.GetChild(2).GetComponent<TextMeshProUGUI>();
            caption1.text = caption2.text = stepConfig.ButtonCaption + " ";
            caption1.gameObject.SetActive(true);
            caption2.gameObject.SetActive(true);
        }
        buttonGo.GetComponent<Button>().onClick.AddListener(StartGame);
        startTimerTime = System.DateTime.Now;

        

        if (stepConfig.SkipEnabled)
        {
            var skipButton = transform.Find("ButtonSkip").GetComponent<Button>();
            skipButton.image.color = Color.white;
            skipButton.interactable = true;
            var skipPanel = transform.Find("Skip");
            if (stepConfig.PenaltyPointsForSkip > 0)
            {
                var skipPoints = skipPanel.Find("SkipPoints").gameObject;
                skipPoints.GetComponent<TextMeshProUGUI>().text = "- " + stepConfig.PenaltyPointsForSkip;
                skipPanel.Find("SkipImageMask").GetComponent<Image>().enabled = false;
                skipPoints.SetActive(true);
            }
            skipPanel.Find("ButtonOk").GetComponent<Button>().onClick.AddListener(SkipStep);
        }

        //Load image
        var imageMask = content.Find("ImageMask");
        if (stepConfig.ImageUrl != "" && stepConfig.ImageUrl != null)
        {
            yield return StartCoroutine(netManager.GetTexture(stepConfig.ImageUrl));
            
            var image = imageMask.GetChild(0).GetComponent<Image>();
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

        StartCoroutine(StartGPS());
        
        transform.parent.GetComponent<Animator>().Play("StepPanelDown");
        yield return new WaitForSeconds(0.6f);
        //Load audio
        StartCoroutine(PlayAudio());
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

    private void StartGame()
    {
        StartTimer();  
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
        while (!finish)
        {
            float dt = timeToFinish - (float) (System.DateTime.Now - startTimerTime).TotalSeconds;
            int minutes = Mathf.Abs((int)dt) / 60;
            if (dt < 0) 
            {
                timer.color = Color.red;
            }

            if (minutes < 30)
            {
                float seconds = Mathf.Abs(dt) % 60;
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

    private void Update()
    {
        mapPanel.rotation = Quaternion.Lerp(
            mapPanel.rotation, 
            gpsManager.DeviceHeading, 
            Time.time * compasBalanceSpeed);
        compass.rotation = mapPanel.rotation;
        arrow.rotation = Quaternion.Lerp(
            arrow.rotation, 
            gpsManager.GetDirectionTo(pointLatitude, pointLongitude), 
            Time.time * compasBalanceSpeed);
    }

    public IEnumerator UpdateData()
    {
        for (int i = stepConfig.CurrentNavPoint; i < stepConfig.NavigationRoute.Length; i++)
        {
            float distance = 0;
            for (int n = i; n < stepConfig.NavigationRoute.Length-1; n++)
            {
                distance += gpsManager.DistanceInM(
                    stepConfig.NavigationRoute[n].Latitude, 
                    stepConfig.NavigationRoute[n].Longitude, 
                    stepConfig.NavigationRoute[n+1].Latitude, 
                    stepConfig.NavigationRoute[n+1].Longitude);
            }        
        
            pointLatitude = stepConfig.NavigationRoute[i].Latitude;
            pointLongitude = stepConfig.NavigationRoute[i].Longitude;
            pointRadius = stepConfig.NavigationRoute[i].Accuracy;

            var navPoint = mapController.navPoints[i];
            navPoint.GetComponent<Image>().color = Color.red;
            navPoint.GetComponent<Animator>().enabled = true;
            var pointImage = navPoint.transform.Find("Image");
            pointImage.GetComponent<Image>().color = Color.red;
            pointImage.Find("Caption").gameObject.SetActive(true);
            var pointDistance = pointImage.GetComponentInChildren<TextMeshProUGUI>();

            var distanceToNexPoint = gpsManager.GetDistanceTo(pointLatitude, pointLongitude);
            pointDistance.text = distanceToNexPoint.ToString() + "м";
            distanceValue.text = ((int)distance + distanceToNexPoint).ToString();
        
            while (!finish)
            {
                if (distanceToNexPoint < pointRadius)
                {
                    navPoint.GetComponent<Animator>().enabled = false;
                    navPoint.GetComponent<Image>().enabled = false;
                    pointImage.GetComponent<Image>().color = new Color(0, 0.8f, 0);
                    pointImage.Find("Caption").gameObject.SetActive(false);
                    break;
                }

                yield return new WaitForSeconds(1f);
                distanceToNexPoint = gpsManager.GetDistanceTo(pointLatitude, pointLongitude);
                pointDistance.text = distanceToNexPoint.ToString() + "м";
                distanceValue.text = ((int)distance + distanceToNexPoint).ToString();
                // if (distance < finishRadius + 20) distanceValue.color = new Color(0, 0.7843137f, 0);
            }
            stepConfig.CurrentNavPoint++;
        }
        Arrival();
    }

    private IEnumerator ShowGPSData()
    {
        while (!finish)
        {
            OnGPSDataUpdate.Invoke(Input.location.lastData.latitude, Input.location.lastData.longitude,
                    Input.location.lastData.altitude, Input.location.lastData.horizontalAccuracy);

            gpsLocation.text = "Lat: " + Input.location.lastData.latitude + "\n" +
                    "Long: " + Input.location.lastData.longitude + "\n" + 
                    "Alt: " + Input.location.lastData.altitude + " m\n" + 
                    "Accuracy: " + Input.location.lastData.horizontalAccuracy + " m";
            yield return new WaitForSeconds(2f);
        }
    }

    private void SkipStep()
    {
        gpsManager.StopGPS();
        finish = true;
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
        if (stepConfig.NextStep == 0) questStatistic.CurrentStep++;
        else questStatistic.CurrentStep = stepConfig.NextStep;
        questController.NextStep();
    }

    private void Arrival()
    {
        gpsManager.StopGPS();
        finish = true;
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
        questStatistic.CompletedSteps++;
        questStatistic.Finish = stepConfig.Finish;
        if (stepConfig.NextStep == 0) questStatistic.CurrentStep++;
        else questStatistic.CurrentStep = stepConfig.NextStep;

        var skipButton = transform.Find("ButtonSkip").GetComponent<Button>();
            skipButton.image.color = new Color(1f,1f,1f,0.5f);
            skipButton.interactable = false;
        
        arrival.GetComponentInChildren<Button>().onClick.AddListener(()=>{questController.NextStep();});
        arrival.SetActive(true);
    }

    private IEnumerator StartGPS()
    {
        var gpsStatus = transform.Find("Info").Find("GPSStatus").GetComponent<TextMeshProUGUI>();

        yield return StartCoroutine(gpsManager.StartGPS(true));

        switch (gpsManager.GetGPSStatus)
        {
            case GPSStatus.Disabled:
                gpsStatus.color = Color.red;
                gpsStatus.text = "ОТКЛЮЧЕН";
                break;
            case GPSStatus.NoSignal:
                gpsStatus.color = Color.red;
                gpsStatus.text = "НЕТ СИГНАЛА";
                break;
            case GPSStatus.Error:
                gpsStatus.color = Color.red;
                gpsStatus.text = "ОШИБКА";
                break;
            case GPSStatus.OK:
                gpsStatus.color = new Color(0, 0.7843137f, 0);
                gpsStatus.text = "OK";
                StartCoroutine(ShowGPSData());
                
                yield return StartCoroutine(mapController.InitializeMap());

                while(Input.location.lastData.horizontalAccuracy > 25)
                {
                    yield return new WaitForSeconds(1f);;
                }                
                var buttonGo = transform.Find("ButtonGo").GetComponent<Button>();
                buttonGo.onClick.AddListener(()=>{
                    StopCoroutine(ShowGPSData());
                    // mapController.ShowPath();
                    StartCoroutine(mapController.UpdateMap());
                    StartCoroutine(UpdateData());
                });
                buttonGo.interactable = true;
                buttonGo.gameObject.GetComponent<Image>().color = Color.white;
                break;
        }
    }    

    public void LatChange(string lat)
    {
        pointLatitude = float.Parse(lat);
    }
    public void LonChange(string lon)
    {
        pointLongitude = float.Parse(lon);
    }
}

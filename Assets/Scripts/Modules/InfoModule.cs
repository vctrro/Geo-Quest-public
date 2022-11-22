using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoModule : MonoBehaviour
{
    private QuestController questController;
    private QuestStatistic questStatistic;
    private NetManager netManager = new NetManager();
    private StepConfig stepConfig;
    private AudioSource stepAudio;

    private IEnumerator Start()
    {
        questController = GameObject.Find("QuestController").GetComponent<QuestController>();
        questStatistic = questController.questConfig.QuestStatistic;
        stepConfig = questController.questConfig.Steps[questStatistic.CurrentStep];
        stepAudio = GetComponent<AudioSource>();
        
        transform.Find("Name").GetComponent<TextMeshProUGUI>().text = stepConfig.Name;
        var content = transform.Find("Description").GetChild(0);
        content.Find("Content").GetComponent<TextMeshProUGUI>().text = stepConfig.Description;
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

        //Load image
        var imageMask = content.Find("ImageMask");
        if (stepConfig.ImageUrl != "" && stepConfig.ImageUrl != null)
        {
            yield return StartCoroutine(netManager.GetTexture(stepConfig.ImageUrl));
            
            var image = imageMask.GetChild(0).GetComponent<Image>();
            image.sprite = netManager.sprite;
            image.SetNativeSize();
            imageMask.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(image.sprite.texture.width, image.sprite.texture.height);
            if (image.sprite.texture.width > 940)
            {
                float aspectRatio = 940.0f/(float)image.sprite.texture.width;
                float height = (float)image.sprite.texture.height * aspectRatio;
                image.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(940, height);
                imageMask.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(940, height);
            }
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.gameObject.GetComponent<RectTransform>());

        transform.Find("ButtonGo").GetComponent<Button>().onClick.AddListener(()=>{
            questStatistic.Finish = stepConfig.Finish;
            if (stepConfig.NextStep == 0) questStatistic.CurrentStep++;
            else questStatistic.CurrentStep = stepConfig.NextStep;
            questController.NextStep();
            });
        
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
}

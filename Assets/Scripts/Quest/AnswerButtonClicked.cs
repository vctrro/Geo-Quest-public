using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnswerButtonClicked : MonoBehaviour
{
    private TextMeshProUGUI[] caption;
    private GameObject icon;
    private enum ButtonMode
    {
        Answer,
        Reload,
        NextStep
    }
    private ButtonMode buttonMode = ButtonMode.Answer;
    private QuestController questController;
    private QuestionModule questionModule;
    private Button button;
    
    private void Start()
    {
        questController = GameObject.Find("QuestController").GetComponent<QuestController>();
        button = GetComponent<Button>();
        button.onClick.AddListener(ButtonClicked);
        questionModule =  GameObject.Find("Canvas").transform.Find("StepPanel").GetChild(0).
            gameObject.GetComponent<QuestionModule>();

        icon = transform.GetChild(0).gameObject;
        caption = gameObject.GetComponentsInChildren<TextMeshProUGUI>();
        
        questionModule.OnBranchMode.AddListener(()=>
            {
                caption[0].text = caption[1].text = "выбор ";
            });
        questionModule.OnCorrectAnswer.AddListener(()=>
            {
                caption[0].gameObject.SetActive(false);
                caption[1].gameObject.SetActive(false);
                icon.SetActive(true);
                buttonMode = ButtonMode.NextStep;
            });
        questionModule.OnNotCompleteAnswer.AddListener(()=>
            {
                caption[0].text = caption[1].text = "ещё раз ";
                buttonMode = ButtonMode.Reload;
            });
        questionModule.OnWrongAnswer.AddListener((List<int> temp)=>
            {
                caption[0].text = caption[1].text = "ещё раз ";
                buttonMode = ButtonMode.Reload;
            });
    }

    private void ButtonClicked()
    {
        Debug.Log(buttonMode);
        switch (buttonMode)
        {
            case ButtonMode.Answer:
                questionModule.CheckAnswer();
                break;
            case ButtonMode.Reload:
                questionModule.OnReloadStep.Invoke();
                buttonMode = ButtonMode.Answer;
                caption[0].text = caption[1].text = "ответ ";
                break;
            case ButtonMode.NextStep:
                questController.NextStep();
                break;
        }        
    }
}

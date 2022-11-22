using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableButton : MonoBehaviour
{
    [SerializeField] private Sprite buttonSelected;
    [SerializeField] private Sprite buttonNotSelected;
    [SerializeField] private Sprite buttonWrongAnswer;
    private QuestController questController;
    private QuestionModule questionModule;
    private Button button;
    private int answerNumber;
    
    private void Start()
    {
        answerNumber = transform.GetSiblingIndex();
        questController = GameObject.Find("QuestController").GetComponent<QuestController>();
        button = GetComponent<Button>();
        button.onClick.AddListener(ButtonClicked);
        questionModule = GameObject.Find("Canvas").transform.Find("StepPanel").GetChild(0).
            gameObject.GetComponent<QuestionModule>();
        questionModule.OnAnswerSelected.AddListener(AnswerSelected);
        questionModule.OnWrongAnswer.AddListener(ShowWrongAnswers);
        questionModule.OnCheckAnswer.AddListener(()=>{button.interactable = false;});
        questionModule.OnReloadStep.AddListener(ReloadStep);
    }

    private void AnswerSelected(int number)
    {
        if (answerNumber == number)
        {
            if (button.image.sprite == buttonSelected) button.image.sprite = buttonNotSelected;
            else button.image.sprite = buttonSelected;
        }
        else
        {
            if (!questionModule.multiAnswer) button.image.sprite = buttonNotSelected;
        }
    }

    private void ShowWrongAnswers(List<int> wrongAnswer)
    {
        if (wrongAnswer.Contains(answerNumber))
        {
            button.image.sprite = buttonWrongAnswer;
        }
    }

    private void ReloadStep()
    {
        button.image.sprite = buttonNotSelected;
        button.interactable = true;
    }

    private void ButtonClicked()
    {
        questionModule.OnAnswerSelected.Invoke(answerNumber);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnswerRating : MonoBehaviour
{
    [SerializeField] private Sprite[] answerRatingSprites;
    [SerializeField] private AudioClip[] answerRatingClips;
    [SerializeField] private Sprite[] pointsSprites;
    private TextMeshProUGUI answerRatingText;
    private GameObject points;
    private QuestController questController;
    private QuestionModule questionModule;

    private void Start()
    {
        questController = GameObject.Find("QuestController").GetComponent<QuestController>();
        questionModule =  GameObject.Find("Canvas").transform.Find("StepPanel").GetChild(0).
            gameObject.GetComponent<QuestionModule>();
        answerRatingText = transform.Find("Caption").GetComponent<TextMeshProUGUI>();
        points = transform.Find("Points").gameObject;

        questionModule.OnCorrectAnswer.AddListener(()=>
            {
                answerRatingText.text = "правильный ответ ";
                GetComponent<Image>().sprite = answerRatingSprites[0];
                GetComponent<AudioSource>().clip = answerRatingClips[0];
                if (questionModule.questionConfig.Points > 0)
                {
                    points.GetComponent<Image>().sprite = pointsSprites[0];
                    points.GetComponentInChildren<TextMeshProUGUI>().text = "+ " + questionModule.questionConfig.Points;
                    points.SetActive(true);
                }
                gameObject.SetActive(true);
            });
        questionModule.OnNotCompleteAnswer.AddListener(()=>
            {
                answerRatingText.text = "не полный ответ ";
                GetComponent<Image>().sprite = answerRatingSprites[1];
                GetComponent<AudioSource>().clip = answerRatingClips[1];
                gameObject.SetActive(true);
            });
        questionModule.OnWrongAnswer.AddListener((List<int> temp)=>
            {
                answerRatingText.text = "неправильный ответ ";
                GetComponent<Image>().sprite = answerRatingSprites[2];
                GetComponent<AudioSource>().clip = answerRatingClips[2];
                if (questionModule.questionConfig.PenaltyPoints > 0)
                {
                    points.GetComponent<Image>().sprite = pointsSprites[1];
                    points.GetComponentInChildren<TextMeshProUGUI>().text = "- " + questionModule.questionConfig.PenaltyPoints;
                    points.SetActive(true);
                }
                gameObject.SetActive(true);
            });
        questionModule.OnReloadStep.AddListener(()=>
            {
                points.SetActive(false);
                gameObject.SetActive(false);
            });

        gameObject.SetActive(false);
    }
}

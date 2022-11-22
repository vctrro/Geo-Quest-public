using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadSection : MonoBehaviour
{
    [SerializeField] private string section;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(()=>{
                    GameManager.Instance.GameConfig.CurrentSection = section;
                    GameManager.Instance.LoadScene("QuestList");
                    });
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private string scene;
    private Button button;

    private void Start()
    {
        button = GetComponent<Button>();

        button.onClick.AddListener(()=>{
                    GameManager.Instance.LoadScene(scene);
                    });
    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClicked : MonoBehaviour
{
    private Button button;
    private AudioSource clickSound;
    private AudioManager audioManager;

    private void Start()
    {
        audioManager = GameManager.Instance.AudioManager;
        button = GetComponent<Button>();
        clickSound = GetComponent<AudioSource>();

        button.onClick.AddListener(()=>{
            if (clickSound != null ) clickSound.Play();
            else audioManager.ButtonClicked.Play();
            });
    }
}

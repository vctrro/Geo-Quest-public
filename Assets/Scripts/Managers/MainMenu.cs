using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private GameManager gameManager;
    private GameConfig gameConfig;
    private void Awake()
    {
        gameConfig = GameManager.Instance.GameConfig;
    }
}

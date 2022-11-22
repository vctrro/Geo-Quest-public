using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Audio;

// public enum Sound { MUSIC, SOUNDS, OFF }

public class AudioManager : MonoBehaviour
{
    private GameConfig gameConfig;
    private GameManager gameManager;
    private AudioSource buttonClicked, completed;

    public AudioSource ButtonClicked { get => buttonClicked; }
    public AudioSource Completed { get => completed; }

    private void Awake()
    {
        // gameManager = GameManager.Instance;
        // gameConfig = gameManager.GameConfig;

        buttonClicked = gameObject.AddComponent<AudioSource>();
        buttonClicked.playOnAwake = false;
        buttonClicked.volume = 0.5f;
        completed = gameObject.AddComponent<AudioSource>();
        completed.playOnAwake = false;

        Addressables.LoadAssetAsync<AudioClip>("button-clicked").Completed +=
                (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<AudioClip> clip) => {
                    buttonClicked.clip = clip.Result;
                };
        Addressables.LoadAssetAsync<AudioClip>("completed").Completed +=
                (UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<AudioClip> clip) => {
                    completed.clip = clip.Result;
                };
    }
}

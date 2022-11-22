using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GPSListener : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI accuracyText;
    [SerializeField] private TextMeshProUGUI coordinateText;
    private NavigationModule navigationModule;
    private void Start()
    {
        navigationModule = GameObject.Find("Canvas").transform.Find("StepPanel").GetChild(0).
            gameObject.GetComponent<NavigationModule>();
        navigationModule.OnGPSDataUpdate.AddListener(GPSReceiver);
    }

    private void GPSReceiver(float latitude, float longitude, float altitude, float accuracy)
    {
        if (accuracy <= 25) accuracyText.color = new Color(0, 0.7843137f, 0);
        accuracyText.text = accuracy.ToString();
        coordinateText.text =
                "Ш: " + latitude + " / " +
                "Д: " + longitude + " / " + 
                "В: " + altitude + " m";
    }
}

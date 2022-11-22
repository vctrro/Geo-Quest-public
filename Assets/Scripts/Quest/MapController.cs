using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapController : MonoBehaviour
{
    [SerializeField] private GameObject pOIPrefab;
    [SerializeField] private GameObject navPointPrefab;
    [SerializeField] private Sprite finishPointSprite;
    [SerializeField] private GameObject path;
    private QuestController questController;
    private GPSManager gpsManager;
    private NetManager netManager = new NetManager();
    private StepConfig stepConfig;
    private Map map;
    private Transform mapTransform;
    private RectTransform mapTransformRect;
    public GameObject[] navPoints;
    private float mapLat, mapLon;
    private float scaleCoefficient = 10;
    float mapBalanceSpeed = 0.002f;

    public IEnumerator InitializeMap()
    {
        questController = GameObject.Find("QuestController").GetComponent<QuestController>();
        gpsManager = transform.GetComponentInParent<GPSManager>();
        var questStatistic = questController.questConfig.QuestStatistic;
        stepConfig = questController.questConfig.Steps[questStatistic.CurrentStep];
        map = questController.questConfig.Map;
        mapTransform = transform.Find("Map");
        mapTransformRect = mapTransform.GetComponent<RectTransform>();
        navPoints = new GameObject[stepConfig.NavigationRoute.Length];

        //Load map
        Debug.Log("Load map");
        if (map.MapImageUrl != "" && map.MapImageUrl != null)
        {
            yield return StartCoroutine(netManager.GetTexture(map.MapImageUrl));
            var mapImage = mapTransform.GetComponent<Image>();
            mapImage.sprite = netManager.sprite;
            mapImage.SetNativeSize();

            mapImage.enabled = true;
            scaleCoefficient = 1000/map.MapScale;
            mapLat = map.LeftCornerCoords.Latitude;
            mapLon = map.LeftCornerCoords.Longitude;
        }
        else
        {
            mapLat = stepConfig.NavigationRoute[0].Latitude;
            mapLon = stepConfig.NavigationRoute[0].Longitude;
        }

        //Load NavigationPoints
        Debug.Log("Load NavPoints");
        for(int i = 0; i < stepConfig.NavigationRoute.Length; i++)
        {
            var distance = scaleCoefficient * gpsManager.DistanceInM(
                mapLat, mapLon,
                stepConfig.NavigationRoute[i].Latitude, stepConfig.NavigationRoute[i].Longitude);
            var angel = Mathf.Deg2Rad * gpsManager.DirectionFromTo(
                mapLat, mapLon,
                stepConfig.NavigationRoute[i].Latitude, stepConfig.NavigationRoute[i].Longitude);
            var navPointCoords = new Vector2(
                distance * Mathf.Sin(angel),
                distance * Mathf.Cos(angel));
            // GameObject navPoint = Instantiate(navPointPrefab, navPointCoords, Quaternion.identity, mapTransform);
            GameObject navPoint = Instantiate(navPointPrefab);
            navPoint.transform.SetParent(mapTransform);
            navPoint.GetComponent<RectTransform>().anchoredPosition = navPointCoords;
            var pointDiameter = stepConfig.NavigationRoute[i].Accuracy * 2 * scaleCoefficient;
            navPoint.GetComponent<RectTransform>().sizeDelta = new Vector2(pointDiameter, pointDiameter);
            if (i == stepConfig.NavigationRoute.Length-1)
            {
                var navPointImage = navPoint.transform.Find("Image").GetComponent<Image>();
                navPointImage.sprite = finishPointSprite;
                navPointImage.SetNativeSize();
            }
            navPoints[i] = navPoint;
        }

        //Load POIs
        Debug.Log("Load POIs");
        for(int i = 0; i < map.POIs.Length; i++)
        {
            var distance = scaleCoefficient * gpsManager.DistanceInM(
                mapLat, mapLon,
                map.POIs[i].Coordinates.Latitude, map.POIs[i].Coordinates.Longitude);
            var angel = Mathf.Deg2Rad * gpsManager.DirectionFromTo(
                mapLat, mapLon,
                map.POIs[i].Coordinates.Latitude, map.POIs[i].Coordinates.Longitude);
            var pOICoords = new Vector2(
                distance * Mathf.Sin(angel),
                distance * Mathf.Cos(angel));
            // GameObject pOI = Instantiate(pOIPrefab, pOICoords, Quaternion.identity, mapTransform);
            GameObject pOI = Instantiate(pOIPrefab);
            pOI.transform.SetParent(mapTransform);
            pOI.GetComponent<RectTransform>().anchoredPosition = pOICoords;
            var caption = pOI.transform.Find("Caption");
            caption.gameObject.SetActive(true);
            var pOIName = caption.GetComponentInChildren<TextMeshProUGUI>();
            pOIName.text = map.POIs[i].Name;
            LayoutRebuilder.ForceRebuildLayoutImmediate(pOIName.gameObject.GetComponent<RectTransform>());
            var captionRect = caption.GetComponent<RectTransform>();
            captionRect.sizeDelta = new Vector2(
                pOIName.gameObject.GetComponent<RectTransform>().sizeDelta.x + 20,
                captionRect.sizeDelta.y);

            if (map.POIs[i].ImageUrl != "" && map.POIs[i].ImageUrl != null)
            {
                var pOIImage = pOI.transform.Find("Image").GetComponent<Image>();
                yield return StartCoroutine(netManager.GetTexture(map.POIs[i].ImageUrl));
                pOIImage.sprite = netManager.sprite;
            }

            if (map.POIs[i].Color != "" && map.POIs[i].Color != null)
            {
                var pOIColor = new Color();
                if (ColorUtility.TryParseHtmlString(map.POIs[i].Color, out pOIColor))
                pOI.GetComponent<Image>().color = pOIColor;
            }

            caption.gameObject.SetActive(false);
        }

        // StartCoroutine(transform.GetComponentInParent<NavigationModule>().UpdateData());
        // StartCoroutine(UpdateMap());
    }

    public IEnumerator UpdateMap()
    {
        // ShowPath();
        while (true)
        {
            float CurrentLat = Input.location.lastData.latitude,
            CurrentLon = Input.location.lastData.longitude;
            // gpsManager.GetGPSData(out CurrentLat, out CurrentLon, out var alt, out var acc);

            var distance = scaleCoefficient * gpsManager.DistanceInM(
                CurrentLat, CurrentLon,
                mapLat, mapLon);
            var angel = Mathf.Deg2Rad * gpsManager.DirectionFromTo(
                CurrentLat, CurrentLon,
                mapLat, mapLon);
            // var mapShift = new Vector2(
            //     distance * Mathf.Sin(angel),
            //     distance * Mathf.Cos(angel));
            mapTransformRect.anchoredPosition = new Vector2(
                    distance * Mathf.Sin(angel),
                    distance * Mathf.Cos(angel));

            yield return new WaitForSeconds(1f);
        }
    }

    public void ShowPath()
    {
        Vector2 segment = navPoints[0].GetComponent<RectTransform>().anchoredPosition;
        float angle = Vector2.SignedAngle(Vector2.up, segment);
        var pathRect = path.GetComponent<RectTransform>();
        pathRect.sizeDelta = new Vector2(30, segment.magnitude);
        pathRect.localRotation = Quaternion.Euler(0 , 0, angle);
        // foreach (var item in collection)
        // {
            
        // }
    }
}

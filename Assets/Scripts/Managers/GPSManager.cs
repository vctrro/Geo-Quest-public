using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GPSStatus
{
    Disabled,
    NoSignal,
    Error,
    OK
}
public class GPSManager : MonoBehaviour
{
    private const double EquatorialEarthRadius = 6378.1370;
    private const double Deg2Rad = (Math.PI / 180); //degrees to radians
    private const float LowPassFilter = 0.05f;

    private GPSStatus _getGPSStatus;
    private Quaternion _deviceHeading, _directionToPoint;
    private float _heading;

    public GPSStatus GetGPSStatus { get => _getGPSStatus; }
    public Quaternion DeviceHeading { get => _deviceHeading; }

    public IEnumerator StartGPS(bool compassEnable = false)
    {
        // First, check if user has location service enabled
        if (Input.location.isEnabledByUser == false)
        {
            _getGPSStatus = GPSStatus.Disabled;
            yield break;
        }

        // Start service before querying location
        Input.location.Start(5, 5);

        // Wait until service initializes
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(0.5f);
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if (maxWait < 1)
        {
            _getGPSStatus = GPSStatus.NoSignal;
            yield break;
        }

        // Connection has failed
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            _getGPSStatus = GPSStatus.Error;
            yield break;
        }
        else
        {
            // Access granted and location value could be retrieved
            _getGPSStatus = GPSStatus.OK;
            if (compassEnable) StartCoroutine(StartCompass());
        }
    }

    private IEnumerator StartCompass()
    {
        Input.compass.enabled = true;
        float compasHeading = Input.compass.trueHeading;
        _deviceHeading = Quaternion.Euler(0, 0, Input.compass.trueHeading);
        KalmanFilter kalmanFilter = new KalmanFilter(1, 1, LowPassFilter, 1, 0.1, compasHeading);
        while (true)
        {
            var deviation = Mathf.Abs(Input.compass.trueHeading - compasHeading);
            if (deviation > 90)
            {
                kalmanFilter = new KalmanFilter(1, 1, LowPassFilter, 1, 0.1, Input.compass.trueHeading);
            }
            compasHeading = (float)kalmanFilter.Output(Input.compass.trueHeading);
            _heading = compasHeading;
            compasHeading = Input.compass.trueHeading;

            _deviceHeading = Quaternion.Euler(0, 0, _heading);
            yield return null;
            // yield return new WaitForSeconds(0.1f);
        }

    }

    public void StopGPS()
    {
        StopCoroutine(StartCompass());
        Input.compass.enabled = false;
        Input.location.Stop();
    }

    // public void GetGPSData(out float latitude, out float longitude, out float altitude, out float accuracy)
    // {
    //     latitude = Input.location.lastData.latitude;
    //     longitude = Input.location.lastData.longitude;
    //     altitude = Input.location.lastData.altitude;
    //     accuracy = Input.location.lastData.horizontalAccuracy;
    // }

    public int GetDistanceTo(double lat2, double long2)
    {
        return (int)DistanceInM(Input.location.lastData.latitude, Input.location.lastData.longitude, lat2, long2);
    }

    public Quaternion GetDirectionTo(double lat2, double long2)
    {
        float directionAngle = DirectionFromTo(Input.location.lastData.latitude, Input.location.lastData.longitude, lat2, long2);
        _directionToPoint = Quaternion.Euler(0, 0, _heading - directionAngle);
        return _directionToPoint;
    }

    public float DistanceInM(double lat1, double long1, double lat2, double long2)
    {
        return (float)(1000D * DistanceInKM(lat1, long1, lat2, long2));
    }

    private double DistanceInKM(double lat1, double long1, double lat2, double long2)
    {
        double dlong = (long2 - long1) * Deg2Rad;
        double dlat = (lat2 - lat1) * Deg2Rad;
        double a = Math.Pow(Math.Sin(dlat / 2D), 2D) + Math.Cos(lat1 * Deg2Rad) * Math.Cos(lat2 * Deg2Rad) * Math.Pow(Math.Sin(dlong / 2D), 2D);
        double c = 2D * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1D - a));
        double d = EquatorialEarthRadius * c;

        return d;
    }

    public float DirectionFromTo(double lat1, double long1, double lat2, double long2)
    {
        double dlong = (long2 - long1) * Deg2Rad;
        double lat1r = lat1 * Deg2Rad;
        double lat2r = lat2 * Deg2Rad;
        double y = Math.Sin(dlong) * Math.Cos(lat2r);
        double x = Math.Cos(lat1r) * Math.Sin(lat2r) - Math.Sin(lat1r) * Math.Cos(lat2r) * Math.Cos(dlong);

        double result = Math.Atan2(y, x) / Deg2Rad;
        return (float)result;
    }

    private void OnDestroy()
    {        
        // Stop service if there is no need to query location updates continuously
        Input.compass.enabled = false;
        Input.location.Stop();
    }
}
public class KalmanFilter
{
    private double A, H, Q, R, P, x;

    public KalmanFilter(double A, double H, double Q, double R, double initial_P, double initial_x)
    {
        this.A = A; //factor of real value to previous real value
        // double B = 0; //factor of real value to real control signal
        this.H = H;
        this.Q = Q; //Process noise
        this.R = R; //assumed environment noise.
        this.P = initial_P;
        this.x = initial_x; //first measured value
    }

    public double Output(double input)
    {
        // time update - prediction
        x = A * x;
        P = A * P * A + Q;

        // measurement update - correction
        double K = P * H / (H * P * H + R);
        x = x + K * (input - H * x);
        P = (1 - K * H) * P;

        return x;
    }
}
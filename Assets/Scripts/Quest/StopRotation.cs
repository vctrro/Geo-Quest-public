using UnityEngine;

public class StopRotation : MonoBehaviour
{    
    private void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}

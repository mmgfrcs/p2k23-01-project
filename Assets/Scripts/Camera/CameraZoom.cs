using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [SerializeField] private float minZoom, maxZoom;

    private Camera mainCam;
    private float currentZoom, targetZoom, vel;
    
    // Start is called before the first frame update
    void Awake()
    {
        mainCam = GetComponent<Camera>();
        currentZoom = mainCam.orthographicSize;
        targetZoom = currentZoom;
    }

    // Update is called once per frame
    void Update()
    {
        mainCam.orthographicSize = Mathf.SmoothDamp(mainCam.orthographicSize, targetZoom, ref vel, 0.2f);

        if (Input.mouseScrollDelta.y == 0) return;
        targetZoom = Mathf.Clamp(targetZoom - Input.mouseScrollDelta.y, minZoom, maxZoom);

    }
}

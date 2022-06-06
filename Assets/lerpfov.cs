using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lerpfov : MonoBehaviour
{
    Camera _camera;
    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        _camera.fieldOfView = 40 + (Time.frameCount % 40);
    }
}

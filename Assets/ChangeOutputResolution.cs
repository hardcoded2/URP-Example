using UnityEngine;
using UnityEngine.XR;

public class ChangeOutputResolution 
{
    [RuntimeInitializeOnLoadMethod]
    static void SetOutputResolution()
    {
        XRSettings.eyeTextureResolutionScale = 0.8f; //80% of the output resolution
    }
}

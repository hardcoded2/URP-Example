using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

public class SampleInit : MonoBehaviour
{
    IEnumerator Start()
    {
        while (XRGeneralSettings.Instance == null )
        {
            Debug.Log($"Waiting on XRGeneralSettings.Instance frame {Time.frameCount}");
            yield return null;
        }
        while ( XRGeneralSettings.Instance.Manager == null)
        {
            Debug.Log($"Waiting on XRGeneralSettings.Instance.Manager frame {Time.frameCount}");
            yield return null;
        }
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
        
        while (!XRGeneralSettings.Instance.Manager.isInitializationComplete)
        {
            Debug.Log($"Waiting on XRGeneralSettings.Instance.Manager.isInitializationComplete frame {Time.frameCount}");
            yield return null;
        }
        Debug.Log($"about to call XRGeneralSettings.Instance.Manager.StartSubsystems frame {Time.frameCount}");
        XRGeneralSettings.Instance.Manager.StartSubsystems();
        Debug.Log($"done calling XRGeneralSettings.Instance.Manager.StartSubsystems  frame {Time.frameCount}");
    }

}

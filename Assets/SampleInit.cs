using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.XR.Management;
using Wave.Essence;

public class SampleInit : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"Start asink");
        
        Debug.Log("About to write");
        
        try
        {
            var filePath = $"/mnt/sdcard/Android/data/{Application.identifier}/files/testingfoo"; //try creating a new file as well, for kicks
            testWriteReadAtPath(filePath);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        Debug.Log("About to read");
        try
        {
            var filePath = $"/mnt/sdcard/Android/data/{Application.identifier}/files/testfile";
            Debug.Log($"file output is {File.ReadAllText(filePath)}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        Debug.Log("end asink");
    }

    void testWriteReadAtPath(string filePath)
    {
        var msg = $"contents wooo {DateTime.Now.ToString()} and temp cache path {Application.temporaryCachePath} and persistent {Application.persistentDataPath}";
        Debug.Log($"About to write to {filePath}");

        File.WriteAllText(filePath,msg);
        Debug.Log($"Wrote message to {filePath}");
        Debug.Log($"Value of message is {File.ReadAllText(filePath)}");
        Debug.Log($"About to print external files dirs");
    }
}

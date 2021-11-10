﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.XR.Management;
using Wave.Essence;

public class SampleInit : MonoBehaviour
{
    IEnumerator WaitOnPermissions(PermissionManager pmInstance,string[] permissions,int maxRequestCount=1000)
    {
        bool gotPermissions = false;
        for(int i=0;i<maxRequestCount;i++)
        {
            (bool finished, bool success) = (false,true);

            pmInstance.requestPermissions(permissions,results => {
                foreach (var result in results)
                {
                    Debug.Log($"Request for {result.PermissionName} granted result:{result.Granted}");
                    if (result.Granted == false)
                    {
                        success = false;
                    }
                }
                finished = true;
            });
            Debug.Log("waiting for permissions");
            while (finished == false)
            {
                yield return null;
            }
            Debug.Log($"finished waiting for permissions was it successful {success}");
            if (success)
            {
                gotPermissions = true;
                break;
            }
            Debug.Log($"Retrying {i}");
            yield return new WaitForSeconds(.3f);
        }
        Debug.Log($"Permissions got ret");
        if (!gotPermissions)
        {
            Debug.LogError("Didn't get permissions. womp womp");
        }
    }

    /*
    <uses-permission android:name="com.htc.vr.core.server.VRDataWrite"/>
    <uses-permission android:name="com.htc.vr.core.server.VRDataRead"/>
    <uses-permission android:name="com.htc.vr.core.server.VRDataProvider" />
     */
    IEnumerator Start()
    {
        Debug.Log($"Start asink");
        var pmInstance = Wave.Essence.PermissionManager.instance;
        Debug.Log("waiting for permission manager");
        while (!pmInstance.isInitialized())
            yield return null;
        Debug.Log("finished waiting for permission manager");

        //,"vive.wave.vr.oem.data.OEMDataWrite" <- no popup and never gets granted
        var permissions = new[] {"android.permission.READ_EXTERNAL_STORAGE", "android.permission.WRITE_EXTERNAL_STORAGE","vive.wave.vr.oem.data.OEMDataRead"};
        
        yield return WaitOnPermissions(pmInstance, permissions);
        Debug.Log("Got known good permissions");

        yield return WaitOnPermissions(pmInstance, new[]
        {
            "com.htc.vr.core.server.VRDataRead"
            /*,
            "com.htc.vr.core.server.VRDataWrite", //doesn't return true ever
            "com.htc.vr.core.server.VRDataProvider" //doesn't return true ever
            */
        },100);

        Debug.Log("About to write");
        
        //var filePath = $"/mnt/sdcard/Android/data/{Application.identifier}/files/testingfoo";
        try
        {
            var filePath = $"/storage/ext_sd/Android/data/{Application.identifier}/files/testingfoo"; //should work
            testWriteReadAtPath(filePath);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        try
        {
            var filePath = $"/storage/ext_sd/testingbar"; //should work
            testWriteReadAtPath(filePath);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        try
        {
            
            var filePath = $"/storage/ext_sd/HelloWorldReading.txt";
            Debug.Log("Test read");
            Debug.Log($"File: {File.ReadAllText(filePath)}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        try
        {
            
            var filePath = $"/storage/3365-3432/HelloWorldReading.txt";
            Debug.Log("Test read2");
            Debug.Log($"File: {File.ReadAllText(filePath)}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
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

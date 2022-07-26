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

        /*,
    
    yield return WaitOnPermissions(pmInstance, new[]
    {
        "com.htc.vr.core.server.VRDataRead" //seems to block on the request
        "com.htc.vr.core.server.VRDataWrite", //doesn't return true ever
        "com.htc.vr.core.server.VRDataProvider" //doesn't return true ever
        },100);
        */

        Debug.Log("About to write");
        
        //var filePath = $"/mnt/sdcard/Android/data/{Application.identifier}/files/testingfoo";
        try
        {
            manualWriteTest();
        }
        catch (Exception e)
        {
            Debug.LogError($"ManualTestwrite failed due to {e.Message}");
        }
        
        try
        {
            testWriteToAllStorageDirectories();
        }
        catch (Exception e)
        {
            Debug.LogError($"asink: error writing to all storage directories" + e.Message);
            Debug.LogException(e);
        }
    }

    void manualWriteTest()
    {
        const string DEBUG_SD_CARD_ID_SPECIFIC_TO_AUTHOR_HARDWARE = "3365-3432";
        var filesToTry = new string[]
        {
            $"/storage/ext_sd/Android/data/{Application.identifier}/files/testingfoo",
            $"/storage/ext_sd/Android/data/{Application.identifier}/cache/testingfoo2",
            $"/storage/{DEBUG_SD_CARD_ID_SPECIFIC_TO_AUTHOR_HARDWARE}/Android/data/{Application.identifier}/cache/testingfoo3",
            $"/storage/{DEBUG_SD_CARD_ID_SPECIFIC_TO_AUTHOR_HARDWARE}/Android/data/{Application.identifier}/files/testingfoo4"
        };
        var logPrefix = $"asink: {nameof(manualWriteTest)}";
        foreach (var filePath in filesToTry)
        {
            try
            {
                testWriteReadAtPath(filePath);
                Debug.Log($"{logPrefix} successfully wrote file {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} Failed to write file {filePath} : {e.Message}");
                Debug.LogException(e);
            }
            
        }

        //var directory = $"/storage/ext_sd/Android/data/{Application.identifier}/"; //this fails
        var directory = $"/storage/{DEBUG_SD_CARD_ID_SPECIFIC_TO_AUTHOR_HARDWARE}/Android/data/{Application.identifier}/";
        PrintFilesInDirectory(directory,"testingfoo*",logPrefix);
        
    }

    void PrintFilesInDirectory(string directory,string matchString,string logPrefix)
    {
        try
        {
            var allFiles = Directory.GetFiles(directory, matchString,SearchOption.AllDirectories);
            
            Debug.Log($"{logPrefix} All written files:" + String.Join(" and ",allFiles));
        }
        catch (Exception e)
        {
            Debug.LogError($"{logPrefix} Failed finding files in directory {directory} : {e.Message}");
            Debug.LogException(e);
        }
    }

    void testWriteToAllStorageDirectories()
    {
        //try writing to all drives
        int fileNumber = 0;
        var logPrefix = $"asink: {nameof(testWriteToAllStorageDirectories)}";
        foreach (var rootDirectory in Directory.EnumerateDirectories("/storage"))
        {
            var filePath = $"{rootDirectory}/Android/data/{Application.identifier}/files/testingbar{fileNumber++}";
            try
            {
                testWriteReadAtPath(filePath);
                Debug.Log($"{logPrefix} successfully wrote file {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} Failed to write file {filePath}");
                Debug.LogException(e);
            }
            
        }
        var directory = $"/storage/";
        PrintFilesInDirectory(directory,"testingbar*",logPrefix);
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

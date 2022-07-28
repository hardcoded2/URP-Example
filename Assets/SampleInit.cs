using System;
using System.IO;
using UnityEngine;

public class SampleInit : MonoBehaviour
{

    void Start()
    {
        string androidSDCardPath = GetAndroidExternalSDDir();
        
        Debug.Log($"android sd card: {androidSDCardPath}");
        testWriteReadAtPath($"{androidSDCardPath}/testingAgain");
    }

    public static string GetAndroidExternalSDDir()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    // Get all available external file directories (emulated and sdCards)
                    AndroidJavaObject[] externalFilesDirectories =  context.Call<AndroidJavaObject[]>("getExternalFilesDirs", (object)null);
                    AndroidJavaObject emulated = null;
                    AndroidJavaObject sdCard = null;
                    if (externalFilesDirectories == null)
                    {
                        Debug.LogError("NULL EXTERNAL FILES DIR");
                        return "";
                    }
                    
                    for (int i = 0; i < externalFilesDirectories.Length; i++)
                    {
                        AndroidJavaObject directory = externalFilesDirectories[i];
                        using (AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment"))
                        {
                            // Check which one is the emulated and which the sdCard.
                            bool isRemovable = environment.CallStatic<bool>("isExternalStorageRemovable", directory);
                            bool isEmulated = environment.CallStatic<bool>("isExternalStorageEmulated", directory);
                            if (isEmulated)
                                emulated = directory;
                            else if (isRemovable && isEmulated == false)
                                sdCard = directory;
                        }
                    }
                    // Return the sdCard if available
                    if (sdCard != null)
                    {
                        string returnStr = sdCard.Call<string>("getAbsolutePath");
                        return returnStr;
                        //return returnStr.Substring(0, returnStr.IndexOf("Android")) + "/";
                    }
                    else
                        return null;// emulated.Call<string>("getAbsolutePath");
                }
            }
        } catch(Exception e)
        {
            Debug.LogWarning(e.ToString());
            return null;
        }
    }

    void testWriteReadAtPath(string filePath)
    {
        var msg = $"contents wooo {DateTime.Now.ToString()} to file location {filePath}";
        Debug.Log($"About to write to {filePath}");

        File.WriteAllText(filePath,msg);
        Debug.Log($"Wrote message to {filePath}");
        Debug.Log($"Value of message is {File.ReadAllText(filePath)}");
        Debug.Log($"About to print external files dirs");
    }
}

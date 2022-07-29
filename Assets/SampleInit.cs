using System;
using System.IO;
using UnityEngine;

public class SampleInit : MonoBehaviour
{

    void Start()
    {
        string androidSDCardPath = GetAndroidExternalSDDir();
        if (string.IsNullOrEmpty(androidSDCardPath))
        {
            Debug.Log($"Android sd card path is empty");
            return;
        }
        
        Debug.Log($"android sd card: {androidSDCardPath}");
        testWriteReadAtPath($"{androidSDCardPath}/testingAgain");
    }

    public static string GetAndroidExternalSDDir() //returns the /files directory, ie /storage/3365-3432/Android/data/com.DefaultCompany.URP_Example24/files - if you want cache, then split the path
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject context = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    // Get all available external file directories (emulated and sdCards)
                    AndroidJavaObject[] externalFilesDirectories =  context.Call<AndroidJavaObject[]>("getExternalFilesDirs", (object)null);
                    AndroidJavaObject sdCard = null;
                    if (externalFilesDirectories == null)
                    {
                        Debug.LogError("NULL EXTERNAL FILES DIR");
                        return string.Empty;
                    }
                    
                    for (int i = 0; i < externalFilesDirectories.Length; i++)
                    {
                        AndroidJavaObject directory = externalFilesDirectories[i];
                        using (AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment"))
                        {
                            // Check which one is the sdCard.
                            bool isRemovable = environment.CallStatic<bool>("isExternalStorageRemovable", directory);
                            bool isEmulated = environment.CallStatic<bool>("isExternalStorageEmulated", directory);
                            if (isRemovable && isEmulated == false)
                                sdCard = directory;
                        }
                    }
                    // Return the sdCard if available
                    if (sdCard != null)
                    {
                        string returnStr = sdCard.Call<string>("getAbsolutePath");
                        return returnStr;
                    }
                    else
                        return string.Empty;// emulated.Call<string>("getAbsolutePath");
                }
            }
        } catch(Exception e)
        {
            Debug.LogWarning(e.ToString());
            return string.Empty;
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.XR.Management;
using Wave.Essence;
using Object = System.Object;

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
        /*
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
  /*      
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
*/
        //symlinkTest();
        //resolveSymlinkUsingFile();
        string androidSDCardPath = GetAndroidExternalSDDir();
        
        Debug.Log($"android sd card: {androidSDCardPath}");
        testWriteReadAtPath($"{androidSDCardPath}/testingAgain");
        
        yield break;
    }

    void symlinkTest()
    {
        try
        {
            string sdcardPathIfPresent = resolveSymlink("/storage/ext_sd"); //todo: does this throw if the sdcard isn't there?
            Debug.Log($"SDCard path {sdcardPathIfPresent}");
            //not sure if there is a slash at the end
            //add one for now
            var sdcardPath = sdcardPathIfPresent.EndsWith("/") ? sdcardPathIfPresent : sdcardPathIfPresent + "/";
            const string fileNamePrefix = "testingbaz";
            var filesToTry = new string[]
            {
                $"/storage/ext_sd/Android/data/{Application.identifier}/files/{fileNamePrefix}1",
                $"/storage/ext_sd/Android/data/{Application.identifier}/cache/{fileNamePrefix}2",
                $"/storage/{sdcardPath}/Android/data/{Application.identifier}/cache/{fileNamePrefix}3",
                $"/storage/{sdcardPath}/Android/data/{Application.identifier}/files/{fileNamePrefix}4"
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
        }
        catch (Exception e)
        {
            Debug.LogError("error in resolving symlink "+e.Message);
            Debug.LogException(e);
        }
        
    }
   
    private void ImportFromIntent(string importPath)
    {
         try
         {
             // Get the current activity
             AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
             AndroidJavaObject activityObject = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");

             // Get the current intent
             AndroidJavaObject intent = activityObject.Call<AndroidJavaObject>("getIntent");

             // Get the intent data using AndroidJNI.CallObjectMethod so we can check for null
             IntPtr method_getData = AndroidJNIHelper.GetMethodID(intent.GetRawClass(), "getData", "()Ljava/lang/Object;");
             IntPtr getDataResult = AndroidJNI.CallObjectMethod(intent.GetRawObject(), method_getData, AndroidJNIHelper.CreateJNIArgArray(new object[0]));
             if (getDataResult.ToInt32() != 0)
             {
                 // Now actually get the data. We should be able to get it from the result of AndroidJNI.CallObjectMethod, but I don't now how so just call again
                 AndroidJavaObject intentURI = intent.Call<AndroidJavaObject>("getData");

                 // Open the URI as an input channel
                 AndroidJavaObject contentResolver = activityObject.Call<AndroidJavaObject>("getContentResolver");
                 AndroidJavaObject inputStream = contentResolver.Call<AndroidJavaObject>("openInputStream", intentURI);
                 AndroidJavaObject inputChannel = inputStream.Call<AndroidJavaObject>("getChannel");

                 // Open an output channel
                 AndroidJavaObject outputStream = new AndroidJavaObject("java.io.FileOutputStream", importPath);
                 AndroidJavaObject outputChannel = outputStream.Call<AndroidJavaObject>("getChannel");

                 // Copy the file
                 long bytesTransfered = 0;
                 long bytesTotal = inputChannel.Call<long>("size");
                 while (bytesTransfered < bytesTotal)
                 {
                     bytesTransfered += inputChannel.Call<long>("transferTo", bytesTransfered, bytesTotal, outputChannel);
                 }

                 // Close the streams
                 inputStream.Call("close");
                 outputStream.Call("close");
             }
         }
         catch (System.Exception ex)
         {
             // Handle error
         }
    }
             /*
        */
    string resolveSymlinkUsingFile()
    {
        //
        //new File("/storage/ext_sd").toPath().toRealPath().toString()
        string path = "";

        try
        {
            AndroidJavaObject fileInstanceTest = new AndroidJavaObject("java.io.File", "/storage/ext_sd");
            var javaPathObject = fileInstanceTest.Call<AndroidJavaObject>("toPath");
            //tehcnially thie 
            //var realPath = javaPathObject.Call<AndroidJavaObject>("toRealPath",new object[]{});
            var realPath = javaPathObject.Call<AndroidJavaObject>("toRealPath",new object[]{});
            path = realPath.Call<string>("toString");
            Debug.Log($"Path is {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"failed resolving symlink : {e.Message}");
            Debug.LogException(e);
        }
        return path;
    }
    string resolveSymlink(string symlinkPath)
    {
        
        //return result for java "FileSystems.getDefault().getPath("/storage/ext_sd").toRealPath().toString();"
        AndroidJavaClass fileSystemsClass = new AndroidJavaClass("java.nio.file.FileSystems");
        AndroidJavaObject fileSystemInstance = fileSystemsClass.CallStatic<AndroidJavaObject>("getDefault"); // .GetStatic<AndroidJavaObject>("currentActivity");
        Debug.Log($"Is fs null {fileSystemInstance == null} ");
        AndroidJavaClass pathClass = new AndroidJavaClass("java.nio.file.Path");
        AndroidJavaObject pathInstanceTest = new AndroidJavaObject("java.nio.file.Path");
        //AndroidJavaObject pathObject = new AndroidJavaObject("")

        //AndroidJavaObject symlinkPathObjectInstance = fileSystemInstance.Call<AndroidJavaObject>("getPath", symlinkPath); //i think this fails since it's trying to use the Path version instead of the arbitrary string based ones.. could fix this if i did the marshalling myself
        //AndroidJavaObject symlinkPathObjectInstance = fileSystemInstance.Call<AndroidJavaObject>("getPath", "storage","ext_sd"); 
        //AndroidJNIHelper.CreateJNIArgArray()
        //public static IntPtr ConvertToJNIArray(Array array) => _AndroidJNIHelper.ConvertToJNIArray(array);
        //maybe something here... AndroidJNIHelper.ConvertToJNIArray()
        
        //AndroidJavaObject symlinkPathObjectInstance = fileSystemInstance.Call<AndroidJavaObject>("getPath", "storage","ext_sd"); //this still fails
        //AndroidJNIHelper.CreateJNIArgArray(new object[0]);
        // fake --IntPtr method_getData = AndroidJNIHelper.GetMethodID(intent.GetRawClass(), "getData", "()Ljava/lang/Object;");

        //try next... manual thingies
        //AndroidJNIHelper.GetMethodID(fileSystemInstance.GetRawClass(), "getPath", "()Ljava/lang/Object;");
        //IntPtr rawPointerToArray = AndroidJNIHelper.ConvertToJNIArray(new string[]{ symlinkPath});
        //AndroidJavaObject symlinkPathObjectInstance = fileSystemInstance.Call<string>("getPath", rawPointerToArray);
        /*
        AndroidJNIHelper.GetSignature()
        AndroidJNIHelper.GetSignature() + 
        AndroidJavaObject symlinkPathObjectInstance = fileSystemInstance.Call<AndroidJavaObject>("getPath", new []{ new string[]{"/storage","ext_sd"}});
        IntPtr methodId = AndroidJNIHelper.GetMethodID<ReturnType>((IntPtr) this.m_jclass, methodName, args, false);

        var symlinkRealPath = symlinkPathObjectInstance.Call<AndroidJavaObject>("toRealPath");
        string symlinkCSharpString = symlinkRealPath.Call<string>("toString");
                */

        return "symlinkCSharpString";
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
                    //AndroidJavaObject[] externalFilesDirectories = context.Call<AndroidJavaObject[]>("getExternalFilesDirs");
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
    void manualWriteTest()
    {
        //const string DEBUG_SD_CARD_ID_SPECIFIC_TO_AUTHOR_HARDWARE = "52A0-B627"; // is usb stick
        const string DEBUG_SD_CARD_ID_SPECIFIC_TO_AUTHOR_HARDWARE = "3365-3432"; //microsd
        const string fileNamePrefix = "testingfoo";
        var filesToTry = new string[]
        {
            $"/storage/ext_sd/Android/data/{Application.identifier}/files/{fileNamePrefix}1",
            $"/storage/ext_sd/Android/data/{Application.identifier}/cache/{fileNamePrefix}2",
            $"/storage/{DEBUG_SD_CARD_ID_SPECIFIC_TO_AUTHOR_HARDWARE}/Android/data/{Application.identifier}/cache/{fileNamePrefix}3",
            $"/storage/{DEBUG_SD_CARD_ID_SPECIFIC_TO_AUTHOR_HARDWARE}/Android/data/{Application.identifier}/files/{fileNamePrefix}4"
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
        PrintFilesInDirectory(directory,$"{fileNamePrefix}*",logPrefix);
        
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
        const string fileNamePrefix = "testingbar";
        foreach (var rootDirectory in Directory.EnumerateDirectories("/storage"))
        {
            var filePath = $"{rootDirectory}/Android/data/{Application.identifier}/files/{fileNamePrefix}{fileNumber++}";
            try
            {
                testWriteReadAtPath(filePath);
                Debug.Log($"{logPrefix} successfully wrote file {filePath}");
                PrintFilesInDirectory(rootDirectory,$"{fileNamePrefix}*",logPrefix);
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} Failed to write file {filePath}");
                Debug.LogException(e);
            }
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

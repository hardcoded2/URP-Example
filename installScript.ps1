Write-Host "installing apk"
adb install -r -g -d "Builds/test.apk"
Write-Host "installed apk"

Write-Host "double checking that root directories exist"
adb shell mkdir -m777 /mnt/sdcard/Android/data/com.DefaultCompany.AccessStorage/files

"fakefilecontents" | Out-file testfile

echo "pushing sample folder of files"
adb push testfile /mnt/sdcard/Android/data/com.DefaultCompany.AccessStorage/files/testfile

echo "double checking permissions of files"
adb shell chmod 777 /mnt/sdcard/Android/data/com.DefaultCompany.AccessStorage/files

echo "folder of content files should now be located and accessible at /mnt/sdcard/Android/data/com.DefaultCompany.AccessStorage/files"

#/mnt/sdcard/Android/data/com.DefaultCompany.AccessStorage/files

Project set up to show how to use URP with the wave sdk

Sample apks included in CI link

[![CI](https://github.com/hardcoded2/URP-Example/actions/workflows/ci.yml/badge.svg)](https://github.com/hardcoded2/URP-Example/actions/workflows/ci.yml)

Also some branches that are different tests
* filesystem-batchscript - how to upload a file that will be accessable to your app after installing it, having a similar effect to downloading from the internet to Application.persistentDataPath
* sdcard - in progress work showing how to access files on the sdcard
* sdcard_root - showing how to access a file of an external sd card root using chmod. a hack less elegantly than filesystem-batchscript which requires one to chmod 777 a file, as android restricts application usage at the root of the sdcard



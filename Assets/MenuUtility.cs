using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.IO;
using TMPro;
using SFB;

public class MenuUtility : MonoBehaviour
{
    public GameObject obj;
    public void StartGame()
    {
        SceneManager.LoadScene("Eye_Tracking");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BeginCalibration()
    {
        int result = ViveSR.anipal.Eye.SRanipal_Eye_API.LaunchEyeCalibration(System.IntPtr.Zero);
        print(result);
    }

    public void NameSetting()
    {
        TMP_InputField input = obj.GetComponent<TMP_InputField>();
        string temp = input.text;
        PlayerPrefs.SetString(obj.name, input.text);
    }

    public void FileSelect()
    {
        TMP_InputField input = obj.GetComponent<TMP_InputField>();
        var path = StandaloneFileBrowser.OpenFolderPanel("Set File Destination", Application.dataPath, true);
        input.text = path[0];
        PlayerPrefs.SetString(obj.name, path[0]);
    }
}

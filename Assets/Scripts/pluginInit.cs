using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class pluginInit : MonoBehaviour
{
    // Start is called before the first frame update
    private Button multiplyButton, toastButton;
    private AndroidJavaClass unityClass;
    private AndroidJavaObject unityActivity;
    private AndroidJavaObject _plugInstance;
    void Start()
    {
        unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
        _plugInstance = new AndroidJavaObject("com.kinten.unityplugin.PluginInstance");
        if (_plugInstance == null){
            throw new UnityException("Could not find the plugin class specified");
            Debug.Log("Could not find the plugin class specified");
        }
        _plugInstance.CallStatic("getUnityActivity", unityActivity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void onClickMultiplyButton() {
        if (_plugInstance != null) {
            int output = _plugInstance.Call<int>("Product",77,5);
            Debug.Log("Result of Product is "+output);
        }
    }
    public void onClickToastButton() {
        if (_plugInstance != null) {
            _plugInstance.Call("showText","Amazing");
            Debug.Log("text shown");
        }
    }
}

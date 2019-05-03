using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum SteamPunkSetting
{
    PauseWhileOpen,
    ShowUnityLog,
    Count
}

public delegate void OnSettingChanged(SteamPunkSetting setting, bool value);

public class SteamPunkSettings : MonoBehaviour
{
    public Toggle pauseToggle;
    public Toggle logToggle;

    public OnSettingChanged onSettingChanged;
    const string prefName = "spp_setting_";

    bool[] settings = new bool[(int)SteamPunkSetting.Count];

    // Start is called before the first frame update
    void Start()
    {
        pauseToggle.onValueChanged.AddListener(onPauseToggleChanged);
        logToggle.onValueChanged.AddListener(onLogToggleChanged);

        for (int i=0; i < settings.Length; i++)
        {
            SteamPunkSetting setting = (SteamPunkSetting)i;
            settings[i] = PlayerPrefs.GetInt(prefName + setting) == 1;

            SetSetting(setting, settings[i]);
        }
    }

    public bool GetSettingValue(SteamPunkSetting setting)
    {
        return settings[(int)setting];
    }

    void onPauseToggleChanged(bool val)
    {
        SetSetting(SteamPunkSetting.PauseWhileOpen, val);
    }

    void onLogToggleChanged(bool val)
    {
        SetSetting(SteamPunkSetting.ShowUnityLog, val);
    }

    void SetSetting(SteamPunkSetting setting, bool val)
    {
        PlayerPrefs.SetInt(prefName + setting, val ? 1 : 0);
        settings[(int)setting] = val;

        if (onSettingChanged != null)
            onSettingChanged(setting, val);

        switch (setting)
        {
            case SteamPunkSetting.PauseWhileOpen:
                pauseToggle.isOn = val;
                break;
            case SteamPunkSetting.ShowUnityLog:
                logToggle.isOn = val;
                break;
        }

    }
}

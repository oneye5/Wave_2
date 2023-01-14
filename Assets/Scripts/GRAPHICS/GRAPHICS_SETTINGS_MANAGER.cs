using System.Collections;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class GRAPHICS_SETTINGS_MANAGER : MonoBehaviour
{

    public float fov;
    public int aaMode; //0 = none, 1 = fxaa, 2 = taa,3 = smaa
    public bool deferred;
    public int matQuality; // 0 - 2 (low , med, high)
    public bool bloom;
    public bool ssr;

    public int fullscreenMode; //full = 0, window full = 1, 2 = max window, 3 = window
    public Vector2Int resolution;

    private HDAdditionalCameraData cameraData;
    private FrameSettings frameSettings;
    private FrameSettingsOverrideMask frameSettingsOverrideMask;

    public void resetToDefault()
    {
        fov = 90;
        aaMode = 0;
        deferred = true;
        matQuality = 2;
        bloom = true;
        ssr = true;
        fullscreenMode = 3;
        UnityEngine.Debug.Log(Screen.currentResolution.width + " h " + Screen.currentResolution.height);
        resolution = new Vector2Int(Screen.currentResolution.width , Screen.currentResolution.height);


    }
    void init(Camera cam)
    {
        cameraData = Camera.main.GetComponent<HDAdditionalCameraData>();
        frameSettings = cameraData.renderingPathCustomFrameSettings;
        frameSettingsOverrideMask = cameraData.renderingPathCustomFrameSettingsOverrideMask;
        cameraData.customRenderingSettings = true;

        frameSettingsOverrideMask.mask[(uint)FrameSettingsField.LitShaderMode] = true;

        frameSettingsOverrideMask.mask[(uint)FrameSettingsField.MaterialQualityLevel] = true;

        frameSettingsOverrideMask.mask[(uint)FrameSettingsField.Bloom] = true;

        frameSettingsOverrideMask.mask[(uint)FrameSettingsField.SSR] = true;

        frameSettingsOverrideMask.mask[(uint)FrameSettingsField.Vignette] = true;

        frameSettingsOverrideMask.mask[(uint)FrameSettingsField.VirtualTexturing] = true;



        cameraData.renderingPathCustomFrameSettingsOverrideMask = frameSettingsOverrideMask;
    }
    public void setCamProperties(Camera cam)
    {
        init(cam);

        //     if(deferred)
        //       frameSettings.litShaderMode = LitShaderMode.Deferred;
        //else
        //      frameSettings.litShaderMode = LitShaderMode.Forward;

        if(matQuality == 0)
            frameSettings.materialQuality = UnityEngine.Rendering.MaterialQuality.Low;
        else if(matQuality == 1)
            frameSettings.materialQuality = UnityEngine.Rendering.MaterialQuality.Medium;
        else if(matQuality == 2)
            frameSettings.materialQuality = UnityEngine.Rendering.MaterialQuality.High;

        cameraData.antialiasing = (HDAdditionalCameraData.AntialiasingMode)aaMode; //0 = none, 1 = fxaa, 2 = tmaa,3 = smaa

        if(bloom)
            frameSettings.SetEnabled(FrameSettingsField.Bloom , true);
        else
            frameSettings.SetEnabled(FrameSettingsField.Bloom , false);

        if(ssr)
            frameSettings.SetEnabled(FrameSettingsField.SSR , true);
        else
            frameSettings.SetEnabled(FrameSettingsField.SSR , false);
        //other settings

        if(fullscreenMode == 0 || fullscreenMode == 1)
        Screen.SetResolution(Screen.currentResolution.width , Screen.height , true);
        else
            Screen.SetResolution(Screen.currentResolution.width , Screen.height , true);
        Screen.fullScreenMode = (FullScreenMode)fullscreenMode;
        cam.fieldOfView = fov;

        SetFrameSettings(frameSettings);
    }
    private void SetFrameSettings(FrameSettings frameSettings)
    {
        cameraData.renderingPathCustomFrameSettings = frameSettings;
        UnityEngine.Debug.Log("cam settings changed");
    }

    private void Awake()
    {
        GRAPHICS_SETTINGS_MANAGER_REF.Instance = this;
        resetToDefault();
    }
}

public static class GRAPHICS_SETTINGS_MANAGER_REF
{
    public static GRAPHICS_SETTINGS_MANAGER Instance;
}

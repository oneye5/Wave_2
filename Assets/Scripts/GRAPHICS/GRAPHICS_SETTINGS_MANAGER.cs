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
    public int aaMode; //0 = none, 1 = fxaa, 2 = tmaa,3 = smaa
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
        fullscreenMode = 1;
        resolution = new Vector2Int(Screen.currentResolution.width,Screen.currentResolution.height);

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
        
        if(deferred)
            frameSettings.litShaderMode = LitShaderMode.Deferred;
        else
            frameSettings.litShaderMode = LitShaderMode.Forward;

        if(matQuality ==0)
            frameSettings.materialQuality = UnityEngine.Rendering.MaterialQuality.Low;
        else if(matQuality ==1)
            frameSettings.materialQuality = UnityEngine.Rendering.MaterialQuality.Medium;
        else if(matQuality==2)
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
        

        //Screen.SetResolution(resolution.x , resolution.y , false);
        Screen.fullScreenMode = (FullScreenMode)fullscreenMode;


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
    }
    private void Update()
    {
        if(Input.GetKeyDown("0"))
        {
            setCamProperties(Camera.main);
        }
        if(Input.GetKeyDown("1"))
        {
            resetToDefault();
        }


        if(Input.GetKeyDown("e"))
        {
            aaMode = 0;
        }
        if(Input.GetKeyDown("r"))
        {
            aaMode = 1;
        }
        if(Input.GetKeyDown("t"))
        {
            aaMode = 2;
        }
        if(Input.GetKeyDown("y"))
        {
            aaMode = 3;
        }


        if(Input.GetKeyDown("u"))
        {
            fullscreenMode = 0;
        }
        if(Input.GetKeyDown("i"))
        {
            fullscreenMode = 1;
        }
        if(Input.GetKeyDown("o"))
        {
            fullscreenMode = 2;
        }
        if(Input.GetKeyDown("p"))
        {
            fullscreenMode = 3;
        }

        if(Input.GetKeyDown("f"))
        {
            ssr = false;
        }
        if(Input.GetKeyDown("g"))
        {
            ssr = true;
        }

        if(Input.GetKeyDown("h"))
        {
            bloom = false;
        }
        if(Input.GetKeyDown("j"))
        {
            bloom = true;
        }

        if(Input.GetKeyDown("k"))
        {
            matQuality = 0;
        }
        if(Input.GetKeyDown("l"))
        {
            matQuality = 1;
        }
        if(Input.GetKeyDown(";"))
        {
            matQuality = 2;
        }

        if(Input.GetKeyDown("z"))
        {
            deferred = true;
        }
        if(Input.GetKeyDown("x"))
        {
            deferred = false;
        }
    }
}

public static class GRAPHICS_SETTINGS_MANAGER_REF
{
    public static GRAPHICS_SETTINGS_MANAGER Instance;
}

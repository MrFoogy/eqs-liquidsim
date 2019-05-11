using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendCamera : MonoBehaviour, RenderTextureCameraListener {
    public Material mat;
    private DynamicRenderTextureCamera cam1;
    private DynamicRenderTextureCamera cam2;
    public string[] shaderTextureNames;
    public DynamicRenderTextureCamera colorCamera;
    public DynamicRenderTextureCamera depthCamera;

    void Start() {
        SetCam1(colorCamera);
        SetCam2(depthCamera);
    }

    public void SetCam1(DynamicRenderTextureCamera cam) {
        cam1 = cam;
        cam1.AddListener(this);
    }

    public void SetCam2(DynamicRenderTextureCamera cam) {
        cam2 = cam;
        cam2.AddListener(this);
    }

    public void OnRenderTextureChange(DynamicRenderTextureCamera cam, RenderTexture newRenderTexture) {
        if (cam == cam1) {
            mat.SetTexture(shaderTextureNames[0], newRenderTexture);
        } else {
            mat.SetTexture(shaderTextureNames[1], newRenderTexture);
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture dest) {
        Graphics.Blit(source, dest, mat);
    }
}

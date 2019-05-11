using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidPostProcess : MonoBehaviour {
    public Material mat;
    public Camera cam;

    void Start() {
        //cam.depthTextureMode = DepthTextureMode.Depth;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination, mat);
    }
}

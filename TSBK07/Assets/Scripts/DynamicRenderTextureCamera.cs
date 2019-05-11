using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface RenderTextureCameraListener {
    void OnRenderTextureChange(DynamicRenderTextureCamera cam, RenderTexture newRenderTexture);
}

public class DynamicRenderTextureCamera : MonoBehaviour {
    public Camera cam;
    [HideInInspector]
    public Vector2 proportions = Vector2.one;
    private Vector2 prevScreenSize = Vector2.zero;
    private List<RenderTextureCameraListener> listeners = new List<RenderTextureCameraListener>();

    void Update() {
        if (cam.targetTexture == null || prevScreenSize.x != Screen.width || prevScreenSize.y != Screen.height) {
            UpdateRenderTexture();
        }
    }

    public void AddListener(RenderTextureCameraListener listener) {
        listener.OnRenderTextureChange(this, GetRenderTexture());
        listeners.Add(listener);
    }

    public void UpdateRenderTexture() {
        if (cam.targetTexture != null) {
            cam.targetTexture.Release();
        }

        cam.targetTexture = new RenderTexture(Mathf.RoundToInt(Screen.width * proportions.x), 
            Mathf.RoundToInt(Screen.height * proportions.y), 24);
        prevScreenSize = new Vector2(Screen.width, Screen.height);
        foreach (RenderTextureCameraListener listener in listeners) {
            listener.OnRenderTextureChange(this, cam.targetTexture);
        }
    }

    public RenderTexture GetRenderTexture() {
        if (cam.targetTexture == null) {
            UpdateRenderTexture();
        }
        return cam.targetTexture;
    }
}

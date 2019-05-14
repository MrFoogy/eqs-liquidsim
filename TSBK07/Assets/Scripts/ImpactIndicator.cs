using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ImpactIndicatorParams {
    public Color color;
    public float radius;

    public ImpactIndicatorParams(Color color, float radius) {
        this.color = color;
        this.radius = radius;
    }
}

public class ImpactIndicator : MonoBehaviour {
    public Renderer renderer;
    public float textureScrollSpeed;

    void Update() {
        renderer.material.SetTextureOffset("_MainTex", renderer.material.GetTextureOffset("_MainTex") + Vector2.one * Time.deltaTime * textureScrollSpeed);
    }

    public void ApplyParams(ImpactIndicatorParams indicatorParams) {
        transform.localScale = Vector3.one * indicatorParams.radius * 2f;
        renderer.material.SetTextureScale("_MainTex", renderer.material.GetTextureScale("_MainTex") * indicatorParams.radius);
        //renderer.material.SetColor("_Color", new Color(indicatorParams.color.r, indicatorParams.color.g, indicatorParams.color.b, renderer.material.GetColor("_Color").a));
        renderer.material.SetColor("_EmissionColor", indicatorParams.color);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidReplacementShader : MonoBehaviour {
    public Shader replacementShader;
    public Camera cam;

    void Start() {
        cam.SetReplacementShader(replacementShader, "");
    }
}

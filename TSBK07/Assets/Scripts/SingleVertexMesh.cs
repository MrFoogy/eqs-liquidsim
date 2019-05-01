using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleVertexMesh : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        Mesh mesh = new Mesh();
        Vector3[] newVertices = new Vector3[1];
        newVertices[0] = new Vector3(0f, 0f, 0f);
        Vector2[] newUV = new Vector2[1];
        newUV[0] = new Vector2(0f, 0f);
        mesh.vertices = newVertices;
        mesh.uv = newUV;
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.SetIndices(new int[] { 0 }, MeshTopology.Points, 0);
    }
}

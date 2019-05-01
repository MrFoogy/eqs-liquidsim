using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;

/*
 * Implementation of SPH in Unity ECS
 */
public class LiquidManager : MonoBehaviour {
    private EntityManager manager;

    public GameObject particlePrefab;
    public MeshFilter meshFilter;
    public Material liquidMaterial;

    // For particle placement
    public int startParticles;
    public float boxSide;
    private List<Particle> particles = new List<Particle>();

    void Start() {
        manager = World.Active.EntityManager;

        Mesh mesh = new Mesh();
        Vector3[] newVertices = new Vector3[1];
        newVertices[0] = new Vector3(0f, 0f, 0f);
        Vector2[] newUV = new Vector2[1];
        newUV[0] = new Vector2(0f, 0f);
        mesh.vertices = newVertices;
        mesh.uv = newUV;
        GetComponent<MeshFilter>().mesh = mesh;
        mesh.SetIndices(new int[] { 0 }, MeshTopology.Points, 0);

        InitParticles();
    }

    private void InitParticles() {
        NativeArray<Entity> entities = new NativeArray<Entity>(startParticles, Allocator.Temp);
        manager.Instantiate(particlePrefab, entities);
        for (int i = 0; i < startParticles; i++) {
            float x = UnityEngine.Random.Range(0f, boxSide);
            float y = UnityEngine.Random.Range(0f, boxSide);
            float z = UnityEngine.Random.Range(0f, boxSide);
            manager.SetComponentData(entities[i], new Translation { Value = new float3(x, y, z) });
            manager.SetSharedComponentData(entities[i], new RenderMesh { mesh = meshFilter.sharedMesh, castShadows = UnityEngine.Rendering.ShadowCastingMode.Off,
                subMesh = 0, layer = 0, material = liquidMaterial, receiveShadows = false });
        }
        entities.Dispose();
    }
}

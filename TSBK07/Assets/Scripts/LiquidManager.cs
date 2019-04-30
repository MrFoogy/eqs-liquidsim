using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

/*
 * Implementation of SPH in Unity ECS
 */
public class LiquidManager : MonoBehaviour {
    private EntityManager manager;

    public GameObject particlePrefab;

    // For particle placement
    public int startParticles;
    public float boxSide;
    private List<Particle> particles = new List<Particle>();

    void Start() {
        manager = World.Active.EntityManager;
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
        }
        entities.Dispose();
    }
}

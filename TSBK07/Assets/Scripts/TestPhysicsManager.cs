using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Physics;
using Collider = Unity.Physics.Collider;

public class TestPhysicsManager : MonoBehaviour {
    private EntityManager manager;

    public GameObject particlePrefab;

    // For particle placement
    public int startParticles;
    public float boxSide;

    void OnEnable() {
        if (isActiveAndEnabled) {
            manager = World.Active.EntityManager;
            InitParticles();
        }
    }

    private void InitParticles() {
        Entity sourceEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(particlePrefab, World.Active);
        BlobAssetReference<Collider> sourceCollider = manager.GetComponentData<PhysicsCollider>(sourceEntity).Value;
        Unity.Mathematics.Random random = new Unity.Mathematics.Random();
        for (int i = 0; i < startParticles; i++) {
            var instance = manager.Instantiate(sourceEntity);
            float x = UnityEngine.Random.Range(0f, boxSide);
            float y = UnityEngine.Random.Range(0f, boxSide);
            float z = UnityEngine.Random.Range(0f, boxSide);
            manager.SetComponentData(instance, new Translation { Value = new float3(x, y, z) });
            manager.SetComponentData(instance, new PhysicsCollider { Value = sourceCollider });
        }
    }
}

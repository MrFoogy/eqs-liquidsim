using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using Material = UnityEngine.Material;
using Ray = Unity.Physics.Ray;
using RaycastHit = Unity.Physics.RaycastHit;
using Unity.Mathematics;

public class TadpoleLiquidIntersection : MonoBehaviour {
    public Renderer tadpoleRenderer;
    public Material collideMaterial;
    public Material noCollideMaterial;
    public float collisionRadius;
    public CollisionFilter collisionFilter;
    public CharacterController characterController;
    [HideInInspector]
    public bool isInLiquid;

    // Start is called before the first frame update
    void Start() {


    }

    // Update is called once per frame
    void Update() {
        isInLiquid = TestLiquidCollision();
        tadpoleRenderer.material = (isInLiquid || !characterController.isGrounded) ? collideMaterial : noCollideMaterial;
    }

    public bool TestLiquidCollision() {
        var physicsWorldSystem = Unity.Entities.World.Active.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        PointDistanceInput input = new PointDistanceInput() {
            MaxDistance = collisionRadius,
            Position = transform.position,
            Filter = new CollisionFilter() {
                MaskBits = (1 << 1),
                CategoryBits = (1 << 0),
                GroupIndex = 0
            }
        };
        return collisionWorld.CalculateDistance(input);
    }
}

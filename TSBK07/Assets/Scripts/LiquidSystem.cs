using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;

/*
 * Code is largely based on: https://medium.com/@leomontes_60748/how-to-implement-a-fluid-simulation-on-the-cpu-with-unity-ecs-job-system-bf90a0f2724f
 */
[UpdateBefore(typeof(TransformSystemGroup))]
public class LiquidSystem : JobComponentSystem{
    private EntityQuery particlesQuery;
    private List<LiquidParticle> liquidTypes = new List<LiquidParticle>(10);
    private List<PreviousParticle> previousParticles = new List<PreviousParticle>();

    private static readonly int[] cellOffsetTable =
    {
        1, 1, 1, 1, 1, 0, 1, 1, -1, 1, 0, 1, 1, 0, 0, 1, 0, -1, 1, -1, 1, 1, -1, 0, 1, -1, -1,
        0, 1, 1, 0, 1, 0, 0, 1, -1, 0, 0, 1, 0, 0, 0, 0, 0, -1, 0, -1, 1, 0, -1, 0, 0, -1, -1,
        -1, 1, 1, -1, 1, 0, -1, 1, -1, -1, 0, 1, -1, 0, 0, -1, 0, -1, -1, -1, 1, -1, -1, 0, -1, -1, -1
    };

    private struct PreviousParticle
    {
        public NativeMultiHashMap<int, int> hashMap;
        public NativeArray<Translation> copyParticlesPosition;
        public NativeArray<LiquidParticleVelocity> copyParticlesVelocity;
        public NativeArray<float3> particlesForces;
        public NativeArray<float> particlesPressure;
        public NativeArray<float> particlesDensity;

        public NativeArray<int> cellOffsetTable;
    }

    [BurstCompile]
    private struct HashPositions : IJobParallelFor {
        [ReadOnly] public float cellRadius;

        // TODO: is the positions part really needed?
        public NativeArray<Translation> positions;
        public NativeMultiHashMap<int, int>.Concurrent hashMap;

        public void Execute(int index) {
            float3 position = positions[index].Value;

            int hash = GridHash.Hash(position, cellRadius);
            hashMap.Add(hash, index);

            positions[index] = new Translation { Value = position };
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps) {
        EntityManager.GetAllUniqueSharedComponentData(liquidTypes);

        // Ignore typeIndex 0, can't use the default for anything meaningful.
        for (int typeIndex = 1; typeIndex < liquidTypes.Count; typeIndex++) {
            LiquidParticle particleSettings = liquidTypes[typeIndex];
            particlesQuery.SetFilter(particleSettings);

            int cacheIndex = typeIndex - 1;
            int particleCount = particlesQuery.CalculateLength();

            NativeMultiHashMap<int, int> hashMap = new NativeMultiHashMap<int, int>(particleCount, Allocator.TempJob);

            //NativeArray<Translation> particlesPosition = new NativeArray<Translation>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            //NativeArray<LiquidParticleVelocity> particlesVelocity = new NativeArray<LiquidParticleVelocity>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<float3> particlesForces = new NativeArray<float3>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<float> particlesPressure = new NativeArray<float>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<float> particlesDensity = new NativeArray<float>(particleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            NativeArray<int> cellOffsetTableNative = new NativeArray<int>(cellOffsetTable, Allocator.TempJob);

            var copyParticlesPositions = particlesQuery.ToComponentDataArray<Translation>(Allocator.TempJob, out var particlesPositionJobHandle);
            var copyParticlesVelocities = particlesQuery.ToComponentDataArray<LiquidParticleVelocity>(Allocator.TempJob, out var particlesVelocityJobHandle);

            PreviousParticle nextParticle = new PreviousParticle {
                hashMap = hashMap,
                copyParticlesPosition = copyParticlesPositions,
                copyParticlesVelocity = copyParticlesVelocities,
                particlesForces = particlesForces,
                particlesPressure = particlesPressure,
                particlesDensity = particlesDensity,
                cellOffsetTable = cellOffsetTableNative
            };
            if (cacheIndex > previousParticles.Count - 1) {
                previousParticles.Add(nextParticle);
            } else {
                previousParticles[cacheIndex].hashMap.Dispose();
                previousParticles[cacheIndex].copyParticlesPosition.Dispose();
                previousParticles[cacheIndex].copyParticlesVelocity.Dispose();
                previousParticles[cacheIndex].particlesForces.Dispose();
                previousParticles[cacheIndex].particlesPressure.Dispose();
                previousParticles[cacheIndex].particlesDensity.Dispose();
                previousParticles[cacheIndex].cellOffsetTable.Dispose();
            }
            previousParticles[cacheIndex] = nextParticle;

            MemsetNativeArray<float> particlesPressureJob = new MemsetNativeArray<float> { Source = particlesPressure, Value = 0f };
            JobHandle particlesPressureJobHandle = particlesPressureJob.Schedule(particleCount, 64, inputDeps);

            MemsetNativeArray<float> particlesDensityJob = new MemsetNativeArray<float> { Source = particlesDensity, Value = 0f };
            JobHandle particlesDensityJobHandle = particlesDensityJob.Schedule(particleCount, 64, inputDeps);

            MemsetNativeArray<float3> particlesForceJob = new MemsetNativeArray<float3> { Source = particlesForces, Value = new float3(0f, 0f, 0f) };
            JobHandle particlesForceJobHandle = particlesForceJob.Schedule(particleCount, 64, inputDeps);


            // Hash the positions
            HashPositions hashPositionsJob = new HashPositions {
                positions = copyParticlesPositions,
                hashMap = hashMap.ToConcurrent(),
                cellRadius = particleSettings.kernelRadius
            };
            JobHandle hashPositionsJobHandle = hashPositionsJob.Schedule(particleCount, 64, particlesPositionJobHandle);

            JobHandle mergeHandle1 = JobHandle.CombineDependencies(hashPositionsJobHandle, particlesPressureJobHandle, particlesDensityJobHandle);

            ComputeDensityPressure computeDensityPressureJob = new ComputeDensityPressure {
                particlesPosition = copyParticlesPositions,
                densities = particlesDensity,
                pressures = particlesPressure,
                settings = particleSettings
            };
            JobHandle computeDensityPressureJobHandle = computeDensityPressureJob.Schedule(particleCount, 64, mergeHandle1);

            JobHandle mergeHandle2 = JobHandle.CombineDependencies(computeDensityPressureJobHandle, particlesForceJobHandle, particlesVelocityJobHandle);

            ComputeForces computeForcesJob = new ComputeForces {
                particlesPosition = copyParticlesPositions,
                particlesVelocity = copyParticlesVelocities,
                particlesForces = particlesForces,
                particlesPressure = particlesPressure,
                particlesDensity = particlesDensity,
                settings = particleSettings
            };
            JobHandle computeForcesJobHandle = computeForcesJob.Schedule(particleCount, 64, mergeHandle2);

            Integrate integrateJob = new Integrate {
                particlesPosition = copyParticlesPositions,
                particlesVelocity = copyParticlesVelocities,
                particlesDensity = particlesDensity,
                particlesForces = particlesForces
            };
            JobHandle integrateJobHandle = integrateJob.Schedule(particlesQuery, computeForcesJobHandle);

            inputDeps = integrateJobHandle;
        }
        liquidTypes.Clear();
        return inputDeps;
    }

    protected override void OnStopRunning() {
        for (int i = 0; i < previousParticles.Count; i++) {
            previousParticles[i].hashMap.Dispose();
            previousParticles[i].copyParticlesPosition.Dispose();
            previousParticles[i].copyParticlesVelocity.Dispose();
            previousParticles[i].particlesForces.Dispose();
            previousParticles[i].particlesPressure.Dispose();
            previousParticles[i].particlesDensity.Dispose();
            previousParticles[i].cellOffsetTable.Dispose();
        }
        previousParticles.Clear();
    }

    protected override void OnCreateManager() {
        particlesQuery = GetEntityQuery(ComponentType.ReadOnly(typeof(LiquidParticle)), typeof(Translation), typeof(LiquidParticleVelocity));
    }

    [BurstCompile]
    private struct ComputeDensityPressure : IJobParallelFor {
        [ReadOnly] public NativeArray<Translation> particlesPosition;
        [ReadOnly] public LiquidParticle settings;
        public NativeArray<float> densities;
        public NativeArray<float> pressures;

        private const float PI = 3.14159274F;
        private const float GAS_CONST = 2000.0f;

        public void Execute(int index) {
            int particleCount = particlesPosition.Length;
            float3 position = particlesPosition[index].Value;
            float density = 0f;
            for (int j = 0; j < particleCount; j++) {
                float3 rij = particlesPosition[j].Value - position;
                float r2 = math.lengthsq(rij);
                if (r2 < settings.kernelRadiusSquared) {
                    density += settings.particleMass * (315.0f / (64.0f * PI * math.pow(settings.kernelRadius, 9.0f))) * math.pow(settings.kernelRadiusSquared - r2, 3.0f);
                }
            }

            densities[index] = density;
            pressures[index] = GAS_CONST * (density - settings.restDensity);
        }
    }

    [BurstCompile]
    private struct ComputeForces : IJobParallelFor {
        [ReadOnly] public NativeArray<Translation> particlesPosition;
        [ReadOnly] public NativeArray<LiquidParticleVelocity> particlesVelocity;
        [ReadOnly] public NativeArray<float> particlesPressure;
        [ReadOnly] public NativeArray<float> particlesDensity;
        [ReadOnly] public LiquidParticle settings;

        public NativeArray<float3> particlesForces;
        private const float PI = 3.14159274F;

        public void Execute(int index) {
            int particleCount = particlesPosition.Length;
            float3 position = particlesPosition[index].Value;
            float3 velocity = particlesVelocity[index].Value;
            float pressure = particlesPressure[index];
            float density = particlesDensity[index];

            float3 pressureForce = new float3(0f, 0f, 0f);
            float3 viscosityForce = new float3(0f, 0f, 0f);

            for (int j = 0; j < particleCount; j++) {
                if (index == j) continue;
                float3 rij = particlesPosition[j].Value - position;
                float r2 = math.lengthsq(rij);
                float r = math.sqrt(r2);
                if (r < settings.kernelRadius) {
                    pressureForce += -math.normalize(rij) * settings.particleMass * (pressure + particlesPressure[j]) / (2.0f * density) * 
                        (-45.0f / (PI * math.pow(settings.kernelRadius, 6.0f))) * math.pow(settings.kernelRadius - r, 2.0f);

                    viscosityForce += settings.viscosity * settings.particleMass * (particlesVelocity[j].Value - velocity) / density * 
                        (45.0f / (PI * math.pow(settings.kernelRadius, 6.0f))) * (settings.kernelRadius - r);
                }
            }

            float3 gravityForce = new float3(0f, -9.81f, 0f) * density * settings.gravity;
            particlesForces[index] = pressureForce + viscosityForce + gravityForce;
        }
    }

    [BurstCompile]
    private struct Integrate : IJobForEachWithEntity<Translation, LiquidParticleVelocity> {
        [ReadOnly] public NativeArray<float3> particlesForces;
        [ReadOnly] public NativeArray<float> particlesDensity;

        public NativeArray<Translation> particlesPosition;
        public NativeArray<LiquidParticleVelocity> particlesVelocity;
        private const float DELTA_TIME = 0.001f;

        public void Execute(Entity entity, int index, ref Translation translation, ref LiquidParticleVelocity particleVelocity) {
            float3 position = particlesPosition[index].Value;
            float3 velocity = particlesVelocity[index].Value;

            velocity += DELTA_TIME * particlesForces[index] / particlesDensity[index];
            position += DELTA_TIME * velocity;
            if (position.y < 0f) {
                position.y = 0f;
                velocity *= -0.2f;
            }

            /*
            particlesVelocity[index] = new LiquidParticleVelocity { Value = velocity };
            particlesPosition[index] = new Translation { Value = position };
            */

            translation = new Translation {
                Value = position
            };
            particleVelocity = new LiquidParticleVelocity {
                Value = velocity
            };
        }
    }

    // Need some other way to apply back??
    /*
     [BurstCompile]
     private struct ApplyPositions : IJobParallelFor {
        [ReadOnly] public NativeArray<Translation> particlesPosition;
        [ReadOnly] public NativeArray<LiquidParticleVelocity> particlesVelocity;
    }
    */
}

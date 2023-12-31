﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[System.Serializable]
public struct LiquidParticle : ISharedComponentData {
    // Simulation properties
    public float viscosity;
    public float particleMass;
    public float kernelRadius; 
    public float kernelRadiusSquared; 
    public float restDensity;
}

public class LiquidParticleComponent : SharedComponentDataProxy<LiquidParticle> {
}

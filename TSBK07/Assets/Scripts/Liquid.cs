using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Particle {
    public Particle(Vector3 startPos, GameObject particleObj) {
        pos = startPos;
        vel = Vector3.zero;
        force = Vector3.zero;
        rho = 0f;
        p = 0f;
        this.particleObj = particleObj;
    }
    public Vector3 pos, vel, force;
    // Density
    public float rho;
    // Pressure
    public float p;
    // Visualization
    public GameObject particleObj;
}

/*
 * Implementation of SPH
 */
public class Liquid : MonoBehaviour {
    public GameObject particlePrefab;

    // For particle placement
    public int startParticles;
    public float boxSide;
    public float deltaTime;
    private List<Particle> particles = new List<Particle>();

    // Simulation properties
    public float viscosity;
    public float particleMass;
    public float kernelRadius; 
    public float restDensity;
    public float gravity;

    private const float GAS_CONST = 2000.0f;
    // Smoothing kernels
    private float squaredKernelRadius;
    private float poly6Coeff;
    private float spikyCoeff;
    private float viscCoeff;

    void Start() {
        ComputeConstants();
        InitParticles();
    }

    void Update() {
        ComputeDensityPressuse();
        ComputeForces();
        Integrate();
        ApplyPositions();
    }

    private void ApplyPositions() {
        foreach (Particle p in particles) {
            p.particleObj.transform.position = p.pos;
        }
    }

    private void ComputeConstants() {
        poly6Coeff = 315f / (65f * Mathf.PI * Mathf.Pow(kernelRadius, 9f));
        spikyCoeff = -45f / (Mathf.PI * Mathf.Pow(kernelRadius, 6f));
        viscCoeff = 45f / (Mathf.PI * Mathf.Pow(kernelRadius, 6f));
        squaredKernelRadius = kernelRadius * kernelRadius;
    }

    private void InitParticles() {
        for (int i = 0; i < startParticles; i++) {
            float x = Random.Range(0f, boxSide);
            float y = Random.Range(0f, boxSide);
            float z = Random.Range(0f, boxSide);
            particles.Add(new Particle(new Vector3(x, y, z), GameObject.Instantiate(particlePrefab)));
        }
    }

    private void Integrate() {
        foreach (Particle p in particles) {
            p.vel += deltaTime * (p.force / p.rho);
            p.pos += deltaTime * p.vel;
            if (p.pos.y <= 0f) {
                p.pos.y = 0f;
                p.vel *= -0.2f;
            }
        }
    }

    private void ComputeDensityPressuse() {
        foreach (Particle pi in particles) {
            pi.rho = 0f;
            foreach (Particle pj in particles) {
                Vector3 rij = pj.pos - pi.pos;
                float r2 = rij.sqrMagnitude;
                // Use squared kernel radius for efficiency
                if (r2 < squaredKernelRadius) {
                    // Use the Poly6 smoothing kernel
                    pi.rho += particleMass * poly6Coeff * Mathf.Pow(squaredKernelRadius - r2, 3f);
                }
            }
            // Use ideal gas law to calculate pressure
            pi.p = GAS_CONST * (pi.rho - restDensity);
        }
    }

    private void ComputeForces() {
        foreach (Particle pi in particles) {
            Vector3 pressureForce = Vector3.zero;
            Vector3 viscosityForce = Vector3.zero;
            foreach (Particle pj in particles) {
                if (pj == pi) continue;
                Vector3 rij = pi.pos - pj.pos;
                float r = rij.magnitude;
                if (r < kernelRadius) {
                    pressureForce += -rij.normalized * particleMass * (pi.p + pj.p) / (2f * pj.rho) * spikyCoeff * Mathf.Pow(kernelRadius - r, 2f);
                    viscosityForce += viscosity * particleMass * (pj.vel - pi.vel) / pj.rho * viscCoeff *(kernelRadius - r);
                }
            }
            Vector3 gravityForce = Vector3.down * gravity * pi.rho;
            pi.force = pressureForce + viscosityForce + gravityForce;
        }
    }
}

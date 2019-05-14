using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LiquidSpawner : MonoBehaviour, ArcSpeedProvider {
    public float launchVspeed;
    public Camera cam;
    public ArcIndicator arcIndicator;
    public LiquidManager liquidManager;
    public float spawnRate;
    private float spawnCounter = 0f;
    public float spawnOffset;
    public float velocity;
    private Vector3 aimPoint = Vector3.zero;
    // Compensate for drag
    public float hSpeedMult;
    public Text particlesText;
    private int particles = 0;

    void Start() {
        //arcIndicator.SetSpeedProvider(this);
    }

    void Update() {
        aimPoint = GetAimPoint();
        //arcIndicator.UpdateIndicator(aimPoint);
        Vector3 direction = Vector3.ProjectOnPlane(aimPoint - transform.position, Vector3.up).normalized;
        if (Input.GetMouseButton(0)) {
            spawnCounter += Time.deltaTime * spawnRate;
            if (spawnCounter > 1f) {
                spawnCounter -= 1;

                particles++;
                liquidManager.SpawnLiquidParticle(transform.position + Random.onUnitSphere * spawnOffset, direction * GetHSpeed() * hSpeedMult + Vector3.up * GetVSpeed());
            }
            particlesText.text = "Particles: " + particles;
        }
    }

    public float GetHSpeed() {
        return PhysicsUtil.GetHSpeedForArc(transform.position, aimPoint, launchVspeed, PhysicsUtil.GetGravity());
    }

    private Vector3 GetAimPoint() {
        // TODO: Improve performance of this
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("Default"))) {
            return hit.point;
        } else {
            return Vector3.zero;
        }
    }

    public float GetVSpeed() {
        return launchVspeed;
    }
}

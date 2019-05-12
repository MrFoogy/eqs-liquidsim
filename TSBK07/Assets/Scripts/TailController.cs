using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailController : MonoBehaviour {
    public Transform tailAttachBone;
    public Transform[] tailBones;
    public float convergenceRate;
    // This essentially nullifies the parent-child transform updates
    // Also, these do not store any of the sine wave information
    private Quaternion[] previousRotations;
    private Vector3[] previousPositions;

    private float[] attachDistances;
    public float tailSegmentMoveSpeed;
    public float sineFrequency;
    public float sineIntensity;
    public float intensityGrowthFactor;
    public float sineSpeed;
    public Vector3 sineAxis;

    void Start() {
        previousRotations = new Quaternion[tailBones.Length];
        previousPositions = new Vector3[tailBones.Length];
        attachDistances = new float[tailBones.Length];
        Transform previous = tailAttachBone;
        for (int i = 0; i < tailBones.Length; i++) {
            previousPositions[i] = tailBones[i].position;
            attachDistances[i] = Vector3.Distance(tailBones[i].position, previous.position);
            previous = tailBones[i];
        }
    }

    // Update is called once per frame
    void LateUpdate() {
        Transform parentBone = tailAttachBone;
        for (int i = 0; i < tailBones.Length; i++) {
            tailBones[i].position = previousPositions[i];
            tailBones[i].rotation = previousRotations[i];
        }
        Vector3 parentPos = tailAttachBone.position;
        Quaternion parentRot = tailAttachBone.rotation;
        for (int i = 0; i < tailBones.Length; i++) {
            Vector3 movedPosition = previousPositions[i]/* + moveDirection * Mathf.Min((movementAmount * 0.02f), moveMagn)*/;
            // Make sure it's still at attachDistance
            Vector3 newDirection = parentPos - movedPosition;
            float newDist = newDirection.magnitude;
            newDirection.Normalize();
            tailBones[i].position = movedPosition + newDirection * (newDist - attachDistances[i]);
            // Magic rotation to compensate for weird blender model rotation for bones
            tailBones[i].rotation = Quaternion.LookRotation(newDirection, parentRot * Vector3.up);
            tailBones[i].rotation *= Quaternion.AngleAxis(-90f, Vector3.up);

            parentPos = tailBones[i].position;
            parentRot = tailBones[i].rotation;
        }
        for (int i = 0; i < tailBones.Length; i++) {
            previousPositions[i] = tailBones[i].position;
            previousRotations[i] = tailBones[i].rotation;
        }
        Vector2 sinPos = new Vector2(0f, 0f);
        float currentSineIntensity = sineIntensity;
        for (int i = 0; i < tailBones.Length; i++) {
            // Find the new x-coordinate for the sine wave
            float boneLength = attachDistances[i];
            float newX = sinPos.x + boneLength * sineFrequency;
            float targetY = Mathf.Sin(newX + sineSpeed * Time.time) * currentSineIntensity;
            Vector2 delta = new Vector2(newX, targetY) - sinPos;
            delta = delta.normalized * boneLength * sineFrequency;
            currentSineIntensity += delta.x * intensityGrowthFactor;
            float relativeAngle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            Quaternion newRot = previousRotations[i] * Quaternion.AngleAxis(relativeAngle, sineAxis);
            tailBones[i].rotation = newRot;
            sinPos = sinPos + delta;
        }
        // Apply sine wave
        /*
        Transform parentBone = tailAttachBone;
        for (int i = 0; i < tailBones.Length; i++) {
            tailBones[i].rotation = previousRotations[i];
        }
        for (int i = 0; i < tailBones.Length; i++) {
            previousRotations[i] = Quaternion.RotateTowards(tailBones[i].rotation, parentBone.rotation, convergenceRate * Time.deltaTime);
            parentBone = tailBones[i];
        }
        for (int i = 0; i < tailBones.Length; i++) {
            tailBones[i].rotation = previousRotations[i];
        }
        */
    }
}

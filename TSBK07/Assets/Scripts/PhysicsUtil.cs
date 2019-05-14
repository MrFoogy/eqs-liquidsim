using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsUtil {
    public static float GetGravity() {
        return Physics.gravity.magnitude;
    }

    public static float GetHSpeedForArc(Vector3 origin, Vector3 destination, float vspeed, float gravity, float maxHSpeed = -1f) {
        Vector3 delta = destination - origin;
        float time = GetArcTimeAtHeight(origin.y, vspeed, gravity, destination.y);
        if (float.IsNaN(time)) {
            return -1f;
        } else {
            float hDist = Vector3.ProjectOnPlane(delta, Vector3.up).magnitude;
            float hSpeed = hDist / time;
            if (maxHSpeed != -1f && hSpeed > maxHSpeed) {
                hSpeed = maxHSpeed;
            }
            return hSpeed;
        }
    }

    public static float GetArcTimeAtHeight(float startY, float startVelY, float gravity, float endY) {
        return GetBothArcTimesAtHeight(startY, startVelY, gravity, endY)[1];
    }

    public static float[] GetBothArcTimesAtHeight(float startY, float startVelY, float gravity, float endY) {
        float p = startVelY / (-gravity / 2f);
        float q = (startY - endY) / (-gravity / 2f);
        float[] res = new float[2];
        res[0] = -p / 2f - Mathf.Sqrt(p * p / 4f - q);
        res[1] = -p / 2f + Mathf.Sqrt(p * p / 4f - q);
        return res;
    }

    public static float GetArcHeightAtTime(float startVelY, float gravity, float t) {
        return startVelY * t - (gravity * t * t / 2f);
    }

    public static float GetArcXAtTime(float startVelX, float t) {
        return startVelX * t;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ArcSpeedProvider {
    float GetHSpeed();
    float GetVSpeed();
}

public class ArcIndicator : MonoBehaviour {
    public Transform source;
    private ArcSpeedProvider speedProvider;
    [HideInInspector]
    public Mesh arcMesh;
    public int segments;
    public float width;
    [HideInInspector]
    public float startVelX;
    [HideInInspector]
    public float startVelY;
    [HideInInspector]
    public float endTime;
    [HideInInspector]
    public float endY;
    public float gravity;
    private float maxHSpeed;
    private Vector3 lastPosition;
    private bool useEndY = true;
    private bool useDirectionalAim = false;
    public GameObject arrowHead;
    public ImpactIndicator impactIndicator;

	// Use this for initialization
	void Start () {
        arcMesh = GetComponent<MeshFilter>().mesh;
        arcMesh.Clear();
	}

    /*
    void Update() {
        Recalculate();
        if (lastPosition != transform.position) {
            lastPosition = transform.position;
        }
    }

    void OnValidate() {
        if (arcMesh != null && Application.isPlaying) {
            Recalculate();
        }    
    }
    */

    public void SetImpactIndicatorParams(ImpactIndicatorParams impactParams) {
        if (impactParams == null && impactParams.radius > 0f) {
            impactIndicator.gameObject.SetActive(false);
        } else {
            impactIndicator.gameObject.SetActive(true);
            impactIndicator.ApplyParams(impactParams);
        }
    }
    
    public void SetSpeedProvider(ArcSpeedProvider speedProvider) {
        this.speedProvider = speedProvider;
    }

    public void SetUseDirectionalAim(bool useDirectionalAim) {
        this.useDirectionalAim = useDirectionalAim;
    }

    public void UpdateIndicator(Vector3 aimPoint) {
        // Vspeed is set to constant
        // Calculate the hspeed needed to reach the target, clamped by maxHSpeed
        transform.position = source.position;
        //endY = player.playerInput.GetAimPoint().y;
        endY = -20f;
        //startVelX = PhysicsUtil.GetHSpeedForArc(source.position, targetPos, startVelY, gravity, maxHSpeed);
        startVelX = speedProvider.GetHSpeed();
        startVelY = speedProvider.GetVSpeed();
        Vector3 dir = Vector3.ProjectOnPlane(aimPoint - source.position, Vector3.up);
        transform.rotation = Quaternion.LookRotation(dir);
        Recalculate();
    }

    private void Hide() {
        arcMesh.Clear();
    }

    public void Recalculate() {
        if (useEndY) endTime = PhysicsUtil.GetArcTimeAtHeight(transform.position.y, startVelY, gravity, endY);
        CreateArcMesh(CalculateArcVerts(endTime, true));
    }

    private void CreateArcMesh(Vector3[] arcVerts) {
        arcMesh.Clear();
        if (arcVerts.Length == 0) {
            // Could not generate arc verts, exit
            return;
        }
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        int[] triangles = new int[segments * 6 * 2];
        for (int i = 0; i <= segments; i++) {
            vertices[i * 2] = new Vector3(width * 0.5f, arcVerts[i].y, arcVerts[i].z);
            vertices[i * 2 + 1] = new Vector3(width * -0.5f, arcVerts[i].y, arcVerts[i].z);
            if (i != segments) {
                triangles[i * 12] = i * 2;
                triangles[i * 12 + 1] = triangles[i * 12 + 4] = (i + 1) * 2;
                triangles[i * 12 + 2] = triangles[i * 12 + 3] = i * 2 + 1;
                triangles[i * 12 + 5] = (i + 1) * 2 + 1;

                triangles[i * 12 + 6] = i * 2;
                triangles[i * 12 + 7] = triangles[i * 12 + 10] = i * 2 + 1; 
                triangles[i * 12 + 8] = triangles[i * 12 + 9] = (i + 1) * 2;
                triangles[i * 12 + 11] = (i + 1) * 2 + 1;
            }
        }
        int len = vertices.Length;
        if (arrowHead != null) {
            Vector3 arrowPosition = Vector3.Lerp(GetInWorldSpace(vertices[len - 2]), GetInWorldSpace(vertices[len - 1]), 0.5f);
            Vector3 arrowDirection = GetInWorldSpace(vertices[len - 1]) - GetInWorldSpace(vertices[len - 3]);
            arrowHead.transform.position = arrowPosition;
            arrowHead.transform.rotation = Quaternion.LookRotation(arrowDirection, Vector3.up);
            // Move back the last vertices a little
            vertices[len - 1] = Vector3.Lerp(vertices[len - 3], vertices[len - 1], 0.5f);
            vertices[len - 2] = Vector3.Lerp(vertices[len - 4], vertices[len - 2], 0.5f);
        }
        if (impactIndicator != null && impactIndicator.isActiveAndEnabled) {;
            Vector3 pos = GetInWorldSpace(vertices[len - 1]);
            impactIndicator.transform.position = pos;
            impactIndicator.transform.rotation = Quaternion.identity;
        }

        arcMesh.vertices = vertices;
        arcMesh.triangles = triangles;
    }

    /*
     * Calculate an arc (single line) of vertices
     */
    private Vector3[] CalculateArcVerts(float endTime, bool checkCollision) {
        Vector3[] verts = new Vector3[segments + 1];
        for (int i = 0; i <= segments; i++) {
            float t = Mathf.Lerp(0f, endTime, (float) i / segments);
            float z = startVelX * t;
            float y = PhysicsUtil.GetArcHeightAtTime(startVelY, gravity, t);
            verts[i] = new Vector3(0f, y, z);
            if (i != 0 && checkCollision) {
                RaycastHit hit;
                Vector3 start = GetInWorldSpace(verts[i - 1]);
                Vector3 end = GetInWorldSpace(verts[i]);
                int mask = LayerMask.GetMask("Default");
                if (Physics.Raycast(start, (end - start).normalized, out hit, Vector3.Distance(start, end), mask)) {
                    return CalculateArcVertsBlocked(hit.point);
                }
            }
        }
        return verts;
    }

    private Vector3[] CalculateArcVertsBlocked(Vector3 blockedPoint) {
        float[] timesAtHeight = PhysicsUtil.GetBothArcTimesAtHeight(transform.position.y, startVelY, gravity, blockedPoint.y);
        if (float.IsNaN(timesAtHeight[0]) || float.IsNaN(timesAtHeight[1])) {
            return new Vector3[0];
        }
        Vector3 point1 = GetInWorldSpace(new Vector3(0f, PhysicsUtil.GetArcHeightAtTime(startVelY, gravity, timesAtHeight[0]), 
            PhysicsUtil.GetArcXAtTime(startVelX, timesAtHeight[0])));
        Vector3 point2 = GetInWorldSpace(new Vector3(0f, PhysicsUtil.GetArcHeightAtTime(startVelY, gravity, timesAtHeight[1]), 
            PhysicsUtil.GetArcXAtTime(startVelX, timesAtHeight[1])));
        float newEndTime = Vector3.Distance(point1, blockedPoint) < Vector3.Distance(point2, blockedPoint) ? timesAtHeight[0] : timesAtHeight[1];
        return CalculateArcVerts(newEndTime, false);
    }

    private Vector3 GetInWorldSpace(Vector3 pos) {
        return transform.position + transform.rotation * pos;
    }


}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {
    public float turnSpeed;
    public float moveSpeed;

    // Update is called once per frame
    void Update() {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Jump"), Input.GetAxis("Vertical"));
        if (movement.magnitude == 0f) {
            return;
        }
        if (movement.magnitude > 1f) {
            movement.Normalize();
        }
        Vector3 desiredForward = movement;
        transform.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(transform.forward, Vector3.up),
            Quaternion.LookRotation(desiredForward, Vector3.up), turnSpeed * Time.deltaTime);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Movement : MonoBehaviour {
    public CharacterController characterController;
    public TadpoleLiquidIntersection liquidIntersection;

    public float turnSpeed;
    public float maxMoveSpeed;
    public float minMoveSpeed;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;

    void Start() {
        characterController = GetComponent<CharacterController>();
    }

    void Update() {
        bool isInLiquid = liquidIntersection.TestLiquidCollision();
        if (characterController.isGrounded) {
            // We are grounded, so recalculate
            // move direction directly from axes

            moveDirection = new Vector3(Input.GetAxis("Vertical"), 0.0f, -Input.GetAxis("Horizontal"));
            if (moveDirection.magnitude > 0f) {
                moveDirection.Normalize();
            }
            moveDirection *= liquidIntersection.isInLiquid ? maxMoveSpeed : minMoveSpeed;

            if (Input.GetButton("Jump") && isInLiquid) {
                moveDirection.y = jumpSpeed;
            }
        } 

        if (moveDirection.magnitude > 0.9f) {
            transform.rotation = Quaternion.RotateTowards(Quaternion.LookRotation(transform.forward, Vector3.up),
                Quaternion.LookRotation(moveDirection, Vector3.up), turnSpeed * Time.deltaTime);
        }

        // Apply gravity. Gravity is multiplied by deltaTime twice (once here, and once below
        // when the moveDirection is multiplied by deltaTime). This is because gravity should be applied
        // as an acceleration (ms^-2)
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    #region Fields
    [Header("Player Motor Settings:")]
    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    private float cameraRotationLimit = 85f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private float cameraRotationX = 0f;
    private float currentCameraRotationX = 0f;
    private Vector3 thrusterForce = Vector3.zero;

    private Rigidbody playerRigidBody;
    #endregion

    #region MonoBehaviour
    // Use this for initialization
    void Start()
    {
        this.playerRigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Updates each Physics Iteration
    void FixedUpdate()
    {
        this.PerformMovement();
        this.PerformRotation();
    }
    #endregion

    #region Custom_Methods
    /// <summary>
    /// Gets player velocity vector
    /// </summary>
    /// <param name="velocity"></param>
    public void Move(Vector3 velocity)
    {
        this.velocity = velocity;
    }

    /// <summary>
    /// Gets player rotation vector
    /// </summary>
    /// <param name="rotation"></param>
    public void Rotate(Vector3 rotation)
    {
        this.rotation = rotation;
    }

    /// <summary>
    /// Gets camera rotation vector
    /// </summary>
    /// <param name="cameraRotationX"></param>
    public void RotateCamera(float cameraRotationX)
    {
        this.cameraRotationX = cameraRotationX;
    }

    /// <summary>
    /// Gets thruster force
    /// </summary>
    /// <param name="thrusterForce"></param>
    public void ApplyThrusterForce(Vector3 thrusterForce)
    {
        this.thrusterForce = thrusterForce;
    }

    /// <summary>
    /// Apply player movement
    /// </summary>
    private void PerformMovement()
    {
        if (this.velocity != Vector3.zero)
        {
            this.playerRigidBody.MovePosition(this.playerRigidBody.position + velocity * Time.fixedDeltaTime);
        }

        if (this.thrusterForce != Vector3.zero)
        {
            this.playerRigidBody.AddForce(this.thrusterForce * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// Apply player and fps camera rotation
    /// </summary>
    private void PerformRotation()
    {
        this.playerRigidBody.MoveRotation(this.playerRigidBody.rotation * Quaternion.Euler(this.rotation));

        if (this.playerCamera != null)
        {
            //playerCam.transform.Rotate(-1f * this.cameraRotationX); // old rotation

            // Sets rotation and clamps it
            this.currentCameraRotationX -= this.cameraRotationX;
            this.currentCameraRotationX = Mathf.Clamp(this.currentCameraRotationX, -this.cameraRotationLimit, cameraRotationLimit);

            // Apply rotation to the transform of the player camera
            this.playerCamera.transform.localEulerAngles = new Vector3(this.currentCameraRotationX, 0f, 0f);
        }
    }
    #endregion
}

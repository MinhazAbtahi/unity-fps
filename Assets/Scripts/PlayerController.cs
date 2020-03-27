using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConfigurableJoint))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    #region Fields
    [Header("Player Controller Settings:")]
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float mouseSensitivity = 5f;
    [SerializeField]

    [Header("Thruster Settings:")]
    private float thrusterForce = 1300f;
    [SerializeField]
    private float thrusterFuelBurnSpeed = 1.0f;
    [SerializeField]
    private float thrusterFuelRegenerationSpeed = 0.3f;
    [SerializeField]
    private float thrusterFuelAmount = 1f;

    [Header("Layer Mask Settings: ")]
    [SerializeField]
    private LayerMask environmentLayerMask;

    [Header("Configurable Spring Joint Settings:")]
    [SerializeField]
    private float jointPositionSpring = 20f;
    [SerializeField]
    private float jointMaxForce = 40f;

    // Cached Components
    private Animator animator;
    private PlayerMotor playerMotor;
    private ConfigurableJoint configJoint;
    #endregion

    #region MonoBehaviour
    // Use this for initialization
    void Start()
    {
        this.playerMotor = GetComponent<PlayerMotor>();
        this.configJoint = GetComponent<ConfigurableJoint>();
        this.animator = GetComponent<Animator>();

        this.SetConfigJointSettings(this.jointPositionSpring);
    }

    // Update is called once per frame
    void Update()
    {
        // Freeze Movement
        if (PauseMenu.isOn)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
            }

            this.FreezeMovement();

            return;
        }

        // Cursor Lock
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Calculate movement Velocity as a 3D Vector
        float xMovement = Input.GetAxis("Horizontal");
        float zMovement = Input.GetAxis("Vertical");

        Vector3 horizontalMovement = transform.right * xMovement;
        Vector3 verticalMovement = transform.forward * zMovement;

        // Animate player thrusters
        this.animator.SetFloat("MovementVelocity", zMovement);

        // Final veclicty Vector [previously normalized movements]
        Vector3 velocity = (horizontalMovement + verticalMovement) * speed;

        // Apply Movement
        this.playerMotor.Move(velocity);

        // Calculate rotation as 3D Vector (turning left & right)
        float yRotation = Input.GetAxisRaw("Mouse X");

        Vector3 rotation = new Vector3(0f, yRotation, 0f) * this.mouseSensitivity;

        //Apply Rotation
        this.playerMotor.Rotate(rotation);

        // Calculate camera rotation as 3D Vector (turning up & down)
        float xRotation = Input.GetAxisRaw("Mouse Y");

        float cameraRotationX = xRotation * this.mouseSensitivity;

        // Apply Camera Rotation
        this.playerMotor.RotateCamera(cameraRotationX);

        // Detetcs surface and apply physical positioning according to correct target spring
        RaycastHit hitInfo;
        Vector3 origin = this.transform.position;
        Vector3 direction = Vector3.down;
        float maxDistance = 100f;
        if (Physics.Raycast(origin, direction, out hitInfo, maxDistance, this.environmentLayerMask))
        {
            this.configJoint.targetPosition = new Vector3(0f, -hitInfo.point.y, 0f);
        }
        else
        {
            this.configJoint.targetPosition = new Vector3(0f, 0f, 0f);
        }

        // Calculate thruster force
        Vector3 _thrusterForce = Vector3.zero;
        if (Input.GetButton("Jump") && this.thrusterFuelAmount > 0)
        {
            this.thrusterFuelAmount -= this.thrusterFuelBurnSpeed * Time.deltaTime;

            if (this.thrusterFuelAmount >= 0.01f)
            {
                _thrusterForce = Vector3.up * this.thrusterForce;

                this.SetConfigJointSettings(0f);
            }
        }
        else
        {
            this.thrusterFuelAmount += this.thrusterFuelRegenerationSpeed * Time.deltaTime;

            this.SetConfigJointSettings(this.jointPositionSpring);
        }

        // Clamps the thruster fuel amount
        this.thrusterFuelAmount = Mathf.Clamp(this.thrusterFuelAmount, 0f, 1f);

        // Apply Thruster Force
        this.playerMotor.ApplyThrusterForce(_thrusterForce);
    }
    #endregion

    #region Custom_Methods
    /// <summary>
    /// Set user defined Configurable joint settings
    /// </summary>
    /// <param name="_positionSpring"></param>
    private void SetConfigJointSettings(float _positionSpring)
    {
        this.configJoint.yDrive = new JointDrive
        {
            positionSpring = _positionSpring,
            maximumForce = jointMaxForce
        };
    }

    /// <summary>
    /// Freezes player movement
    /// </summary>
    private void FreezeMovement()
    {
        Vector3 zeroVelocity = Vector3.zero;

        this.playerMotor.Move(zeroVelocity);
        this.playerMotor.Rotate(zeroVelocity);
        this.playerMotor.RotateCamera(0.0f);
    }
    
    /// <summary>
    /// Returns thrusters fuel amount
    /// </summary>
    /// <returns></returns>
    public float GetThrusterFuelAmount()
    {
        return this.thrusterFuelAmount;
    } 
    #endregion
}

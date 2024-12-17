using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Rewired;

public class MovementController : MonoBehaviour
{
    // Get Rewired setup
    public int playerId;
    private Player player;

    // Hero Stats
    public float movSpeed;

    // Player parts
    public Transform meshToRotate;
    private Collider playerCollider;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator animator;

    [HideInInspector]
    private MovementControllerStatus movementStatus;
    public MovementControllerStatus MovementStatus
    {
        get { return movementStatus; }
        set { movementStatus = value; }
    }

    [HideInInspector]
    private RotationControllerStatus rotationStatus;
    public RotationControllerStatus RotationStatus
    {
        get { return rotationStatus; }
        set { rotationStatus = value; }
    }

    private Vector3 movDir;
    private float angleL;

    void Start()
    {
        playerCollider = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    void Awake()
    {
        // Get the Player for a particular playerId
        player = ReInput.players.GetPlayer(playerId);
    }

    void Update()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        HandleInput();
        HandleMovement();
    }

    void HandleInput()
    {
        float dirXL = player.GetAxis("LHorizontal");
        float dirZL = player.GetAxis("LVertical");

        // Calculate movement direction and angle
        movDir = new Vector3(dirXL, 0, dirZL).normalized;
        angleL = Mathf.Atan2(dirXL, dirZL) * Mathf.Rad2Deg;
    }

    void HandleMovement()
    {
        if (movementStatus == MovementControllerStatus.FREEMOVEMENT)
        {
            if (movDir != Vector3.zero)
            {
                HandleRotation();

                // Update position based on movement direction
                rb.linearVelocity = transform.TransformDirection(movDir * movSpeed);

                // Update animations
                animator.SetFloat("VelX", 0);
                animator.SetFloat("VelY", 1);
            }
            else
            {
                rb.linearVelocity = Vector3.zero; // Stop immediately
                animator.SetFloat("VelX", 0);
                animator.SetFloat("VelY", 0);
            }
        }
    }

    void HandleRotation()
    {
        if (rotationStatus == RotationControllerStatus.FREEROTATION)
        {
            meshToRotate.localRotation = Quaternion.AngleAxis(angleL, Vector3.up);
        }
        else if (rotationStatus == RotationControllerStatus.FREEZEROTATION)
        {
            // Maintain current rotation
            meshToRotate.localRotation = meshToRotate.localRotation;
        }
    }
}

// Movement Statuses
public enum MovementControllerStatus
{
    FREEMOVEMENT,
    FREEZEMOVEMENT,
}

public enum RotationControllerStatus
{
    FREEROTATION,
    FREEZEROTATION
}

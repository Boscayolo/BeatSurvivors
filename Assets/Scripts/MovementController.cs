using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using Rewired;

public class MovementController : MonoBehaviour
{

    //Get Rewired setup
    public int playerId;
    private Player player;

    //Hero Stats
    public float movSpeed;

    //Player parts
    public Transform meshToRotate;
    private Collider playerCollider;
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public Animator animator;

    [HideInInspector]
    private MovementControllerStatus movementStatus;


    public MovementControllerStatus MovementStatus
    {
        get
        {
            return movementStatus;
        }
        set
        {
            movementStatus = value;
        }
    }

    [HideInInspector]
    private RotationControllerStatus rotationStatus;
    public RotationControllerStatus RotationStatus
    {
        get
        {
            return rotationStatus;
        }
        set
        {
            rotationStatus = value;
        }
    }


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


    private void FixedUpdate()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null)
        {
            Move();
        }


    }

    void Move()
    {
        float dirXR = player.GetAxis("RHorizontal");
        float dirZR = player.GetAxis("RVertical");

        float angleR = Mathf.Atan2(dirXR, -dirZR) * Mathf.Rad2Deg;

        float dirXL = player.GetAxis("LHorizontal");
        float dirZL = player.GetAxis("LVertical");

        float angleL = Mathf.Atan2(dirXL, dirZL) * Mathf.Rad2Deg;

        //calculate movment direction
        Vector3 movDir = new Vector3(dirXL, 0, dirZL).normalized;

        //Movements and animations
        if (movementStatus == MovementControllerStatus.FREEMOVEMENT)
        {
            if (dirXL != 0f || dirZL != 0f)
            {
                if (rotationStatus == RotationControllerStatus.FREEROTATION)
                {
                    meshToRotate.transform.localRotation = Quaternion.AngleAxis(angleL, Vector3.up);
                }
                else if (rotationStatus == RotationControllerStatus.FREEZEROTATION)
                {
                    meshToRotate.transform.localRotation = meshToRotate.transform.localRotation;
                }

                rb.MovePosition(rb.position + transform.TransformDirection(movDir * Time.deltaTime * movSpeed));
                animator.SetFloat("VelX", 0);
                animator.SetFloat("VelY", 1);
            }

            if (dirXL == 0 && dirZL == 0)
            {
                animator.SetFloat("VelX", 0);
                animator.SetFloat("VelY", 0);
            }
        }

        //Rotation, out of the main block cause rotation should happen even when casting or root or genrally, FREEZEMOVEMENT should let rotation.
        if (dirXR != 0f || dirZR != 0f)
        {
            if (rotationStatus == RotationControllerStatus.FREEROTATION)
            {
                meshToRotate.transform.localRotation = Quaternion.AngleAxis(angleR, Vector3.up);
            }
            else if (rotationStatus == RotationControllerStatus.FREEZEROTATION)
            {
                meshToRotate.transform.localRotation = meshToRotate.transform.localRotation;
            }

            animator.SetFloat("VelX", -dirXL * dirXR);
            animator.SetFloat("VelY", dirZL * -dirZR);
        }

    }

}

//Movement Statuses
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

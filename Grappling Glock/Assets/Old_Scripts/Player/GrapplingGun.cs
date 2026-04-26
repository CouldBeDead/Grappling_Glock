using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingGun : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private LineRenderer lr;
    private Vector3 grapplePoint;

    public LayerMask whatIsGrappleable;
    public LayerMask whatIsRed;
    public Transform gunTip, playerCamera, player;

    [Header("Grapple Settings")]
    [SerializeField] private float maxDistance = 100f;

    [Header("Aim Assist Settings")]
    [SerializeField] private float aimAssistRadius = 1.5f;
    [SerializeField] private float aimAssistAngle = 8f;

    private SpringJoint joint;

    private bool isGrappling = false;
    private Vector3 currentGrapplePosition;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    void Start()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            StartGrapple();
        }
        else if (mouse.leftButton.wasReleasedThisFrame)
        {
            StopGrapple();
        }
    }

    void LateUpdate()
    {
        if (isGrappling)
        {
            UpdateGrapple();
            CheckGrappleEnd();
        }
    }

    bool TryDirectHit(LayerMask layerMask, out RaycastHit hit)
    {
        return Physics.Raycast(
            playerCamera.position,
            playerCamera.forward,
            out hit,
            maxDistance,
            layerMask
        );
    }

    bool TryAimAssistHit(LayerMask layerMask, out RaycastHit hit)
    {
        if (Physics.SphereCast(
            playerCamera.position,
            aimAssistRadius,
            playerCamera.forward,
            out hit,
            maxDistance,
            layerMask
        ))
        {
            Vector3 directionToTarget = hit.collider.bounds.center - playerCamera.position;
            float angleToTarget = Vector3.Angle(playerCamera.forward, directionToTarget);

            if (angleToTarget <= aimAssistAngle)
            {
                return true;
            }
        }

        return false;
    }
    public bool TryGetAvailableGrapplePoint(out Vector3 point)
{
    RaycastHit hit;

    // Direct enemy hit
    if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxDistance, whatIsRed))
    {
        point = hit.point;
        return true;
    }

    // Direct grappleable hit
    if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxDistance, whatIsGrappleable))
    {
        point = hit.point;
        return true;
    }

    // Aim assist enemy hit
    if (Physics.SphereCast(playerCamera.position, aimAssistRadius, playerCamera.forward, out hit, maxDistance, whatIsRed))
    {
        Vector3 directionToTarget = hit.collider.bounds.center - playerCamera.position;
        float angleToTarget = Vector3.Angle(playerCamera.forward, directionToTarget);

        if (angleToTarget <= aimAssistAngle)
        {
            point = hit.point;
            return true;
        }
    }

    // Aim assist grappleable hit
    if (Physics.SphereCast(playerCamera.position, aimAssistRadius, playerCamera.forward, out hit, maxDistance, whatIsGrappleable))
    {
        Vector3 directionToTarget = hit.collider.bounds.center - playerCamera.position;
        float angleToTarget = Vector3.Angle(playerCamera.forward, directionToTarget);

        if (angleToTarget <= aimAssistAngle)
        {
            point = hit.point;
            return true;
        }
    }

    point = Vector3.zero;
    return false;
}

    void StartGrapple()
    {
        RaycastHit hit;

        // Exact enemy hit gets priority
        if (TryDirectHit(whatIsRed, out hit))
        {
            EnemyGrapple(hit);
            return;
        }

        // Exact ground/grappleable hit
        if (TryDirectHit(whatIsGrappleable, out hit))
        {
            GroundGrapple(hit);
            return;
        }

        // Aim assist enemy hit
        if (TryAimAssistHit(whatIsRed, out hit))
        {
            EnemyGrapple(hit);
            return;
        }

        // Aim assist grappleable hit
        if (TryAimAssistHit(whatIsGrappleable, out hit))
        {
            GroundGrapple(hit);
            return;
        }
    }

    void EnemyGrapple(RaycastHit hit)
    {
        if (playerMovement != null && playerMovement.grounded)
        {
            isGrappling = false;
            return;
        }

        grapplePoint = hit.point;
        isGrappling = true;

        SpringJoint existingJoint = player.GetComponent<SpringJoint>();
        if (existingJoint != null)
        {
            Destroy(existingJoint);
        }

        Vector3 direction = (grapplePoint - player.position).normalized;
        float grappleSpeed = 75f;

        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = direction * grappleSpeed;
        }

        lr.positionCount = 2;
        currentGrapplePosition = gunTip.position;
    }

    void GroundGrapple(RaycastHit hit)
    {
        grapplePoint = hit.point;
        isGrappling = true;

        joint = player.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = grapplePoint;

        float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

        joint.maxDistance = distanceFromPoint * 0.3f;
        joint.minDistance = distanceFromPoint * 0.1f;

        joint.spring = 4f;
        joint.damper = 4.5f;
        joint.massScale = 4.5f;

        lr.positionCount = 2;
        currentGrapplePosition = gunTip.position;
    }

    void UpdateGrapple()
    {
        if (!isGrappling)
            return;

        if (lr.positionCount > 0)
        {
            currentGrapplePosition = Vector3.Lerp(
                currentGrapplePosition,
                grapplePoint,
                Time.deltaTime * 8f
            );

            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, currentGrapplePosition);
        }

        if (Vector3.Distance(player.position, grapplePoint) < 2f)
        {
            StopGrapple();
        }
    }

    void CheckGrappleEnd()
    {
        float distance = Vector3.Distance(player.position, grapplePoint);

        if (distance < 2f)
        {
            Collider[] hits = Physics.OverlapSphere(grapplePoint, 1f, whatIsRed);

            foreach (Collider hit in hits)
            {
                EnemyVisionAndTracking enemy = hit.GetComponentInParent<EnemyVisionAndTracking>();

                if (enemy != null)
                {
                    enemy.Die(transform.position);
                }
            }

            LaunchPlayer();
        }
    }

    void LaunchPlayer()
    {
        lr.positionCount = 0;

        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 launchForce = new Vector3(5f, 20f, 0f);
            rb.linearVelocity = launchForce;
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        lr.positionCount = 0;

        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }
    }

    void DrawRope()
    {
        if (joint == null)
            return;

        currentGrapplePosition = Vector3.Lerp(
            currentGrapplePosition,
            grapplePoint,
            Time.deltaTime * 2f
        );

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }

    public bool IsGrappling()
    {
        return joint != null;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}
using UnityEngine;

public class GrapplingGun : MonoBehaviour {
    private PlayerMovement playerMovement;
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public LayerMask whatIsRed;
    public Transform gunTip, playerCamera, player;
    private float maxDistance = 100f;
    private SpringJoint joint;

    private bool isGrappling = false;
    private Vector3 currentGrapplePosition;

    void Start() {
        playerMovement = FindObjectOfType<PlayerMovement>();
    }

    void Awake() {
        lr = GetComponent<LineRenderer>();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            StartGrapple();
        } else if (Input.GetMouseButtonUp(0)) {
            StopGrapple();
        }
    }

    void LateUpdate() {
        if (isGrappling) {
            UpdateGrapple();
            CheckGrappleEnd();
        }
    }

    void enemyGrapple() {
        if (playerMovement != null && playerMovement.grounded) {
            isGrappling = false;
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxDistance, whatIsRed)) {
            grapplePoint = hit.point;
            isGrappling = true;

            if (player.GetComponent<SpringJoint>()) {
                Destroy(player.GetComponent<SpringJoint>());
            }

            Vector3 direction = (grapplePoint - player.position).normalized;
            float grappleSpeed = 75f;
            Rigidbody rb = player.GetComponent<Rigidbody>();

            if (rb != null) {
                rb.linearVelocity = direction * grappleSpeed;
            }

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
    }

    void groundGrapple() {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxDistance, whatIsGrappleable)) {
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
    }

    void StartGrapple() {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxDistance, whatIsRed)) {
            enemyGrapple();
        } else if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, maxDistance, whatIsGrappleable)) {
            groundGrapple();
        }
    }

    void UpdateGrapple() {
        if (!isGrappling) return;

        if (lr.positionCount > 0) {
            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);
            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, currentGrapplePosition);
        }

        if (Vector3.Distance(player.position, grapplePoint) < 2f) {
            StopGrapple();
        }
    }

    void CheckGrappleEnd() {
        float distance = Vector3.Distance(player.position, grapplePoint);

        if (distance < 2f) {
            Collider[] hits = Physics.OverlapSphere(grapplePoint, 1f, whatIsRed);
            foreach (Collider hit in hits) {
                EnemyVisionAndTracking enemy = hit.GetComponentInParent<EnemyVisionAndTracking>();
                if (enemy != null) {
                    enemy.Die(transform.position);
                }
            }

            LaunchPlayer();
        }
    }

    void LaunchPlayer() {
        lr.positionCount = 0;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) {
            Vector3 launchForce = new Vector3(5f, 20f, 0);
            rb.linearVelocity = launchForce;
        }
    }

    void StopGrapple() {
        isGrappling = false;
        lr.positionCount = 0;

        if (joint != null) {
            Destroy(joint);
        }
    }

    void DrawRope() {
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 2f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }

    public bool IsGrappling() {
        return joint != null;
    }

    public Vector3 GetGrapplePoint() {
        return grapplePoint;
    }
}

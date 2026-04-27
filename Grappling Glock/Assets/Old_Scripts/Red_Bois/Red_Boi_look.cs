using UnityEngine;

public class EnemyVisionAndTracking : MonoBehaviour
{
    [Header("References")]
    public Transform gun;
    public GameObject gun_obj;
    public Transform player;
    public LayerMask obstructionMask;

    [Header("Vision Settings")]
    public float visionRange = 20f;
    public float fieldOfView = 60f;
    public int visionRayCount = 5;

    [Header("Aiming Behavior")]
    public float aimDistance = 6f;

    [Header("Rotation Speeds")]
    public float bodyRotationSpeed = 6f;
    public float gunRotationSpeed = 10f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float stopDistance = 5f;

    [Header("Death Ragdoll")]
    public GameObject ragdollPrefab;       // Assign your ragdoll prefab in Inspector
    public Transform hitPointReference;    // Where the enemy got hit (used for direction)


    private Rigidbody rb;
    private bool isDead = false;
    private bool playerDetected = false;
    private bool isSearching = false;

    private Vector3 lastSeenPosition;

    private float searchDuration = 3f;
    private float searchTimer = 0f;
    private Quaternion searchStartRotation;
    private float searchAngle = 45f;
    private float searchRotationSpeed = 2f;
    private float searchRotationTime = 0f;

    private Quaternion initialRotation;
    private float lookAroundTimer = 0f;
    private float lookAroundAngle = 45f;

    [Header("Shooting stuffs")]
    public Transform barrel;
    public GameObject bulletPrefab;
    public float shootCooldown = 3f;
    public float burstInterval = 0.1f;

private float shootTimer = 0f;

    void Start()
    {
        initialRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLOS = HasLineOfSight();

        if (hasLOS)
        {
            playerDetected = true;
            isSearching = false;

            lastSeenPosition = player.position;

            RotateBodyTowardsPlayer();

            if (distanceToPlayer <= aimDistance)
                RotateGunTowardsPlayer();

            if (distanceToPlayer > stopDistance)
                MoveTowardsPlayer();
        }
        else if (playerDetected)
        {
            isSearching = true;
            playerDetected = false;
            searchTimer = searchDuration;
            searchStartRotation = transform.rotation;
            searchRotationTime = 0f;
        }

        if (isSearching)
        {
            float distanceToLastSeen = Vector3.Distance(transform.position, lastSeenPosition);

            if (distanceToLastSeen > stopDistance)
            {
                MoveTowardsPosition(lastSeenPosition);
            }
            else
            {
                searchRotationTime += Time.deltaTime * searchRotationSpeed;
                float angle = Mathf.Sin(searchRotationTime) * searchAngle;
                Quaternion targetRot = Quaternion.Euler(0, angle, 0) * searchStartRotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * bodyRotationSpeed);

                ScanForPlayer();
            }

            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0f)
            {
                isSearching = false;
            }
        }

        if (!playerDetected && !isSearching)
        {
            LookAround();
            ScanForPlayer();
        }
    }

    private void LookAround()
    {
        lookAroundTimer += Time.deltaTime;
        float angle = Mathf.Sin(lookAroundTimer) * lookAroundAngle;
        Quaternion targetRotation = Quaternion.Euler(0, angle, 0) * initialRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * bodyRotationSpeed);
    }

    private void ScanForPlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        int verticalRayCount = 3;
        int horizontalRayCount = visionRayCount;
        float verticalFOV = fieldOfView * 0.5f;

        for (int y = 0; y < verticalRayCount; y++)
        {
            float verticalOffset = Mathf.Lerp(-verticalFOV / 2f, verticalFOV / 2f, (float)y / (verticalRayCount - 1f));
            for (int x = 0; x < horizontalRayCount; x++)
            {
                float horizontalOffset = Mathf.Lerp(-fieldOfView / 2f, fieldOfView / 2f, (float)x / (horizontalRayCount - 1f));
                Quaternion rotation = Quaternion.Euler(verticalOffset, horizontalOffset, 0);
                Vector3 rayDir = rotation * transform.forward;

              if (Physics.Raycast(origin, rayDir, out RaycastHit hit, visionRange, ~0))
                {
                    if (hit.transform == player)
                    {
                        playerDetected = true;
                        return;
                    }
                    else if (((1 << hit.collider.gameObject.layer) & obstructionMask) != 0)
                    {
                        // Vision blocked, don't do anything
                    }
                }

                Debug.DrawRay(origin, rayDir * visionRange, Color.cyan);
            }
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = gun.position + gun.forward * 0.5f;
        Vector3 target = player.position + Vector3.up * 1.0f;
        Vector3 directionToPlayer = (target - origin).normalized;
        float distanceToPlayer = Vector3.Distance(origin, target);

        float sphereRadius = 1.0f;

        if (Physics.SphereCast(origin, sphereRadius, directionToPlayer, out RaycastHit hit, distanceToPlayer, ~0))
        {
            if (hit.transform == player)
            {
                return true;
            }
            else if (((1 << hit.collider.gameObject.layer) & obstructionMask) != 0)
            {
                return false; // Obstructed by ground/wall/etc.
            }
        }

        return false;
    }


    private void RotateGunTowardsPlayer()
    {
        Vector3 direction = player.position - gun.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        gun.rotation = Quaternion.Slerp(gun.rotation, targetRotation, Time.deltaTime * gunRotationSpeed);
    }

    private void RotateBodyTowardsPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * bodyRotationSpeed);
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;
        toPlayer.y = 0;

        Quaternion lookRot = Quaternion.LookRotation(toPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * bodyRotationSpeed);

        Vector3 forward = transform.forward;
        rb.MovePosition(rb.position + forward * moveSpeed * Time.deltaTime);
    }

    private void MoveTowardsPosition(Vector3 targetPosition)
    {
        Vector3 toTarget = (targetPosition - transform.position).normalized;
        toTarget.y = 0;

        Quaternion lookRot = Quaternion.LookRotation(toTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * bodyRotationSpeed);

        Vector3 forward = transform.forward;
        rb.MovePosition(rb.position + forward * moveSpeed * Time.deltaTime);
    }

    public void Die(Vector3 hitOrigin)
{
    if (isDead) return;
    isDead = true;

    // Instantiate ragdoll prefab
    GameObject ragdoll = Instantiate(ragdollPrefab, transform.position, transform.rotation);

    // Calculate force direction
    Vector3 forceDir = (transform.position - hitOrigin).normalized + Vector3.up * 0.5f;
    float forceStrength = 15f;

    // Apply force to all rigidbodies in the ragdoll
    foreach (Rigidbody rb in ragdoll.GetComponentsInChildren<Rigidbody>())
    {
        rb.AddForce(forceDir * forceStrength, ForceMode.Impulse);
    }

    // Destroy the original enemy
    Destroy(gameObject);
}
// Umm shooting stuffs



private void TryShoot()
{
    if (shootTimer <= 0f)
        StartCoroutine(ShootBurst());
}

private System.Collections.IEnumerator ShootBurst()
{
    shootTimer = shootCooldown;

    for (int i = 0; i < 1; i++)
    {
        Instantiate(bulletPrefab, barrel.position, barrel.rotation);
        yield return new WaitForSeconds(burstInterval);
    }
}

void LateUpdate()
{
    if (isDead) return;

    if (playerDetected && Vector3.Distance(transform.position, player.position) <= aimDistance && HasLineOfSight())
    {
        RotateGunTowardsPlayer();
        TryShoot();
    }

    shootTimer -= Time.deltaTime;
}

}

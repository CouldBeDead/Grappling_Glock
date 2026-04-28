using UnityEngine;

public class EnemyGunTracking : MonoBehaviour
{
    public Transform gun;                 // The part that rotates
    public Transform player;              // Reference to the player
    public LayerMask obstructionMask;     // LayerMask for what counts as an obstruction, like walls

    public float rotationSpeed = 5f;      // How fast the gun rotates

    void Update()
    {
        if (HasLineOfSight())
        {
            RotateGunTowardsPlayer();
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 directionToPlayer = player.position - gun.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (Physics.Raycast(gun.position, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer, ~0))
        {
            if (hit.transform == player)
                return true;
        }

        return false;
    }

    private void RotateGunTowardsPlayer()
    {
        Vector3 direction = player.position - gun.position;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Offset the gun rotation by -90 degrees on the Y axis
        targetRotation *= Quaternion.Euler(-90f, 0f, 0f);

        gun.rotation = Quaternion.Slerp(
            gun.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}
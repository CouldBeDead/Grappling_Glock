using UnityEngine;

public class EnemyGunTracking : MonoBehaviour
{
    public Transform gun;                 // The part that rotates
    public Transform player;              // Reference to the player
    public LayerMask obstructionMask;     // LayerMask for what counts as an obstruction (e.g., walls)

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

        // Cast a ray from the gun to the player
        if (Physics.Raycast(gun.position, directionToPlayer.normalized, out RaycastHit hit, distanceToPlayer, ~0))
        {
            // If the hit object is the player, there's LOS
            if (hit.transform == player)
                return true;
        }

        return false;
    }

    private void RotateGunTowardsPlayer()
    {
        Vector3 direction = player.position - gun.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        gun.rotation = Quaternion.Slerp(gun.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}

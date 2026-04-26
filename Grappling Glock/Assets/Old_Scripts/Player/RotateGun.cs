using UnityEngine;

public class RotateGun : MonoBehaviour
{
    public GrapplingGun grappling;

    private Quaternion desiredRotation;
    [SerializeField] private float rotationSpeed = 10f;

    void Update()
    {
        if (grappling == null)
            return;

        Vector3 targetPoint;

        if (grappling.IsGrappling())
        {
            targetPoint = grappling.GetGrapplePoint();
            desiredRotation = Quaternion.LookRotation(targetPoint - transform.position);
        }
        else if (grappling.TryGetAvailableGrapplePoint(out targetPoint))
        {
            desiredRotation = Quaternion.LookRotation(targetPoint - transform.position);
        }
        else
        {
            desiredRotation = transform.parent.rotation;
        }

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            desiredRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}
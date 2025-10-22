using System.Collections.Generic;
using UnityEngine;

public class LevelPart : MonoBehaviour
{
    [SerializeField] private LayerMask intersectionLayer;
    public Transform intersectionCheckParent;
    [SerializeField] private Collider[] intersectionColliders;

    private void Awake()
    {
        intersectionColliders = intersectionCheckParent.GetComponentsInChildren<Collider>();
    }

    [ContextMenu("Set Static To Environment Layer")]
    public void SetStaticToEnvironmentLayer()
    {
        Transform[] allChildren = transform.GetComponentsInChildren<Transform>(true);

        foreach(Transform child in allChildren)
        {
            if(child.gameObject.isStatic)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Environment");
            }
        }
    }


    public bool IntersectionDetected()
    {

        Physics.SyncTransforms();

        foreach (Collider ownCollider in intersectionColliders)
        {
            Collider[] detectedColliders = Physics.OverlapBox(
                ownCollider.bounds.center,
                ownCollider.bounds.extents,
                Quaternion.identity,
                intersectionLayer);

            foreach (Collider collider in detectedColliders)
            {
                LevelPart intersection = collider.GetComponentInParent<LevelPart>();

                if (intersection != null && intersection.intersectionCheckParent != intersectionCheckParent)
                    return true;
            }

        }

        return false;
    }


    public void SnapAndAlignLevelPart(SnapPoint targetSnapPoint)
    {
        SnapPoint entrancePoint = GetEnterSnapPoint();

        AlignTo(entrancePoint, targetSnapPoint); // Always before snapping
        SnapTo(entrancePoint, targetSnapPoint);
    }

    private void SnapTo(SnapPoint ownSnapPoint, SnapPoint targetSnapPoint)
    {
        // Calculate and apply position offset to match snap points
        var offset = transform.position - ownSnapPoint.transform.position;

        var newPosition = targetSnapPoint.transform.position + offset;

        transform.position = newPosition;


    }

    private void AlignTo(SnapPoint ownSnapPoint, SnapPoint targetSnapPoint)
    {
        // Calculate rotation difference and apply necessary rotations
        var rotationOffset = ownSnapPoint.transform.rotation.eulerAngles.y - transform.rotation.eulerAngles.y;
        transform.rotation = targetSnapPoint.transform.rotation;
        transform.Rotate(0, 180, 0); // Flip to face correct direction
        transform.Rotate(0, -rotationOffset, 0); // Apply offset correction

    }

    // Convenience methods to get specific snap points
    public SnapPoint GetExitSnapPoint() => GetSnapPointOfType(SnapPointType.Exit);
    public SnapPoint GetEnterSnapPoint() => GetSnapPointOfType(SnapPointType.Enter);

    private SnapPoint GetSnapPointOfType(SnapPointType type)
    {
        SnapPoint[] snapPoints = GetComponentsInChildren<SnapPoint>();
        List<SnapPoint> filteredSnapPoints = new();

        // Filter snap points by type
        foreach (SnapPoint snapPoint in snapPoints)
        {
            if (snapPoint.SnapPointType == type)
                filteredSnapPoints.Add(snapPoint);
        }

        // Return a random snap point of the requested type
        if (filteredSnapPoints.Count > 0)
        {
            int randomIndex = Random.Range(0, filteredSnapPoints.Count);

            return filteredSnapPoints[randomIndex];
        }

        return null;
    }
}

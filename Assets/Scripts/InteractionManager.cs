using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private float doorSearchRadius = 3.0f;

    private Transform cameraTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InteractWithDoor()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, doorSearchRadius);
        DoorController doorToOpen = null;
        foreach (Collider collider in nearbyColliders)
        {
            DoorController door = collider.GetComponent<DoorController>();
            if (door == null)
                continue;
            if (Physics.Linecast(cameraTransform.position, collider.bounds.center, out RaycastHit hit) && hit.transform == door.transform)
            {
                if (doorToOpen == null || (cameraTransform.position - door.transform.position).sqrMagnitude < (cameraTransform.position - doorToOpen.transform.position).sqrMagnitude)
                    doorToOpen = door;
            }
        }

        if (doorToOpen != null)
            doorToOpen.TryOpenDoor();
    }
}

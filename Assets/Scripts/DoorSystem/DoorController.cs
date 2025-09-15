using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    enum DoorState : int
    {
        Closed,
        Opening,
        Open,
        Closing,
    }

    [SerializeField] private float speed = 75.0f;
    [SerializeField] private DoorState state = DoorState.Closed;

    private Coroutine doorOpeningRoutine;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (CanInteract() && Input.GetKeyDown(KeyCode.U))
        {
            if (state == DoorState.Open)
            {
                if (doorOpeningRoutine == null)
                    Debug.LogError("Door was marked as Open, but no door opening coroutine was active.");
                else
                {
                    StopCoroutine(doorOpeningRoutine);
                    doorOpeningRoutine = null;
                }
            }
            doorOpeningRoutine = StartCoroutine(OpenDoorSwing());
        }
    }

    private bool CanInteract()
    {
        return state == DoorState.Closed || state == DoorState.Open;
    }

    IEnumerator OpenDoorSwing()
    {
        float rotation = 0.0f;
        bool clickedToClose = (state == DoorState.Open);
        Vector3 initialRotation;
        Vector3 finalRotation;

        if (!clickedToClose)
        {
            // Standard: the user clicked to open a closed door.
            initialRotation = transform.eulerAngles;
            finalRotation = initialRotation;
            finalRotation.y += 90.0f;
        }
        else
        {
            // Clicked to close. The final rotation is the starting (opened) value, and the initial is 90 degrees less. Door starts at 90 degrees rotation.
            finalRotation = transform.eulerAngles;
            initialRotation = finalRotation;
            initialRotation.y -= 90.0f;
            rotation = 90.0f;
        }

        // Opening animation and hold-open wait time. Only happens on click to open.
        if (!clickedToClose)
        {
            state = DoorState.Opening;
            while (rotation < 90.0f)
            {
                transform.Rotate(0, speed * Time.deltaTime, 0);
                rotation += speed * Time.deltaTime;
                yield return null;
            }
            transform.eulerAngles = finalRotation;

            state = DoorState.Open;
            yield return new WaitForSeconds(5.0f);
        }

        // Closing animation
        state = DoorState.Closing;
        while (rotation > 0.0f)
        {
            transform.Rotate(0, -speed * Time.deltaTime, 0);
            rotation -= speed * Time.deltaTime;
            yield return null;
        }
        transform.eulerAngles = initialRotation;

        state = DoorState.Closed;        
    }
}

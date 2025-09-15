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

    [SerializeField] private DoorState state = DoorState.Closed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
            StartCoroutine(OpenDoorSwing());
    }

    IEnumerator OpenDoorSwing()
    {
        float speed = 75.0f;
        float rotation = 0.0f;
        Vector3 initialRotation = transform.eulerAngles;
        Vector3 finalRotation = initialRotation;
        finalRotation.y += 90.0f;

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

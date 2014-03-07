using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // camera rotation settings
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationY = 0F;

    // character motor settings

    private Transform camera;
    private CharacterMotor motor;

    private void Start()
    {
        if (networkView.isMine)
        {
            GameObject cameraGO = GameObject.FindWithTag("MainCamera");
            camera = cameraGO.transform;
            camera.parent = gameObject.transform;
            camera.localPosition = new Vector3(0, .65f, 0);
            motor = GetComponent<CharacterMotor>();
        }
        else
        {
            GetComponent<CharacterMotor>().canControl = false;
            enabled = false;
        }
    }

    void Update()
    {
        CameraLook();
        FPSInputController();
    }

    /// <summary>
    /// Makes the character controller look in the mouse's direction
    /// </summary>
    private void CameraLook()
    {
        transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
        camera.localEulerAngles = new Vector3(-rotationY, camera.localEulerAngles.y, 0);
    }

    /// <summary>
    /// Directionalizes the character controller correctly
    /// </summary>
    private void FPSInputController()
    {
        var directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (directionVector != Vector3.zero)
        {
            var directionLength = directionVector.magnitude;
            directionVector = directionVector / directionLength;
            directionLength = Mathf.Min(1, directionLength);
            directionLength = directionLength * directionLength;
            directionVector = directionVector * directionLength;
        }
        motor.inputMoveDirection = transform.rotation * directionVector;
        motor.inputJump = Input.GetButton("Jump");
    }
}

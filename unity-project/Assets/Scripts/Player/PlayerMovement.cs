using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // camera rotation settings
    public float sensitivityX = 15f;
    public float sensitivityY = 15f;

    public float minimumX = -360f;
    public float maximumX = 360f;

    public float minimumY = -60f;
    public float maximumY = 60f;

    public float rotationY = 0f;

    // character motor settings
    private Transform _camera;
    private CharacterMotor _motor;
    private CharacterController _controller;

    private PlayerInfo _playerInfo;

    // network movement settings
    private float _lastSynchroTime = 0f;
    private float _syncDelay = 0f;
    private float _syncTime = 0f;
    private Vector3 _syncStartPosition = Vector3.zero;
    private Vector3 _syncEndPosition = Vector3.zero;

    private void Start()
    {
        if (networkView.isMine)
        {
            GameObject cameraGO = GameObject.FindWithTag("MainCamera");
            _camera = cameraGO.transform;
            _camera.parent = gameObject.transform;
            _camera.localPosition = new Vector3(0, .65f, 0);
            _motor = GetComponent<CharacterMotor>();
            _controller = GetComponent<CharacterController>();
            _playerInfo = GetComponent<PlayerInfo>();
        }
        else
        {
            GetComponent<CharacterMotor>().canControl = false;
        }
    }

    private void Update()
    {
        if (networkView.isMine)
        {
            CameraLook();
            FPSInputController();
            CheckFallDeath();
        }
        else
        {
            SyncMovement();
        }
    }

    private void CheckFallDeath()
    {
        if (transform.position.y < -100)
        {
            _playerInfo.Die();
        }
    }

    private void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        Vector3 syncPosition = Vector3.zero;
        Vector3 syncVelocity = Vector3.zero;
        if (stream.isWriting)
        {
            syncPosition = transform.position;
            stream.Serialize(ref syncPosition);

            syncVelocity = _controller.velocity;
            stream.Serialize(ref syncVelocity);
        }
        else
        {
            stream.Serialize(ref syncPosition);
            stream.Serialize(ref syncVelocity);

            _syncTime = 0f;
            _syncDelay = Time.time - _lastSynchroTime;
            _lastSynchroTime = Time.time;

            _syncEndPosition = syncPosition + syncVelocity * _syncDelay;
            _syncStartPosition = transform.position;
        }
    }

    private void SyncMovement()
    {
        _syncTime += Time.deltaTime;
        transform.position = Vector3.Lerp(_syncStartPosition, _syncEndPosition, _syncTime / _syncDelay);
    }

    /// <summary>
    /// Makes the character controller look in the mouse's direction
    /// </summary>
    private void CameraLook()
    {
        transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
        _camera.localEulerAngles = new Vector3(-rotationY, _camera.localEulerAngles.y, 0);
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
        _motor.inputMoveDirection = transform.rotation * directionVector;
        _motor.inputJump = Input.GetButton("Jump");
    }
}

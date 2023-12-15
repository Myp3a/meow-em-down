using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerInput : MonoBehaviour
{
    private CatControls catControls;
    private CharacterController characterController;

    [SerializeField] private Camera camera;
    [SerializeField] private float movementSpeed = 2.0f;
    [SerializeField] private float lookSensitivity = 1.0f;
    [SerializeField] private float jumpPower = 2.0f;
    [SerializeField] private float pawSpeed = 1.0f;
    [SerializeField] private GameObject pawPrefab;
    
    private float xRotation = 0f;
    private float lastMeow;
    private Vector3 velocity;
    public float gravity = -9.81f;
    private bool grounded;
    private bool pawMovingLeft = false;
    private bool pawMovingRight = false;
    private GameObject leftPaw;
    private GameObject rightPaw;

    public void Awake()
    {
        catControls = new CatControls();
    }

    public void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;
        lastMeow = Time.time;
    }

    public void OnEnable()
    {
        catControls.Enable();
    }

    public void OnDisable()
    {
        catControls.Disable();
    }

    public void Update()
    {
        DoLooking();
        DoMovement();
        DoJump();
        DoPaws();
        DoMeow();
    }

    private void DoLooking()
    {
        Vector2 looking = GetPlayerLook();
        float lookX = looking.x * lookSensitivity * Time.deltaTime;
        float lookY = looking.y * lookSensitivity * Time.deltaTime;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 45f);

        camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * lookX);
    }

    private void DoMovement()
    {
        grounded = characterController.isGrounded;
        if (grounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        Vector2 movement = GetPlayerMovement();
        Vector3 move = transform.right * movement.x + transform.forward * movement.y;
        characterController.Move(move * movementSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void DoJump()
    {
        grounded = characterController.isGrounded;
        if (grounded && catControls.Player.Jump.ReadValue<float>() == 1.0f)
        {
            velocity.y = 2f * jumpPower;
        }
    }

    private void DoMeow()
    {
        if (catControls.Player.Meow.ReadValue<float>() == 1.0f && Time.time - lastMeow > 0.1f)
        {
            lastMeow = Time.time;
            this.GetComponent<AudioSource>().PlayOneShot(this.GetComponent<AudioSource>().clip);
        }
    }

    private void DoPaws()
    {
        if (!pawMovingLeft && catControls.Player.LPaw.ReadValue<float>() == 1.0f)
        {
            pawMovingLeft = true;
            leftPaw = Instantiate(pawPrefab);
            leftPaw.transform.parent = camera.transform;
            leftPaw.transform.localPosition = new Vector3(-0.01025674f, -0.08461541f, 0.9628906f);
            leftPaw.transform.localRotation = Quaternion.Euler(90,0,225);
        }
        if (pawMovingLeft) { 
            leftPaw.transform.Rotate(new Vector3(0, 0, -pawSpeed * Time.deltaTime));
            if (leftPaw.transform.localEulerAngles.y > 225)
            {
                pawMovingLeft = false;
                Destroy(leftPaw);
                leftPaw = null;
            }
        }

        if (!pawMovingRight && catControls.Player.RPaw.ReadValue<float>() == 1.0f)
        {
            pawMovingRight = true;
            rightPaw = Instantiate(pawPrefab);
            rightPaw.transform.parent = camera.transform;
            rightPaw.transform.localPosition = new Vector3(-0.01025674f, -0.08461541f, 0.9628906f);
            rightPaw.transform.localRotation = Quaternion.Euler(90, 0, 135);
        }
        if (pawMovingRight) {
            rightPaw.transform.Rotate(new Vector3(0, 0, pawSpeed * Time.deltaTime));
            if (rightPaw.transform.localEulerAngles.y < 135)
            {
                pawMovingRight = false;
                Destroy(rightPaw);
                rightPaw = null;
            }
        }
    }

    private Vector2 GetPlayerMovement()
    {
        return catControls.Player.Move.ReadValue<Vector2>();
    }

    private Vector2 GetPlayerLook()
    {
        return catControls.Player.Look.ReadValue<Vector2>();
    }
}

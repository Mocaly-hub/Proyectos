using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerPlatformer : MonoBehaviour
{
    public float speed = 7.5f;
    public float jumpHeight = 2.9f;
    public float gravity = -25f;
    public float turnSpeed = 13f;

    public float HorizontalSpeed { get; private set; }
    public float VerticalSpeed { get { return verticalVelocity.y; } }
    public bool IsGrounded { get { return controller != null && controller.isGrounded; } }

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float coyoteTime = 0.12f;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private Vector3 startingPosition;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        startingPosition = transform.position;
    }

    void Update()
    {
        if (GameFlow.Instance != null && GameFlow.Instance.HasWon) return;

        // Controles: WASD / flechas para moverse, espacio para saltar.
        float x = 0f;
        float z = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z -= 1f;
        Vector3 move = new Vector3(x, 0f, z).normalized;
        HorizontalSpeed = move.magnitude;

        if (controller.isGrounded)
        {
            coyoteCounter = coyoteTime;
            if (verticalVelocity.y < 0f) verticalVelocity.y = -2f;
        }
        else coyoteCounter -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space)) jumpBufferCounter = 0.15f;
        else jumpBufferCounter -= Time.deltaTime;

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteCounter = 0f;
            jumpBufferCounter = 0f;
        }

        if (move.sqrMagnitude > 0.01f)
        {
            Quaternion facing = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, facing, turnSpeed * Time.deltaTime);
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move((move * speed + verticalVelocity) * Time.deltaTime);

        if (transform.position.y < -9f) Respawn();
        if (Input.GetKeyDown(KeyCode.R) && GameFlow.Instance != null) GameFlow.Instance.Restart();
    }

    public void Respawn()
    {
        controller.enabled = false;
        transform.position = GameFlow.Instance == null ? startingPosition : GameFlow.Instance.RespawnPosition;
        transform.rotation = Quaternion.identity;
        controller.enabled = true;
        verticalVelocity = Vector3.zero;
        HorizontalSpeed = 0f;
        coyoteCounter = 0f;
        jumpBufferCounter = 0f;
        if (GameFlow.Instance != null) GameFlow.Instance.ShowMessage("¡Inténtalo de nuevo!", 1.7f);
    }
}

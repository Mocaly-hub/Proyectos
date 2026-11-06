using UnityEngine;

/// <summary>Reproduce Idle, Run, Jump y Fall sin depender de root motion.</summary>
public class PlayerAnimationDriver : MonoBehaviour
{
    public Animator animator;
    public PlayerPlatformer motor;
    private string currentState = "";
    private float airborneTime;

    void LateUpdate()
    {
        if (animator == null || motor == null) return;
        // Un pequeño margen evita pasar al estado de caída en desniveles mínimos.
        if (motor.IsGrounded) airborneTime = 0f;
        else airborneTime += Time.deltaTime;

        string state;
        if (airborneTime > .09f && motor.VerticalSpeed > .25f) state = "Jump";
        else if (airborneTime > .09f && motor.VerticalSpeed <= .25f) state = "Fall";
        else if (motor.HorizontalSpeed > .1f) state = "Run";
        else state = "Idle";

        if (state == currentState) return;
        currentState = state;
        animator.CrossFadeInFixedTime(state, .12f, 0);
    }
}

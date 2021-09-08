using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimStateController : MonoBehaviour
{
    public static PlayerAnimStateController Instance { get; private set; }

    [SerializeField]
    public Animator animator;
    float velocityZ = 0.0f;
    float velocityX = 0.0f;
    public float acceleration = 2.0f;
    public float deceleration = 2.0f;

    public bool on_beat = false;
    public bool failure = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!Player.Instance.inBoss)
        {
            bool forwardPressed = Input.GetKey("w");
            bool backwardPressed = Input.GetKey("s");
            bool leftPressed = Input.GetKey("a");
            bool rightPressed = Input.GetKey("d");

            if (forwardPressed && (velocityZ < 0.5f))
            {
                velocityZ += Time.deltaTime * acceleration;
            }

            if (backwardPressed && (velocityZ > -0.5f))
            {
                velocityZ -= Time.deltaTime * acceleration;
            }

            if (leftPressed && (velocityX > -0.5f))
            {
                velocityX -= Time.deltaTime * acceleration;
            }

            if (rightPressed && (velocityX < 0.5f))
            {
                velocityX += Time.deltaTime * acceleration;
            }

            if (!forwardPressed && velocityZ > 0.0f)
            {
                velocityZ -= Time.deltaTime * deceleration;
            }

            if (!backwardPressed && velocityZ < 0.0f)
            {
                velocityZ += Time.deltaTime * deceleration;
            }

            if (!forwardPressed && !backwardPressed && velocityZ != 0.0f && (velocityZ > -0.05f && velocityZ < 0.05f))
            {
                velocityZ = 0.0f;
            }

            if (!leftPressed && velocityX < 0.0f)
            {
                velocityX += Time.deltaTime * deceleration;
            }

            if (!rightPressed && velocityX > 0.0f)
            {
                velocityX -= Time.deltaTime * deceleration;
            }

            if (!leftPressed && !rightPressed && velocityX != 0.0f && (velocityX > -0.05f && velocityX < 0.05f))
            {
                velocityX = 0.0f;
            }

            animator.SetFloat("velocityX", velocityX);
            animator.SetFloat("velocityZ", velocityZ);
        }

    }
}

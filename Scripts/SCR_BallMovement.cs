using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SCR_BallMovement : MonoBehaviour
{
    [HideInInspector] public int teamNumber; //used to find the winner at the end of the round. Set by round sequence.

    [SerializeField] private AudioSource collisionSound;

    private Rigidbody rb;

    //roll the ball based on power set and direction
    public void RollBall(float power, Vector3 direction)
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.TransformDirection(direction) * power, ForceMode.Force);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("Ball") || collision.collider.CompareTag("Jack"))
        {
            //play collision sound if ball hits the jack or another ball
            collisionSound.Play();
        }
    }
}

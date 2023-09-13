using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//controls the behaviour of the yellow teleporters
public class SCR_Teleporter : MonoBehaviour
{
    [SerializeField] private Transform teleportedPos; //the position of where the ball is spawned to

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            //if touched, teleport ball to teleportedPos
            other.transform.position = teleportedPos.position;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//creates an in game moving obstacle
public class SCR_MovingObstacle : MonoBehaviour
{
    //obstacle details
    [SerializeField] private Vector3 startPos;

    [SerializeField] private Vector3 endPos;

    [SerializeField] private float speed = 1f;

    private bool invertDir = false;

    //the current progress of the Lerp
    private float t = 0f;

    // Update is called once per frame
    void Update()
    {
        if (!invertDir)
        {
            //move forwards at the speed set to the end position
            t += Time.deltaTime * speed;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            
            //if end position is reached, change direction
            if(t >= 1f)
            {
                invertDir = true;
                t = 0f;
            }
        }
        else
        {
            //move forwards at the speed set to the start position
            t += Time.deltaTime * speed;
            transform.position = Vector3.Lerp(endPos, startPos, t);

            //if start position is reached, change direction
            if(t >= 1f)
            {
                invertDir = false;
                t = 0f;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using Random = UnityEngine.Random;

public class Boid_Controller : MonoBehaviour
{
    private Rigidbody rb;
    public enum States
    {
        Static,
        ToPosition,
        Patrol,
        Attack
    }


    [Header("State Machine")]
    public States currentState;

    [Header("ToPosition")]
    public Vector3 positionToGo = Vector3.zero;
    public Vector3 boidSignalPos = Vector3.zero;
    public bool signalRecieved = false;

    [Header("Attack")]
    private Vector3 P0 , P1, P2 ;

    [Header("Position")]
    public Player_Controller controller;
    public float maxX, maxZ;
    public bool isObstacle = false;

    [Header("Mouvement")]
    public float speed = 1;
    public bool stop = true;
    public Vector3 velocity = Vector3.zero;


    [Header("Repulsion")]
    public bool hasRepulsion = true;
    public float minDistance;
    public float scalaireRepulsion = 2f;

    [Header("Alignement")]
    public bool hasAlignement = true;
    public float scalaireAlignement = 1f;

    [Header("Attraction")]
    public bool hasAttraction = true;
    public float scalaireAttraction = 1f;

    public List<GameObject> otherBoids = new List<GameObject>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        otherBoids = new List<GameObject>();
        stayAtPosition();
        //velocity = randDir();
    }

    private Vector3 randDir()
    {
        Vector3 result = Vector3.zero;
        do
        {
            float x = Random.Range(-1, 1f);
            float z = Random.Range(-1, 1f);

            result = new Vector3(x, 0, z);

        } while (result == Vector3.zero);

        return result;

    }

    private void FixedUpdate()
    {
        Vector3 dir = velocity;
        if (hasRepulsion) { dir += (repulsion() * scalaireRepulsion); }
        if (hasAlignement) { dir += (alignement() * scalaireAlignement) ; }
        if (hasAttraction) { dir += (attraction() * scalaireAttraction ) ; }

       // Vector3 dir = rb.linearVelocity + (repulsion() * scalaireRepulsion) + (attraction() * scalaireAttraction) + (alignement() * scalaireAlignement);
        rb.AddForce(dir.normalized * speed);

    }
    // Update is called once per frame
    void Update()
    {

        debugPos();

        //return;

        if (Enum.Equals(currentState, States.ToPosition)){

            goToPosition();

            if (signalRecieved)
            {
                //Debug.Log("Vector3.Distance(transform.position, boidSignalPos) = " + Vector3.Distance(transform.position, boidSignalPos));

                if (Vector3.Distance(transform.position, boidSignalPos) < 1.5f)
                {
                    Debug.Log("Static signalRecieved");
                    signalRecieved = false;
                    ChangeState(States.Static);
                    controller.tellOthersToStop(gameObject);
                }

            }
            else
            {

              
                if (Vector3.Distance(transform.position, positionToGo) < 0.5f)
                {
                    Debug.Log("Static");
                    ChangeState(States.Static);
                    controller.tellOthersToStop(gameObject);
                }

            }

           

        }




        /*if (controller.positionToGo != Vector3.zero  && !Enum.Equals(currentState, States.ToPosition))
        {
            ChangeState(States.ToPosition);
        }*/

        if (Input.GetKeyDown(KeyCode.T)) {
            Debug.Log("pressed");
            ChangeState(States.Patrol);
        }

        
    }

    public void ChangeState(States newState)
    {
        
        switch (newState)
        {
            case States.Static:
                //controller.positionToGo = Vector3.zero;

                stayAtPosition();
                minDistance = 4f;
                break;
            case States.ToPosition:
                signalRecieved = false;
                minDistance = 2f;


                break;
            case States.Patrol:
                rb.angularVelocity = Vector3.zero;
                minDistance = 4f;
                patrol();
                break;
            case States.Attack:

                break;
            default:
                break;
        }

        currentState = newState;

    }



    public void stopSignal(Vector3 pos)
    {
        boidSignalPos = pos;
        signalRecieved = true;
    }

    private void tellOthersToMove()
    {
        foreach (GameObject boid in otherBoids)
        {
            boid.GetComponent<Boid_Controller>().goToPosition();
        }
    }

    public void goToPosition()
    {
        //if (!hasAlignement) { hasAlignement = true; }
        //if (!hasAttraction) { hasAttraction = true; }

        //Vector3 pos = new Vector3(controller.positionToGo.x, 0, controller.positionToGo.z);

        velocity = (positionToGo - transform.position).normalized;

    }
    
    private void stayAtPosition()
    {
        if (hasAlignement) { hasAlignement = false;  }
        if (hasAttraction) { hasAttraction = false; }

        velocity = Vector3.zero;
        rb.angularVelocity = velocity;

    }

    private void patrol()
    {
        if (!hasAlignement) { hasAlignement = true; }
        if (!hasAttraction) { hasAttraction = true; }
        if(!hasRepulsion) { hasRepulsion = true; }
        velocity = randDir();

    }
    private Vector3 repulsion()
    {

        Vector3 averageDir = Vector3.zero;
        float nbBoids = 0;
        foreach (GameObject boid in otherBoids)
        {
            if(Vector3.Distance(transform.position , boid.transform.position) < minDistance)
            {
                averageDir +=  transform.position - boid.transform.position ;
                nbBoids++;
            }
        }


        if (nbBoids != 0)
        {
            averageDir /= nbBoids;
        }

        return averageDir;

    }

    private Vector3 alignement()
    {
        Vector3 averageDir = Vector3.zero;
        float nbBoids = 0;
        foreach (GameObject boid in otherBoids) {
            averageDir += boid.GetComponent<Rigidbody>().linearVelocity;
            nbBoids++;
        }


        if (nbBoids != 0) 
        {
            averageDir /= nbBoids;
        }

            return averageDir;

    }

    private Vector3 attraction()
    {
        Vector3 averageDir = Vector3.zero;
        float nbBoids = 0;
        foreach (GameObject boid in otherBoids)
        {
            averageDir +=  boid.transform.position;
            nbBoids++;
        }

        if (nbBoids != 0)
        {
           averageDir /= nbBoids;
           averageDir -= transform.position;
        }


        return averageDir;

    }

  
    private void debugPos()
    {
        if (transform.position.x > maxX)
        {
            transform.position = new Vector3(-(transform.position.x - 5),transform.position.y,transform.position.z);
            rb.linearVelocity = Vector3.zero;
        }

        if (transform.position.x < -maxX)
        {
            transform.position = new Vector3(-(transform.position.x + 5), transform.position.y, transform.position.z);
            rb.linearVelocity = Vector3.zero;
        }

        if (transform.position.z > maxZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -(transform.position.z - 5));
            rb.linearVelocity = Vector3.zero;
        }


        if (transform.position.z < -maxZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -(transform.position.z + 5));
            rb.linearVelocity = Vector3.zero;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collide");
        Vector3 normal = collision.GetContact(0).normal;
        rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, normal);

    }

    private void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(boidSignalPos, 1f);
    }
}

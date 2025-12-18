using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using static UnityEngine.GraphicsBuffer;
using Random = UnityEngine.Random;

public class Boid_Controller : MonoBehaviour
{
    private Rigidbody rb;
    public enum States
    {
        Static,
        ToPosition,
        Patrol,
        Attack,
        Boid
    }


    [Header("State Machine")]
    public States currentState;

    [Header("ToPosition")]
    public Vector3 positionToGo = Vector3.zero;
    public Vector3 boidSignalPos = Vector3.zero;
    public bool signalRecieved = false;

    [Header("Patrol")]
    public float changeDirDuration = 5f;
    private float changeDirElasped = 6f;
    private Coroutine patrolC = null;

    [Header("Attack")]
    public Vector3 positionToAttack = Vector3.zero;
    private Vector3 tempDir = Vector3.zero;
    private Vector3 P0 , P1, P2 ;
    private float t = 0;
    public float berzierSpeed = 0.5f;
    private bool berzierMouv = false;
    private Coroutine waitToattackC = null;
    private Coroutine waitToCurvedC = null;

    [Header("Position")]
    public Player_Controller controller;
    public float maxX, maxZ;
    public bool isObstacle = false;
    public float obstacleStrenght;

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
    private bool goToTargetAgain;

    private Vector3 dirToLook = Vector3.zero;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        otherBoids = new List<GameObject>();

        if (isObstacle)
        {
            hasRepulsion = true;
            hasAlignement = false;
            hasAttraction = false;
            scalaireRepulsion = 8.5f;
        }
        else
        {
            stayAtPosition();
        }


 
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
        if (isObstacle)
        {
            return;
        }


        if (berzierMouv && waitToattackC == null)
        {
            return;    
        }

        Vector3 dir = velocity;
        if (hasRepulsion) { dir += (repulsion() * scalaireRepulsion); }
        if (hasAlignement) { dir += (alignement() * scalaireAlignement) ; }
        if (hasAttraction) { dir += (attraction() * scalaireAttraction ) ; }

        dirToLook = dir;

        rb.AddForce(dir.normalized * speed);


        if (rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            rb.MoveRotation(
                Quaternion.Slerp(
                    rb.rotation,
                    Quaternion.LookRotation(rb.linearVelocity.normalized),
                    0.2f
                )
            );
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (isObstacle)
        {
            return;
        }


        debugPos();

        //return;

        /*dirToLook = transform.position + dirToLook;
        transform.LookAt(dirToLook);*/

        stateMachine();

        if (Input.GetKeyDown(KeyCode.T)) {
            Debug.Log("pressed T");
            ChangeState(States.Patrol);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {

            Debug.Log("pressed Y");
            ChangeState(States.Boid);
        }


    }


    private void stateMachine()
    {
        
        switch (currentState)
        {
            case States.ToPosition:

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


                break;
            case States.Patrol:

                if (patrolC != null) { return; }

                changeDirElasped += Time.deltaTime;
                if(changeDirElasped > changeDirDuration)
                {
                    Debug.Log("Change direction");
                    if (hasAlignement) { hasAlignement = false; }
                    if (hasAttraction) { hasAttraction = false; }
                    if (hasRepulsion) { hasRepulsion = false; }

                    velocity = randDir();
                    patrolC = StartCoroutine(waitToActivatePatrol());
                }

                break;
            case States.Attack:

                //Debug.Log("distance = "+Vector3.Distance(transform.position, positionToAttack));

                if (berzierMouv)
                {
                    if (t < 1f)
                    {
                        transform.position = P1 + Mathf.Pow((1 - t), 2) * (P0 - P1) + Mathf.Pow(t, 2) * (P2 - P1);
                        Vector3 tangent = 2f * (1f - t) * (P1 - P0) + 2f * t * (P2 - P1);
                        if (tangent.sqrMagnitude > 0.001f)
                        {
                            transform.LookAt(transform.position + tangent);
                        }

                        t = t + berzierSpeed * Time.deltaTime;

                        if( t>0.5f && t < 0.6f)
                        {
                           tempDir = (P2 - transform.position).normalized;
                        }

                    }
                    else
                    {
                        if(waitToattackC == null)
                        {
                            velocity = tempDir;
                            rb.linearVelocity = velocity * 50f;
                            waitToattackC = StartCoroutine(waitToattack());
                        }
                        
                    }

                }
                else
                {
                    if (waitToCurvedC != null)
                    {
                        return;
                    }

                    goToTarget();

                    if (Vector3.Distance(transform.position, positionToAttack) < 1f)
                    {
                        ///Debug.Log("Wait to turn");
                        waitToCurvedC = StartCoroutine(waitToCurvedMouvement());
                    }

                }



                break;
            default:
                break;
        }
    }



    public void ChangeState(States newState)
    {

        signalRecieved = false;
        berzierMouv = false;
        StopAllCoroutines();
        t = 0;
        patrolC = null;
        waitToattackC = null;
        waitToCurvedC = null;

        switch (newState)
        {
            case States.Static:
                minDistance = 7f;
                stayAtPosition();

                break;

            case States.ToPosition:
                if (!hasRepulsion) { hasRepulsion = true; }

                if (hasAlignement) { hasAlignement = false; }
                if (hasAttraction) { hasAttraction = false; }
                minDistance = 2f;


                break;
            case States.Patrol:
                minDistance = 7f;
                if (hasRepulsion) { hasRepulsion = true; }
                patrol();

                break;
            case States.Attack:

                minDistance = 3.5f;

                if (hasAlignement) { hasAlignement = false; }
                if (hasAttraction) { hasAttraction = false; }
                if (hasRepulsion) { hasRepulsion = false; }

                goToTarget();
                break;

            case States.Boid:
                minDistance = 9f;
                scalaireRepulsion = 6;

                if (hasAlignement) { hasAlignement = false; }
                if (hasAttraction) { hasAttraction = false; }
                if (hasRepulsion) { hasRepulsion = false; }

                rb.angularVelocity = Vector3.zero;
                velocity = randDir();
                StartCoroutine(waitToActivateBoid());

                break;
            default:
                break;
        }

        currentState = newState;

    }


    private IEnumerator waitToActivateBoid()
    {
        yield return new WaitForSeconds(1);
        if (!hasAlignement) { hasAlignement = true; }
        if (!hasAttraction) { hasAttraction = true; }
        if (!hasRepulsion) { hasRepulsion = true; }
        rb.angularVelocity = Vector3.zero;
        velocity = randDir();
    }


    public void stopSignal(Vector3 pos)
    {
        boidSignalPos = pos;
        signalRecieved = true;
    }


    public void goToPosition()
    {
        velocity = (positionToGo - transform.position).normalized;

    }

    public void goToTarget()
    {
        rb.angularVelocity = Vector3.zero;
        velocity = (positionToAttack - transform.position).normalized;

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

        rb.angularVelocity = Vector3.zero;  
        velocity = randDir();
        patrolC = StartCoroutine(waitToActivatePatrol());


    }


    private IEnumerator waitToActivatePatrol()
    {
        yield return new WaitForSeconds(3);

        if (!hasAlignement) { hasAlignement = true; }
        if (!hasAttraction) { hasAttraction = true; }
        if (!hasRepulsion) { hasRepulsion = true; }
        changeDirElasped = 0;
        patrolC = null;
    }
    private Vector3 repulsion()
    {

        Vector3 averageDir = Vector3.zero;
        float nbBoids = 0;

        foreach (GameObject boid in otherBoids)
        {
            Vector3 diff = transform.position - boid.transform.position;
            float dist = diff.magnitude;

            if (dist < minDistance && dist > 0.0001f)
            {
           
                float strength = (minDistance - dist) / minDistance;
   

                if (boid.GetComponent<Boid_Controller>().isObstacle)
                {
                    strength *= obstacleStrenght;//  100f; 
                }

                averageDir += diff.normalized * strength;
                nbBoids++;
            }
        }


        if (nbBoids != 0)
        {
            averageDir /= nbBoids;
            averageDir = averageDir.normalized;
        }

        return averageDir;

    }

    private Vector3 alignement()
    {
        Vector3 averageDir = Vector3.zero;
        float nbBoids = 0;
        foreach (GameObject boid in otherBoids) {

            if (boid.GetComponent<Boid_Controller>().isObstacle)
            {
                continue;
            }

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
            if (boid.GetComponent<Boid_Controller>().isObstacle)
            {
                continue;

            }
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

    private void setControlPoints()
    {
     
        float p1Z = -30;
        float sign = 1;

        int rand = Random.Range(0, 2);

        if(rand == 1)
        {
            sign *= -1;
        }

      

        P0 = transform.position;

        if (Mathf.Abs(transform.position.x - positionToAttack.x) < 10f)
        {
            P1 = new Vector3(transform.position.x + (30* sign), transform.position.y, transform.position.z );
        }
        else
        {
            P1 = new Vector3(transform.position.x, transform.position.y, transform.position.z + (30 * sign));
        }

        
        P2 = positionToAttack;
    }

    private IEnumerator waitToCurvedMouvement()
    {
        yield return new WaitForSeconds(1f);
        if (!hasAlignement) { hasAlignement = true; }
        if (!hasRepulsion) { hasRepulsion = true; }

        setControlPoints();
        berzierMouv = true;
        rb.angularVelocity = Vector3.zero;
        velocity = Vector3.zero;
        waitToCurvedC = null;

    }

    private IEnumerator waitToattack()
    {
        yield return new WaitForSeconds(1f);

        rb.angularVelocity = Vector3.zero;
        velocity = Vector3.zero;
        setControlPoints();
        t = 0f;

        waitToattackC = null;
       

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

        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(P0, 1f);

        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(P1, 1f);

        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(P2, 1f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(positionToAttack, 1f);
    }
}

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Boid_Controller : MonoBehaviour
{
    private Rigidbody rb;
    private SphereCollider sphC;
    private Vector3 velocity = Vector3.zero;
    public float speed = 1;
    public bool isObstacle = false;

    [Header("repulsion")]
    public float minDistance;
    public float scalaireRepulsion = 2f;

    public List<GameObject> otherBoids = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphC = GetComponent<SphereCollider>();
        otherBoids = new List<GameObject>();
        velocity = randDir();
        rb.linearVelocity = velocity;
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
        //Debug.Log("Result =" + result);

        return result;

    }

    private void FixedUpdate()
    {
        //Vector3 dir = velocity + repulsion() + attraction() + alignement();
        Vector3 dir = rb.linearVelocity + (repulsion() * scalaireRepulsion) + attraction() + alignement();
        rb.AddForce(dir.normalized * speed);

    }
    // Update is called once per frame
    void Update()
    {
        //maxZone();

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
            //averageDir += boid.GetComponent<Boid_Controller>().velocity;
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
            //averageDir += transform.position - boid.transform.position;
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

  


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collide");
        Vector3 normal = collision.GetContact(0).normal;
        rb.linearVelocity = Vector3.Reflect(rb.linearVelocity, normal);

    }
}

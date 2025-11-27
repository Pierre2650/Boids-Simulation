using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Boid_Controller : MonoBehaviour
{
    private Rigidbody rb;
    private SphereCollider sphC;
    private Vector3 velocity = Vector3.zero;
    public float speed = 1;
    public bool isObstacle = false;

    public List<GameObject> otherBoids = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphC = GetComponent<SphereCollider>();

        do
        {
            float x = Random.Range(0, 2);
            float z = Random.Range(0, 2);

            velocity = new Vector3(x, 0, z);

        } while (velocity == Vector3.zero);
    }


    private void FixedUpdate()
    {
        rb.AddForce(velocity.normalized * speed);
    }
    // Update is called once per frame
    void Update()
    {
        //maxZone();

    }

    private void LateUpdate()
    {
        maxZone();
    }

    private void repulsion()
    {

    }

    private void alignement()
    {

    }

    private void attraction()
    {

    }

  

    private void maxZone()
    {
        // DEBUG ONLY
        if (transform.position.x > 36)
        {

            transform.position = new Vector3(35, transform.position.y, transform.position.z);
            velocity.x *= -1;
        }
        else if (transform.position.x < -36)
        {
            transform.position = new Vector3(-35, transform.position.y, transform.position.z);
            velocity.x *= -1;
        }

        if (transform.position.z > 36)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 35);
            velocity.z *= -1;
        }
        else if (transform.position.z < -36)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, -35);
            velocity.z *= -1;
        }
 
    }
}

using UnityEngine;

public class aBoidZone : MonoBehaviour
{
    public Boid_Controller controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<aBoidZone>())
        {
            controller.otherBoids.Add(other.transform.parent.gameObject);
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<aBoidZone>())
        {
            controller.otherBoids.Remove(other.transform.parent.gameObject); 
        }
    }
}

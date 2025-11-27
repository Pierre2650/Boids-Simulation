using UnityEngine;

public class aBoidZone : MonoBehaviour
{
    public Boid_Controller controller;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        controller.otherBoids.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        controller.otherBoids.Remove(other.gameObject);
    }
}

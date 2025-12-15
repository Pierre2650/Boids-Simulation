using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using static UnityEditor.PlayerSettings;

public class Player_Controller : MonoBehaviour
{
    public GameObject test;

    [Header("Camera")]
    public Transform cameraT;
    public float speed;
    public int screenLimit = 100;
    private Vector3 velocity = Vector3.zero;

    [Header("Mouse")]
    public Vector3 positionToGo = Vector3.zero;
    public LayerMask Terrain;

    [Header("Boids")]
    public GameObject[] allBoids;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //cameraMouvement();

    }

    private void cameraMouvement()
    {
        velocity = Vector3.zero;

        if (Input.mousePosition.x < screenLimit)
        {
            velocity = new Vector3(-1, 0, 0);
        }


        if (Input.mousePosition.x > Screen.width - screenLimit)
        {
            velocity = new Vector3(1, 0, 0);
        }

        if (Input.mousePosition.y < screenLimit)
        {
            velocity = new Vector3(velocity.x, 0, -1);

        }


        if (Input.mousePosition.y > Screen.height - screenLimit)
        {
            velocity = new Vector3(velocity.x, 0, 1);
        }

        cameraT.position = Vector3.Lerp(cameraT.position, cameraT.position + velocity.normalized * speed , Time.deltaTime);

    }

    public void leftMouseButtonAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Vector3 pos =  getMouseGamePosition();
            foreach (GameObject go in allBoids) {
                go.GetComponent<Boid_Controller>().positionToGo  = new Vector3(pos.x, 0, pos.z); 
                go.GetComponent<Boid_Controller>().ChangeState(Boid_Controller.States.ToPosition);
            }
        }
    }


    public void tellOthersToStop(GameObject caller)
    {
        foreach (GameObject boid in allBoids)
        {
            if (GameObject.ReferenceEquals(boid, caller)) { continue; }

            Boid_Controller temp = boid.GetComponent<Boid_Controller>();

            if (Vector3.Distance(boid.transform.position, caller.transform.position) < 1.5f) {
                Debug.Log("caller told other to static");
                temp.ChangeState(Boid_Controller.States.Static);
                continue;
            }
            else
            {
                if (temp.signalRecieved) { continue; }
                Debug.Log("caller told other to stopSignal");
                temp.stopSignal(caller.transform.position);
            }

        }
    }
    private Vector3 getMouseGamePosition()
    {
        Camera camera = Camera.main;

        Ray camRayToMap = camera.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(camRayToMap, out RaycastHit result, float.MaxValue, Terrain))
        {
            return result.point;
           
        }

        return Vector3.zero;
    }

}

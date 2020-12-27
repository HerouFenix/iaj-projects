using UnityEngine;
using System.Collections;

public class BulletUFO : MonoBehaviour
{
    public GameObject playerObj;
    // Use this for initialization
    void Start()
    {
        //GameObject playerObj = GameObject.FindGameObjectWithTag("SpaceShip");
        //
        
        transform.LookAt(playerObj.transform.position);

        // Set the bullet to destroy itself after 1 seconds
        Destroy(gameObject, 4.0f);

        // Push the bullet in the direction it is facing
        GetComponent<Rigidbody>()
            .AddForce(transform.forward * 4000f);
    }

}
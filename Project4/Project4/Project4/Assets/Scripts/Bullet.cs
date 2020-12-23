using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public int ID;

    // Use this for initialization
    void Start()
    {
        // Set the bullet to destroy itself after 1 seconds
        Destroy(gameObject, 3.5f);

        // Push the bullet in the direction it is facing
        GetComponent<Rigidbody>()
            .AddForce(transform.forward * 5500f);
    }

}
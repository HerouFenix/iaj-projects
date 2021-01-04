using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    bool hit = false;
    public GameObject ship = null;

    // Use this for initialization
    void Start()
    {
        // Set the bullet to destroy itself after 1 seconds
        Destroy(gameObject, 2.0f);

        // Push the bullet in the direction it is facing
        GetComponent<Rigidbody>()
            .AddForce(transform.forward * 6000f);
    }

    void OnTriggerEnter(Collider c)
    {
        //if (!c.gameObject.tag.Equals("SpaceShip") && !c.gameObject.tag.Equals("Bullet") && !c.gameObject.tag.Equals("Bullet_UFO"))
        //{
        //    hit = true;
        //}
    }

    private void OnDestroy()
    {
        //if (!hit && ship != null)
        //{
        //    ship.GetComponent<Ship>().IncrementScore(-0.25f);
        //}   
    }
}
using UnityEngine;
using System.Collections;

public class BulletUFO : MonoBehaviour
{
    public int ID;

    // Use this for initialization
    void Start()
    {
        GameObject[] allPlayerObj = GameObject.FindGameObjectsWithTag("SpaceShip");
        GameObject playerObj = null;
        for (int i = 0; i < allPlayerObj.Length; i++)
        {
            if (allPlayerObj[i].GetComponent<Ship>().ID == ID)
            {
                playerObj = allPlayerObj[i];
                break;
            }
        }

        if (playerObj != null)
            transform.LookAt(playerObj.transform.position);

        // Set the bullet to destroy itself after 1 seconds
        Destroy(gameObject, 4.0f);

        // Push the bullet in the direction it is facing
        GetComponent<Rigidbody>()
            .AddForce(transform.forward * 4000f);
    }

}
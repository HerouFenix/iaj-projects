using UnityEngine;
using System.Collections;

public class EuclideanTorus : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {

        // Teleport the game object
        if (transform.position.x > 300)
        {

            transform.position = new Vector3(-300, 0, transform.position.z);

        }
        else if (transform.position.x < -300)
        {
            transform.position = new Vector3(300, 0, transform.position.z);
        }

        else if (transform.position.z > 165)
        {
            transform.position = new Vector3(transform.position.x, 0, -165);
        }

        else if (transform.position.z < -165)
        {
            transform.position = new Vector3(transform.position.x, 0, 165);
        }
    }
}
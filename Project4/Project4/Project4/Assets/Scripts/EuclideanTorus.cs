using UnityEngine;
using System.Collections;

public class EuclideanTorus : MonoBehaviour
{
    private bool isWrappingX = false;
    private bool isWrappingY = false;

    float MAX_TIME_OUT_OF_BOUNDS = 1.5f;
    float timeOutOfBounds = 0.0f;

    Camera cam;

    Renderer m_Renderer;
    // Use this for initialization
    void Start()
    {
        cam = Camera.main;
        m_Renderer = GetComponent<Renderer>();
    }

    /*
    void OnBecameInvisible()
    {
        var viewportPosition = cam.WorldToViewportPoint(transform.position);
        var newPosition = transform.position;

        if (viewportPosition.x > 1 || viewportPosition.x < 0)
        {
            newPosition.x = -newPosition.x;

            isWrappingX = true;
        }

        if (viewportPosition.y > 1 || viewportPosition.y < 0)
        {
            newPosition.z = -newPosition.z;

            isWrappingY = true;
        }

        transform.position = newPosition;
    }
    */
    
    // Update is called once per frame
    void Update()
    {

        var isVisible = m_Renderer.isVisible;

        if (isVisible)
        {
            isWrappingX = false;
            isWrappingY = false;
            return;
        }

        if (isWrappingX && isWrappingY)
        {
            return;
        }

        var viewportPosition = cam.WorldToViewportPoint(transform.position);
        var newPosition = transform.position;

        if (!isWrappingX && (viewportPosition.x > 1 || viewportPosition.x < 0))
        {
            if(viewportPosition.x > 0)
            {
                newPosition.x = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
            }
            else
            {
                newPosition.x = cam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
            }
            //newPosition.x = -newPosition.x;

            isWrappingX = true;
        }

        if (!isWrappingY && (viewportPosition.y > 1 || viewportPosition.y < 0))
        {
            if (viewportPosition.y > 0)
            {
                newPosition.z = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).z;
            }
            else
            {
                newPosition.z = cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).z;
            }

            //newPosition.z = -newPosition.z;

            isWrappingY = true;
        }

        transform.position = newPosition;
    }
    
}
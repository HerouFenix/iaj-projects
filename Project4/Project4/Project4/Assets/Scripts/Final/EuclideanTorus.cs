using UnityEngine;
using System.Collections;

public class EuclideanTorus : MonoBehaviour
{
    private bool isWrappingX = false;
    private bool isWrappingY = false;
    public Camera cam;
    Plane[] planes;
    Collider[] objColliders;

    //Renderer m_Renderer;
    // Use this for initialization
    void Start()
    {
        //m_Renderer = GetComponent<Renderer>();
        planes = GeometryUtility.CalculateFrustumPlanes(cam);
        objColliders = GetComponents<Collider>();
    }

    // Update is called once per frame
    void Update()
    {

        //var isVisible = m_Renderer.isVisible;
        //Vector3 screenPoint = cam.WorldToViewportPoint(transform.position);
        //bool isVisible = screenPoint.x > 0.025 && screenPoint.x < 1.025 && screenPoint.y > 0.025 && screenPoint.y < 1.025;

        bool isVisible = true;
        foreach (Collider col in objColliders)
            if (!GeometryUtility.TestPlanesAABB(planes, col.bounds))
            {
                isVisible = false;
                break;
            }


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
            if (viewportPosition.x > 0)
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
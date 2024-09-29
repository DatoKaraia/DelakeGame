using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject playerCamera;
    public float cameraXSensitivity = 3;
    public float cameraYSensitivity = 3;
    public float lowerXLimit = -45;
    public float higherXLimit = 90;
    public float cameraDistanceTop = 3;
    public float cameraDistanceMiddle = 4;
    public float cameraDistanceBottom = 3;
    public float lockedOnXOffset = 2;
    public float lockedOnYOffset = 2;
    public float lockOnRange = 10;
    public float lockOnRetargetDelta = .2f;
    public int lockOnRetargetAngle = 30;

    float cameraYRotation, cameraXRotation, cameraDistance, cameraFinalDistance;
    RaycastHit cameraRay;
    Camera playerViewport;
    [NonSerialized] public GameObject targetLockOn;
    bool canTarget = true;

    // Start is called before the first frame update
    void Start()
    {
        playerViewport = playerCamera.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // Updates side-to-side and up-and-down movement
        cameraYRotation += Input.GetAxis("Mouse X") * cameraXSensitivity;
        cameraXRotation -= Input.GetAxis("Mouse Y") * cameraYSensitivity;

        // Makes the variable loop from -180 to 180. Stops the rotation becoming -14432943 if you spin around too much
        if (Mathf.Abs(cameraYRotation) > 180)
        {
            cameraYRotation -= 360 * Mathf.Sign(cameraYRotation);
        }

        if (cameraXRotation <= lowerXLimit)
        {
            cameraXRotation = lowerXLimit;
        }
        if (cameraXRotation >= higherXLimit)
        {
            cameraXRotation = higherXLimit;
        }


        // Lock On

        if (Input.GetButtonDown("Lock On"))
        {
            if (targetLockOn == null && canTarget)
            {
                StartCoroutine("lockOnAction");
            }

            else
            {
                //if (targetLockOn != null) targetLockOn.transform.parent.gameObject.layer = LayerMask.NameToLayer("Default");

                targetLockOn = null;
            }
        }

        if (targetLockOn != null)
        {
            if (Vector2.Distance(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")), Vector2.zero) > lockOnRetargetDelta && canTarget)
            {
                StartCoroutine("lockOnRetarget");
            }

            cameraXRotation = Mathf.Rad2Deg * -Mathf.Asin((targetLockOn.transform.position.y - transform.position.y) / Vector3.Distance(transform.position, targetLockOn.transform.position));
            cameraYRotation = Mathf.Rad2Deg * Mathf.Atan2(targetLockOn.transform.position.x - transform.position.x, targetLockOn.transform.position.z - transform.position.z);

            if (Vector3.Distance(transform.position, targetLockOn.transform.position) >= lockOnRange * 1.1f)
            {
                targetLockOn = null;
            }
        }


        // Zoom

        if (cameraYRotation <= 0)
        {
            cameraDistance = (cameraDistanceMiddle * cameraDistanceTop) / Mathf.Sqrt(Mathf.Pow(cameraDistanceTop * Mathf.Cos(cameraXRotation * Mathf.Deg2Rad), 2f) + Mathf.Pow(cameraDistanceMiddle * Mathf.Sin(cameraXRotation * Mathf.Deg2Rad), 2f));
        }
        else
        {
            cameraDistance = (cameraDistanceMiddle * cameraDistanceBottom) / Mathf.Sqrt(Mathf.Pow(cameraDistanceBottom * Mathf.Cos(cameraXRotation * Mathf.Deg2Rad), 2f) + Mathf.Pow(cameraDistanceMiddle * Mathf.Sin(cameraXRotation * Mathf.Deg2Rad), 2f));
        }

        cameraFinalDistance = cameraDistance;

        if (Physics.SphereCast(transform.position, .3f, playerCamera.transform.position - transform.position, out cameraRay, cameraDistance, LayerMask.GetMask("Default")))
        {
            cameraFinalDistance = cameraRay.distance - .2f;
        }

        transform.SetPositionAndRotation(transform.position, Quaternion.Euler(cameraXRotation, cameraYRotation, 0f));
        playerCamera.transform.localPosition = new Vector3(lockedOnXOffset, lockedOnYOffset, -cameraFinalDistance);
    }



    IEnumerator lockOnRetarget()
    {
        canTarget = false;

        if (targetLockOn != null) targetLockOn.transform.parent.gameObject.layer = LayerMask.NameToLayer("Default");

        Vector2 direction = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Lock-On Target");
        GameObject currentTarget = targetLockOn;
        Vector3 currentLocation;
        float targetDist = 1f;

        Vector3 currentTargetLocation = playerViewport.WorldToViewportPoint(currentTarget.transform.position);

        foreach (GameObject obj in allTargets)
        {
            currentLocation = playerViewport.WorldToViewportPoint(obj.transform.position);

            if (obj != targetLockOn && Vector2.Angle(direction, new Vector2(currentLocation.x - currentTargetLocation.x, currentLocation.y - currentTargetLocation.y)) <= lockOnRetargetAngle)
            {

                if (Vector2.Distance(new Vector2(currentLocation.x, currentLocation.y), new Vector2(.5f, .5f)) < targetDist && currentLocation.z <= lockOnRange)
                {
                    if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(playerViewport), obj.GetComponent<Collider>().bounds))
                    {
                        targetDist = Vector2.Distance(new Vector2(currentLocation.x, currentLocation.y), new Vector2(.5f, .5f));
                        currentTarget = obj;
                    }
                }
            }

        }
        targetLockOn = currentTarget;

        if (targetLockOn != null)
        {
            //targetLockOn.transform.parent.gameObject.layer = LayerMask.NameToLayer("Targeted");
        }

        yield return new WaitForSeconds(.2f);

        canTarget = true;
    }

    IEnumerator lockOnAction()
    {
        canTarget = false;

        GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Lock-On Target");

        Vector3 currentLocation;
        float targetDist = 1f;

        foreach (GameObject obj in allTargets)
        {
            currentLocation = playerViewport.WorldToViewportPoint(obj.transform.position);

            if (Vector2.Distance(new Vector2(currentLocation.x, currentLocation.y), new Vector2(.5f, .5f)) < targetDist && currentLocation.z <= lockOnRange)
            {
                if (GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(playerViewport), obj.GetComponentInParent<Collider>().bounds))
                {
                    targetDist = Vector2.Distance(new Vector2(currentLocation.x, currentLocation.y), new Vector2(.5f, .5f));
                    targetLockOn = obj;
                }
            }
        }

        if (targetLockOn != null)
        {
            //targetLockOn.transform.parent.gameObject.layer = LayerMask.NameToLayer("Targeted");
        }

        yield return new WaitForSeconds(.2f);

        canTarget = true;
    }
}

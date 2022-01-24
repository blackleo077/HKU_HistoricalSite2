using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundaryController : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject TestCube;
    public GameObject WarningBoardPrefab;

    WarningBoard warningBoard;
    OVRCameraRig ovrRig;
    OVRBoundary myboundary;
    Camera maincam;

    [SerializeField]
    float SafetyDistance = 0.5f;
    [SerializeField]
    float cameraoffset = 1f;

    void Start()
    {
        //spawn
        ovrRig = GetComponentInChildren<OVRCameraRig>();
        maincam = Camera.main;


        myboundary = OVRManager.boundary;
        myboundary.SetVisible(false);
    }

    // Update is called once per frame
    void Update()
    {
        CheckHeadDistance();
    }


    void CheckHeadDistance()
    {
        OVRBoundary.BoundaryTestResult headResult = myboundary.TestNode(OVRBoundary.Node.Head, OVRBoundary.BoundaryType.OuterBoundary);
        if (headResult.ClosestDistance < SafetyDistance)
        {
            Vector3 targetpos = maincam.transform.position;
            targetpos = maincam.transform.TransformDirection(Vector3.forward*cameraoffset) + maincam.transform.position;
            if (!warningBoard)
            {
                warningBoard = GameObject.Instantiate(WarningBoardPrefab, maincam.transform.position, Quaternion.identity).GetComponent<WarningBoard>();
            }
            Debug.Log(targetpos);
                warningBoard.ShowBoard(targetpos);
        }
        else
        {
            if (!warningBoard)
                return;
            warningBoard.HideBoard();
            Destroy(warningBoard);
        }
    }





    void CheckDistanceOfBoundary()
    {
         OVRBoundary.BoundaryTestResult headResult =  myboundary.TestNode(OVRBoundary.Node.Head, OVRBoundary.BoundaryType.OuterBoundary);
            // OVRBoundary.BoundaryTestResult LHandResult = myboundary.TestNode(OVRBoundary.Node.HandLeft, OVRBoundary.BoundaryType.OuterBoundary);
         OVRBoundary.BoundaryTestResult RHandResult = myboundary.TestNode(OVRBoundary.Node.HandRight, OVRBoundary.BoundaryType.OuterBoundary);



        Debug.LogError("TrackSpace :" + ovrRig.trackingSpace);

        Debug.LogError("trackerAnchor :" + ovrRig.trackerAnchor.position);


        // Debug.LogError("testCP :" + testCP.transform.localPosition);
        Debug.LogError("CamPos World:" + Camera.main.transform.position);

        if (headResult.ClosestDistance < SafetyDistance)
        {
            warningBoard.ShowBoard(headResult.ClosestPoint.ToVector3f().FromVector3f()); 
        }
        else
        {
            warningBoard.HideBoard();
        }

        if (RHandResult.ClosestDistance < 0.2f)
        {
            GameObject g = GameObject.Instantiate(TestCube, ovrRig.trackingSpace);
            g.transform.localPosition = RHandResult.ClosestPoint;
        }
    }


    void setBoundaryCorner()
    {
        Vector3[] point = myboundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
        for(int i = 0; i < point.Length; i++)
        {
            GameObject g = GameObject.Instantiate(TestCube, ovrRig.transform.GetChild(0));
            Vector3 newpos = point[i];
            newpos.y = 1f;
            Debug.LogError(newpos);
            g.transform.localPosition = newpos;
        }
    }
}

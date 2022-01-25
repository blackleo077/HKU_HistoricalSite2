using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportController : MonoBehaviour
{
    Vector3[] landingLocation = new Vector3[8];

    GameObject[] testcube = new GameObject[8];


    public GameObject CubePrefab;

    public float offsetDistance = 0.5f;

    PhotonOVRPlayer player;
    private void Start()
    {
        player = transform.GetComponentInParent<PhotonOVRPlayer>();
        for(int i=0;i<testcube.Length;i++)
        {
           // testcube[i] = GameObject.Instantiate(CubePrefab);
        }
    }

    public void Teleport(Vector3 pos)
    {
        transform.position = pos;
    }

    void FindPossibleLandingLocation()
    {
        Vector3 newpos = transform.TransformDirection(Vector3.forward * offsetDistance) + transform.position;

    }
}

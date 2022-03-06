using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfFloating : MonoBehaviour
{
    // Start is called before the first frame update

    private float BoardSpawnYOffset = 1.7f;
    private float degreesPerSecond = 15.0f;
    private float amplitude = 0.01f;
    private float frequency = 1f;

    private Vector3 objectStartPos = Vector3.zero;

    public SelfFloating(Vector3 startpos)
    {
        objectStartPos = startpos;
    }

    public Vector3 Floating()
    {
        if(objectStartPos == Vector3.zero)
        {
            Debug.Log(transform.gameObject.name);
            objectStartPos = transform.position;
        }
        Vector3 floatobj = objectStartPos;
        floatobj.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
        return floatobj;
    }

    public void RotateY(Transform rotateObj)
    {
        rotateObj.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);
    }
}

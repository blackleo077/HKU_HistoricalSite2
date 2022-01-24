using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoBoard : MonoBehaviour
{
    private string name;
    private string description;
    private Vector3 SpawnPos;
    private Vector3 FloatingPos;

    bool isFloat = false;
    bool isRotatY = false;
    bool isSpawned = false;

    private float BoardSpawnYOffset = 1.7f;
    public float degreesPerSecond = 15.0f;
    public float amplitude = 0.5f;
    public float frequency = 1f;



    public float PopupSpeed = 10f;
    public Text UI_name;
    public Text UI_description;

    private void Update()
    {
        if (isSpawned)
        {
            if(isFloat) Float();
            if (isRotatY) RotateY();

            transform.LookAt(Camera.main.transform);
            transform.localScale = Vector3.Lerp(transform.localScale, ScaleWithCamerDistance(), 1);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, SpawnPos , PopupSpeed*Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, ScaleWithCamerDistance(), PopupSpeed * Time.deltaTime);
            isSpawned = Vector3.Distance( transform.position , SpawnPos)<0.01f ? true : false;
        }

    }
    public void Init(string name, string description, Vector3 startPos, bool isFloat, bool isRotatY)
    {
        this.name = name;
        this.description = description;
        this.SpawnPos = startPos + new Vector3(0, BoardSpawnYOffset, 0);
        FloatingPos = this.SpawnPos;
        this.isFloat = isFloat;
        this.isRotatY = isRotatY;

        transform.LookAt(Camera.main.transform);
        SetInfo();
    }

    void SetInfo()
    {
        UI_name.text = name;
        UI_description.text = description;
    }

    void Float()
    {
        FloatingPos = SpawnPos;
        FloatingPos.y += Mathf.Sin(Time.fixedTime * Mathf.PI * frequency) * amplitude;
        transform.position = FloatingPos;
    }

    void RotateY()
    {
        transform.Rotate(new Vector3(0f, Time.deltaTime * degreesPerSecond, 0f), Space.World);
    }


    public float FixedSize = .01f;
    Vector3 ScaleWithCamerDistance()
    {
        float distance = (Camera.main.transform.position - transform.position).magnitude;
        float size = distance * FixedSize * Camera.main.fieldOfView;
        return Vector3.one * size;
       
    }


}

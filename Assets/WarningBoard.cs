using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningBoard : MonoBehaviour
{
    GameObject BoardObject;
    public bool isActiving = false;

    [SerializeField]
    float LerpSpeed = 1f;

    private void Awake()
    {
        BoardObject = transform.GetChild(0).gameObject;
        BoardObject.SetActive(false);
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (isActiving)
        {
            BoardObject.transform.LookAt(Camera.main.transform);
        }
    }
    public void ShowBoard(Vector3 pos)
    {
        Vector3 targetpos = pos;
        transform.position = Vector3.Lerp(transform.position, pos, LerpSpeed * Time.deltaTime);
        BoardObject.SetActive(true);
        isActiving = true;
    }

    public void HideBoard()
    {
        if (!isActiving)
            return;
        BoardObject.SetActive(false);
        isActiving = false;
    }
}

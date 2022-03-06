using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoBoard : MonoBehaviour
{
    private string name;
    private string description;
    private Sprite image;
    private Transform root;

    bool isSpawned = false;

    private float BoardSpawnYOffset = 1.7f;

    Vector3 SpawnPos;
    bool isFloat = false;
    bool isRotatY = false;

    public float PopupSpeed = 10f;
    public Image UI_Border;
    [SerializeField]private float BorderOffset = 35f;
    public Text UI_name;
    public Text UI_description;
    public Image UI_Image;

    SelfFloating floatcontroller;

    private void Start()
    {

    }

    private void Update()
    {

        if (isSpawned)
        {
            if(isFloat) Float();
            if (isRotatY) RotateY();
            transform.localScale = Vector3.Lerp(transform.localScale, ScaleWithCamerDistance(), 1);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, SpawnPos , PopupSpeed*Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, ScaleWithCamerDistance(), PopupSpeed * Time.deltaTime);
            isSpawned = Vector3.Distance( transform.position , SpawnPos)<0.01f ? true : false;
        }
        root.LookAt(Camera.main.transform);

    }

    public void Init(string n, string des)
    {
        name = n;
        description = des;
        if (name == "" || description == "")
            return;

        UI_name.text = name;
        UI_description.text = description;
        UI_name.gameObject.SetActive(true);
        UI_description.gameObject.SetActive(true);
    }
    public void Init(Sprite img)
    {
        image = img;
        if (img == null)
        {
            Debug.LogError("Target info board without sprite image");
            return;
        }

        UI_Image.sprite  = image;
        UI_Image.SetNativeSize();
        float MaskHeight = UI_Image.transform.parent.GetComponent<RectTransform>().rect.height;
        Vector2 currentimgSize = UI_Image.rectTransform.sizeDelta;
        float Ratio = currentimgSize.x / currentimgSize.y;
        float HeightDiff = Mathf.Abs( MaskHeight - currentimgSize.y);
        Debug.Log("MaskHeight : "+ MaskHeight);
        Vector2 ImgSize = new Vector2(HeightDiff * Ratio + currentimgSize.x, MaskHeight);
        UI_Image.rectTransform.sizeDelta = ImgSize;

        UI_Border.rectTransform.sizeDelta = new Vector2(ImgSize.x + BorderOffset, UI_Border.rectTransform.sizeDelta.y);//35 border offset
        UI_Image.gameObject.SetActive(true);
    }

    public void SetFloatingStyle(Vector3 startPos, bool isFloat, bool isRotatY)
    {
        SpawnPos = startPos + new Vector3(0, BoardSpawnYOffset, 0);
        floatcontroller = new SelfFloating(SpawnPos);
        this.isFloat = isFloat;
        this.isRotatY = isRotatY;
        root = transform.GetChild(0);
    }
    void Float()
    {
        transform.position = floatcontroller.Floating();
    }

    void RotateY()
    {
        floatcontroller.RotateY(this.transform);
    }


    public float FixedSize = .01f;
    Vector3 ScaleWithCamerDistance()
    {
        float distance = (Camera.main.transform.position - transform.position).magnitude;
        float size = distance * FixedSize * Camera.main.fieldOfView;
        return Vector3.one * size;
       
    }




}

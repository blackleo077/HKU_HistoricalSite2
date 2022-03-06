using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Artifacts : MonoBehaviour
{

    private string m_Name ;
    private string m_Description;
    private string m_Owner;
    private Sprite m_Image;

    private Vector3 m_location;

    private bool isNetworkTrigger;

    public enum discoverStatus {
        hidden,
        visible,
        discoverd,
    }
    private discoverStatus m_Status;

    public enum BoardStyle
    {
        Text,
        Image,
    }
    private BoardStyle myBoardStyle;

    private InfoBoard infoBoard;

    public Vector3 Location
    {
        get { return m_location; }
        set { m_location = value; }
    }
    SelfFloating floatcontroller;
    private void Start()
    {
        floatcontroller = new SelfFloating(transform.position);
    }
    private void Update()
    {
        floatcontroller.RotateY(this.transform);
    }

    public void SetBoardStyle(BoardStyle style)
    {
        myBoardStyle = style;
    }

    #region Public method

    public void SetInfoText()
    {
        m_Name = "Artifact xxx";
        m_Description = "St Mary's contains a Norman font, an ancient brass lectern, buried during the Civil Wars, and some interesting heraldic ornaments which date from the 15th century.";
        m_Status = discoverStatus.hidden;
    }

    public void SetInfoImage(Sprite img)
    {
        m_Image = img;
    }

    public void ShowInfoBoard(bool active)
    {
        if(active)
        {
            if (infoBoard != null)
                return;

            infoBoard = GameObject.Instantiate((GameObject)Resources.Load("InfoBoard") as GameObject, transform.position , Quaternion.identity).GetComponent<InfoBoard>();
            if (myBoardStyle == BoardStyle.Text)
            {
                infoBoard.Init(m_Name, m_Description);
            }
            else if (myBoardStyle == BoardStyle.Image)
            {
                infoBoard.Init(m_Image);
            }
            infoBoard.SetFloatingStyle(transform.position, true, false);

            isNetworkTrigger = true;
        }
        else
        {
            if (infoBoard == null)
                return;

            Debug.Log("kill board2");
            Destroy(infoBoard.gameObject);

        }
    }

    public void setStatus(discoverStatus status)
    {
        m_Status = status;
    }


    #endregion


    #region
    #endregion



}

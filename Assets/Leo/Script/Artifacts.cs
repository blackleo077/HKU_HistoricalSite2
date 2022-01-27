using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Artifacts : MonoBehaviour
{

    private string m_Name ;
    private string m_Description;
    private string m_Owner;
    private Vector3 m_location;

    private bool isNetworkTrigger;

    public enum discoverStatus {
        hidden,
        visible,
        discoverd,
    }

    private discoverStatus m_Status;

    private InfoBoard infoBoard;


   private void Start()
    {
        Init();
    }

    public void Init()
    {
        m_Name = "Artifact xxx";
        m_Description = "St Mary's contains a Norman font, an ancient brass lectern, buried during the Civil Wars, and some interesting heraldic ornaments which date from the 15th century.";
        m_Status = discoverStatus.hidden;
    }

    public void ShowInfoBoard(bool active)
    {
        if(active)
        {
            if (infoBoard != null)
                return;

            infoBoard = GameObject.Instantiate((GameObject)Resources.Load("InfoBoard") as GameObject, transform.position , Quaternion.identity).GetComponent<InfoBoard>();
            infoBoard.Init(m_Name, m_Description, transform.position,true,true);
            isNetworkTrigger = true;
           // NetworkTrigger(true);
        }
        else
        {
            if (infoBoard == null)
                return;

            Debug.Log("kill board2");
            Destroy(infoBoard.gameObject);

           // NetworkTrigger(false);
        }
    }



    public void setStatus(discoverStatus status)
    {
        m_Status = status;
    }

    public Vector3 Location
    {
        get { return m_location; }
        set { m_location = value; }
    }

    [PunRPC]
    void NetworkTrigger(bool nttrigger)
    {
    }

}

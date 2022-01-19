using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonPlayer : MonoBehaviour
{
    Transform Follow_LHand, Follow_RHand, Follow_Head;
    Transform m_LHand, m_RHand, m_Head;

    float LerpSpeed = 1;
    bool canSync = false;

    // Update is called once per frame
    void Update()
    {
        if (canSync && (Follow_Head&&Follow_LHand&&Follow_RHand))
        {
            m_LHand.position = Vector3.Lerp(m_LHand.position, Follow_LHand.position, LerpSpeed * Time.deltaTime);
            m_RHand.position = Vector3.Lerp(m_RHand.position, Follow_RHand.position, LerpSpeed * Time.deltaTime);
            m_Head.position = Vector3.Lerp(m_Head.position, Follow_Head.position, LerpSpeed * Time.deltaTime);
        }
    }

    public void SetSync(bool active)
    {
        canSync = active;
    }

    public void SetSync(bool active, Transform lhand, Transform rhand, Transform head)
    {
        canSync = active;
        Follow_Head = head;
        Follow_LHand = lhand;
        Follow_RHand = rhand;

    }
}

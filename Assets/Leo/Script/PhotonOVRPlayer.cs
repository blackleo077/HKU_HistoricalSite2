using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;


using Oculus.Avatar;
using Oculus.Platform;
using Oculus.Platform.Models;

public class PhotonOVRPlayer : MonoBehaviour
{
    PhotonView pview;
    OVRManager ovrManager;
    Camera cam;
    GameObject LocalAvatarPrefab;

    [SerializeField]
    OvrAvatar myAvatar;

    [SerializeField]
    string AvatarID;

    bool BodyisLoaded = false;
    bool BodyNetworkSyncExists = false;

    private void Awake()
    {
        Debug.LogError("OVRPlayer Init");
        InitGeneral();
        InitAvatar();
    }

    private void LateUpdate()
    {
        if (BodyisLoaded && !BodyNetworkSyncExists)
        {
            if (myAvatar.transform.childCount > 0)
            {
                AddNetworkSyncToLocalAvatar();
            }
        }
    }


    void InitAvatar()
    {
        if (!pview.IsMine)
        {
            Destroy(myAvatar.GetComponent<OvrAvatarLocalDriver>());
        }
        Debug.LogError("Is My avatar:"+ pview.IsMine);
        Core.Initialize();
        Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
        Request.RunCallbacks();
        
    }

    void AddNetworkSyncToLocalAvatar()
    {
        myAvatar.Body.gameObject.AddComponent<PhotonTransformView>();
        myAvatar.HandLeft.gameObject.AddComponent<PhotonTransformView>();
        myAvatar.HandRight.gameObject.AddComponent<PhotonTransformView>();
        myAvatar.ControllerLeft.gameObject.AddComponent<PhotonTransformView>();
        myAvatar.ControllerRight.gameObject.AddComponent<PhotonTransformView>();
        BodyNetworkSyncExists = true;

        Debug.LogError("Body Is Loaded on " + pview.ViewID);
        Debug.LogError("AvatarID: " + AvatarID);

    }

    void InitGeneral()
    {
        pview = GetComponent<PhotonView>();
        if (pview.IsMine)
        {
            AddOVRComponents();
            ActiveNecessaryComponents();
        }
        else
        {
            RemoveUnnecessaryComponents();
        }
    }
    private void GetLoggedInUserCallback(Message<User> message)
    {
        AvatarID = PhotonSpawnPlayer.instance.GetAvatarID();

        if (!message.IsError)
        {
            Debug.LogFormat("AvatarID ID {0} , OculusUserID: {1} , ID:{2}", myAvatar.oculusUserID, message.Data.OculusID, message.Data.ID);
            Debug.LogError("No Error");
            myAvatar.oculusUserID = message.Data.OculusID;
        }
        else
        {
            myAvatar.oculusUserID = AvatarID;
            Debug.LogErrorFormat(message.GetError().Message);
        }
        BodyisLoaded = true;
    }

    void ActiveNecessaryComponents()
    {
        MonoBehaviour[] components = GetComponentsInChildren<MonoBehaviour>();

        foreach (MonoBehaviour c in components)
        {
            c.enabled = true;

        }
    }

    void AddOVRComponents()
    {

        ovrManager = transform.GetChild(0).gameObject.AddComponent<OVRManager>();
        cam = ovrManager.transform.GetChild(0).GetChild(1).GetComponent<Camera>();
        cam.gameObject.AddComponent<OVRScreenFade>();
    }

    public void RemoveUnnecessaryComponents()
    {
        DestoryComponents();
        DestroyCameraInChildren();
        DestroyAudioListenerInChildren();
        DestroyPhysicsComponents();

    }
    private void DestoryComponents()
    {
        MonoBehaviour[] components = GetComponentsInChildren<MonoBehaviour>();

        foreach (MonoBehaviour c in components)
        {
            if (c is PhotonView || c is PhotonOVRPlayer || c is MonoBehaviourPun|| c is OvrAvatar || c is RemoteLoopbackManager)
            {
            }
            else
            {
                if (c is OVRManager)
                {
                    Debug.LogErrorFormat("Destroy OVRManager on {0}:{1}", c.name, c.GetInstanceID());
                }
                Destroy(c);
            }

        }
    }

    void DestroyPhysicsComponents()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rgb in rigidbodies)
        {
            Destroy(rgb);
        }
        foreach (Collider c in colliders)
        {
            Destroy(c);
        }
    }

    void DestroyCameraInChildren()
    {
        Camera[] cameras = GetComponentsInChildren<Camera>();
        foreach (Camera cam in cameras)
        {
            Destroy(cam);
        }
    }

    void DestroyAudioListenerInChildren()
    {
        AudioListener[] audioListeners = GetComponentsInChildren<AudioListener>();
        foreach (AudioListener aud in audioListeners)
        {
            Destroy(aud);
        }
    }
}

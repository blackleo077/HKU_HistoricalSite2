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
    OVRInputController inputcontroller;
    PhotonView pview;
    OVRManager ovrManager;
    Camera cam;


    [SerializeField]
    string AvatarID;

    bool BodyisLoaded = false;
    bool BodyNetworkSyncExists = false;

    FadeController fadeController;
    OVRCameraRig myCameraRig;
    [SerializeField]
    OvrAvatar myLocalAvatar;
    OVRInputController inputController;
    RemoteLoopbackManager avatarRemote;
    LocomotionController teleportController;
    BoundaryController boundaryController;

    public OvrAvatar LocalAvatarPrefab;


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
            if (myLocalAvatar.transform.childCount > 0)
            {
               // AddNetworkSyncToLocalAvatar();
            }
        }
    }

    #region



    #endregion

    void InitAvatar()
    {
        if (pview.IsMine)
        {

            OvrAvatar localavatar = GameObject.Instantiate(LocalAvatarPrefab);
            myLocalAvatar = localavatar;
            //avatarRemote = myLocalAvatar.gameObject.AddComponent<RemoteLoopbackManager>();
           // avatarRemote.LocalAvatar = myLocalAvatar;
           // avatarRemote.LoopbackAvatar = myRemoteAvatar;

            Core.Initialize();
            Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
            Request.RunCallbacks();

        }
    }

    void AddNetworkSyncToLocalAvatar()
    {
        myLocalAvatar.Body.gameObject.AddComponent<PhotonTransformView>();
        myLocalAvatar.HandLeft.gameObject.AddComponent<PhotonTransformView>();
        myLocalAvatar.HandRight.gameObject.AddComponent<PhotonTransformView>();
        myLocalAvatar.ControllerLeft.gameObject.AddComponent<PhotonTransformView>();
        myLocalAvatar.ControllerRight.gameObject.AddComponent<PhotonTransformView>();
        BodyNetworkSyncExists = true;

        Debug.LogError("Body Is Loaded on " + pview.ViewID);
        Debug.LogError("AvatarID: " + AvatarID);

    }

    void InitGeneral()
    {
        pview = GetComponent<PhotonView>();
        myCameraRig = GetComponentInChildren<OVRCameraRig>();
        teleportController = GetComponentInChildren<LocomotionController>();

        if (pview.IsMine)
        {
            ovrManager = myCameraRig.gameObject.AddComponent<OVRManager>();
            ovrManager.AllowRecenter = false;
            ovrManager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;

            cam = Camera.main.GetComponent<Camera>();
            cam.gameObject.AddComponent<OVRScreenFade>();

            
            fadeController = gameObject.AddComponent<FadeController>();
            fadeController.Register(teleportController.GetComponent<LocomotionTeleport>());

           // boundaryController = gameObject.AddComponent<BoundaryController>();
        }
        else
        {
            RemoveUnnecessaryComponents();
        }
    }
    private void GetLoggedInUserCallback(Message<User> message)
    {
        if (PhotonSpawnPlayer.instance)
        {
            AvatarID = PhotonSpawnPlayer.instance.GetAvatarID();
        }
        else
        {
            AvatarID = "10150030458762178";
        }
       // 

        if (!message.IsError)
        {
            Debug.LogFormat("AvatarID ID {0} , OculusUserID: {1} , ID:{2}", myLocalAvatar.oculusUserID, message.Data.OculusID, message.Data.ID);
            Debug.LogError("No Error");
            myLocalAvatar.oculusUserID = message.Data.OculusID;
        }
        else
        {
            myLocalAvatar.oculusUserID = AvatarID;
            Debug.LogErrorFormat(message.GetError().Message);
        }
        BodyisLoaded = true;
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

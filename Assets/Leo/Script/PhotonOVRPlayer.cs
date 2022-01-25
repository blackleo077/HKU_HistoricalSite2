using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;


using Oculus.Avatar;
using Oculus.Platform;
using Oculus.Platform.Models;

public class PhotonOVRPlayer : MonoBehaviourPun
{
    OVRManager ovrManager;
    PhotonGameManager GM;


    [SerializeField]
    string AvatarID;

    bool BodyisLoaded = false;
    bool BodyNetworkSyncExists = false;

    [SerializeField] FadeController fadeController;
    [SerializeField] OVRCameraRig myCameraRig;
    [SerializeField] Camera mycamera;
    [SerializeField] OvrAvatar myLocalAvatar;
    [SerializeField] OVRInputController inputController;
    [SerializeField] BoundaryController boundaryController;
    [SerializeField] TeleportController teleportController;
    [SerializeField] LocomotionController locomotionController;
    [SerializeField] LocomotionTeleport locomotionTeleport;
    [SerializeField] SimpleCapsuleWithStickMovement stickmovement;

    [SerializeField] OvrAvatar LocalAvatarPrefab;

    GameObject Robot;
    OvrAvatarComponent[] SyncRobotComponent;

    private void Awake()
    {
        Debug.LogError("OVRPlayer Init");
        if (PhotonNetwork.IsConnected)
        {
            GM = GameObject.FindObjectOfType<PhotonGameManager>();
            GM.GetSpawnPlayer().AddOVRPlayer(this);
            Debug.LogError("GM "+ GM);
        }

        if (photonView.IsMine)
        {
            Debug.LogError("Add OVRManager");
            ovrManager = myCameraRig.gameObject.AddComponent<OVRManager>(); // singleton
            ovrManager.AllowRecenter = false;
            ovrManager.trackingOriginType = OVRManager.TrackingOrigin.FloorLevel;
        }
        else
        {
            RemoveUnnecessaryComponents();
        }
        //InitAvatar();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            Debug.LogError("Keep camera enable");
            mycamera.enabled = true;
            mycamera.gameObject.AddComponent<OVRScreenFade>();  // singleton


            fadeController = gameObject.AddComponent<FadeController>();
            fadeController.Register(GetComponentInChildren<LocomotionTeleport>());
        }
        else
        {
            Debug.LogError("disable camera ");
            mycamera.enabled = false;
        }

    }

    private void LateUpdate()
    {
        if (BodyisLoaded && !BodyNetworkSyncExists)
        {
            if (myLocalAvatar.transform.childCount > 0)
            {
                AddNetworkSyncToLocalAvatar();
            }
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
         
        }
    }

    #region



    #endregion

    void InitAvatar()
    {
        if (photonView.IsMine)
        {
            OvrAvatar localavatar = GameObject.Instantiate(LocalAvatarPrefab);
            myLocalAvatar = localavatar;

            Core.Initialize();
            Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
            Request.RunCallbacks();

        }
    }

    void AddNetworkSyncToLocalAvatar()
    {

        myLocalAvatar.gameObject.AddComponent<SyncOVRAvatar>();
        CloneDestroyScript();

        //myLocalAvatar.Body.gameObject.AddComponent<PhotonTransformView>();
        //myLocalAvatar.HandLeft.gameObject.AddComponent<PhotonTransformView>();
        //myLocalAvatar.HandRight.gameObject.AddComponent<PhotonTransformView>();
        //myLocalAvatar.ControllerLeft.gameObject.AddComponent<PhotonTransformView>();
        //myLocalAvatar.ControllerRight.gameObject.AddComponent<PhotonTransformView>();
        BodyNetworkSyncExists = true;

        Debug.LogError("Body Is Loaded on " + photonView.ViewID);
        Debug.LogError("AvatarID: " + AvatarID);

    }

    void CloneDestroyScript()
    {
        Robot = new GameObject("OVRRobot");
        SyncRobotComponent = Robot.GetComponentsInChildren<OvrAvatarComponent>();
        foreach(OvrAvatarComponent comp in SyncRobotComponent)
        {
           // comp.RenderParts
        }

    }



    #region  CallBack Method

    private void GetLoggedInUserCallback(Message<User> message)
    {
        if (GM.GetSpawnPlayer())
        {
            AvatarID = GM.GetAvatarID() ;
        }
        else
        {
            AvatarID = "10150030458762178";
        }

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
    #endregion


    #region Public method

    public void RemoveUnnecessaryComponents()
    {
        Debug.LogError("Remove unnesseary");
        Destroy(myCameraRig);
        DestroyAudioListenerInChildren();
        DestroyPhysicsComponents();
        Destroy(locomotionController.gameObject);
        Destroy(stickmovement);
        Destroy(boundaryController);

    }

    public void RecallPlayers()
    {

        Vector3 targetPos = transform.position;
        GM.MoveToMaster(targetPos);
        /*
        List<PhotonOVRPlayer> players = GM.GetSpawnPlayer().GetPhotonOVRPlayer();
        Debug.LogErrorFormat("Exist players: {0}",players.Count);
        foreach (PhotonOVRPlayer p in players)
        {
            Debug.LogError(p.photonView.ViewID+"Manual call rpc");
            //p.TeleportTo(targetPos);
        }

        Debug.LogError("Photon network players : " +PhotonNetwork.PlayerList.Length);
        
        photonView.RPC("TeleportTo", RpcTarget.All,targetPos);
        */
    }

    public Camera GetCamera()
    {
        return mycamera;
    }

    public OVRCameraRig GetOVRRig()
    {
        return myCameraRig;
    }

    public OVRManager GetOVRManager()
    {
        return ovrManager;
    }

    public void testRPC(string msg)
    {
        if (photonView.IsMine)
        {
            Debug.LogError(photonView.ViewID + "receive rpc call " + msg);
        }
        else
        {
            Debug.LogError(photonView.ViewID + " Not my object" + msg);
        }
    }

    public void TeleportTo(Vector3 tplocation)
    {
        if (photonView.IsMine)
        {
            Debug.LogError("Mine Object"+photonView);
            Debug.LogError(photonView.ViewID + " receive RPC Client Call Teleport to " + tplocation);
            StartCoroutine(SmoothTeleport(tplocation));
        }
        else
        {

            Debug.LogError("Not My Object" + photonView);
        }
    }
    IEnumerator SmoothTeleport(Vector3 location)
    {
        if(fadeController)
            fadeController.TPStart();
        yield return new WaitForSeconds(0.5f);
        teleportController.Teleport(location);
        if(fadeController)
            fadeController.TPEnd();

        Debug.LogErrorFormat("Player {0} tp complete", photonView.ViewID);
    }
    #endregion

    #region destroy method 
    private void DestoryComponents()
    {
        MonoBehaviour[] components = GetComponentsInChildren<MonoBehaviour>();

        foreach (MonoBehaviour c in components)
        {
            if (c is PhotonView || c is PhotonOVRPlayer || c is MonoBehaviourPun|| c is FadeController || c is TeleportController)
            {
            }
            else
            {
                Debug.LogError("Destroy "+c);
                Destroy(c);
            }

        }
    }

    private void DestroyPhysicsComponents()
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

    private void DestroyCameraInChildren()
    {
        Camera[] cameras = GetComponentsInChildren<Camera>();
        Debug.LogError("Destroy cameras");
        foreach (Camera cam in cameras)
        {
            Destroy(cam);
        }
    }

    private void DestroyAudioListenerInChildren()
    {
        AudioListener[] audioListeners = GetComponentsInChildren<AudioListener>();
        foreach (AudioListener aud in audioListeners)
        {
            Destroy(aud);
        }
    }


    private void OnDestroy()
    {

    }

    #endregion
}

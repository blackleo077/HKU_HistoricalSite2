using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using Oculus.Avatar2;
using Oculus.Platform;
using CAPI = Oculus.Avatar2.CAPI;
using Photon.Pun;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PhotonAvatarController : OvrAvatarEntity
{
    [Header("CS Variable")]
    [SerializeField] bool IsLoadCustomAvatar = true;
    [SerializeField] bool ReloadCSAvatarOnFail = true;

    public enum AssetSource
    {
        Zip,
        StreamingAssets,
    }

    [System.Serializable]
    private struct AssetData
    {
        public AssetSource source;
        public string path;
    }

    [Header("Sample Avatar Entity")]
    [SerializeField] private bool _loadUserFromCdn = true;


    [Header("Assets")]
    [Tooltip("Asset paths to load, and whether each asset comes from a preloaded zip file or directly from StreamingAssets")]
    [SerializeField] private List<AssetData> _assets = new List<AssetData> { new AssetData { source = AssetSource.Zip, path = "0" } };

#pragma warning disable CS0414
    [Tooltip("Asset suffix for non-Android platforms")]
    [SerializeField] private string _assetPostfixDefault = "_rift.glb";
    [Tooltip("Asset suffix for Android platforms")]
    [SerializeField] private string _assetPostfixAndroid = "_quest.glb";
#pragma warning restore CS0414


    [Header("CDN")]
    [Tooltip("Automatically retry LoadUser download request on failure")]
    [SerializeField] private bool _autoCdnRetry = true;

    [Tooltip("Automatically check for avatar changes")]
    [SerializeField] private bool _autoCheckChanges = false;
    [Tooltip("How frequently to check for avatar changes")]
    [SerializeField] [Range(4.0f, 320.0f)] private float _changeCheckInterval = 8.0f;

#pragma warning disable CS0414
    [Header("Debug Drawing")]
    [Tooltip("Draw debug visualizations for avatar gaze targets")]
    [SerializeField] private bool _debugDrawGazePos;
    [Tooltip("Color for gaze debug visualization")]
    [SerializeField] private Color _debugDrawGazePosColor = Color.magenta;
#pragma warning restore CS0414

    private enum OverrideStreamLOD
    {
        Default,
        ForceHigh,
        ForceMedium,
        ForceLow,
    }

    [Header("Sample Networking")]
    [Tooltip("Streaming quality override, default will not override")]
    [SerializeField] private OverrideStreamLOD _overrideStreamLod = OverrideStreamLOD.Default;

    private static readonly int DESAT_AMOUNT_ID = Shader.PropertyToID("_DesatAmount");
    private static readonly int DESAT_TINT_ID = Shader.PropertyToID("_DesatTint");
    private static readonly int DESAT_LERP_ID = Shader.PropertyToID("_DesatLerp");
    private bool HasLocalAvatarConfigured => _assets.Count > 0;

    protected IEnumerator Start()
    {
        if (IsLoadCustomAvatar)
        {
            yield return LoadCustomAvatar();
            Debug.LogError("MY ID = "+_userId);
        }
        else
        {
            LoadLocalAvatar();
        }

        switch (_overrideStreamLod)
        {
            case OverrideStreamLOD.ForceHigh:
                ForceStreamLod(StreamLOD.High);
                break;
            case OverrideStreamLOD.ForceMedium:
                ForceStreamLod(StreamLOD.Medium);
                break;
            case OverrideStreamLOD.ForceLow:
                ForceStreamLod(StreamLOD.Low);
                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SetLocalAvatar();
        }
        if (Input.GetKeyDown(KeyCode.Space) && gameObject.name.Contains("Remote"))
        {
            SetRemoteAvatar(6703037566437409);
        }
    }

    public ulong GetAvatarID() { return _userId; }

    public void SetLocalAvatar()
    {
        StartCoroutine(LoadCustomAvatar());
    }
    public void SetRemoteAvatar(ulong uid)
    {
        _userId = uid;
        StartCoroutine(Retry_HasAvatarRequest());
    }


    #region Public OVRAvatarEntity method
    public Transform GetSkeletonTransform(CAPI.ovrAvatar2JointType jointType)
    {
        if (!_criticalJointTypes.Contains(jointType))
        {
            OvrAvatarLog.LogError($"Can't access joint {jointType} unless it is in critical joint set");
            return null;
        }

        return GetSkeletonTransformByType(jointType);
    }
    #endregion



    #region Private OVRAvatarEntity method

    private IEnumerator LoadCustomAvatar()
    {
        if (OvrPlatformInit.status == OvrPlatformInitStatus.NotStarted)
        {
            OvrPlatformInit.InitializeOvrPlatform();
        }

        while (OvrPlatformInit.status != OvrPlatformInitStatus.Succeeded)
        {
            if (OvrPlatformInit.status == OvrPlatformInitStatus.Failed)
            {
                Debug.LogError("Error initializing OvrPlatform. Falling back to local avatar");
                LoadLocalAvatar();
                yield break;
            }

            yield return null;
        }

        // Get User ID
        bool getUserIdComplete = false;
        Users.GetLoggedInUser().OnComplete(message =>
        {
            if (!message.IsError)
            {
                _userId = message.Data.ID;
                Debug.Log("Loaded User ID : "+_userId);
            }
            else
            {
                var e = message.GetError();
                Debug.LogError("Error loading CDN avatar: Falling back to local avatar");
            }

            getUserIdComplete = true;
        });

        while (!getUserIdComplete) { yield return null; }

        yield return LoadUserAvatar();
    }

    private IEnumerator LoadUserAvatar()
    {
        if (_userId == 0)
        {
            LoadLocalAvatar();
            yield break;
        }

        yield return Retry_HasAvatarRequest();
    }

    private IEnumerator Retry_HasAvatarRequest()
    {
        const float HAS_AVATAR_RETRY_WAIT_TIME = 4.0f;
        const int HAS_AVATAR_RETRY_ATTEMPTS = 12;

        int totalAttempts = ReloadCSAvatarOnFail ? HAS_AVATAR_RETRY_ATTEMPTS : 1;
        bool continueRetries = ReloadCSAvatarOnFail;
        int retriesRemaining = totalAttempts;
        bool hasFoundAvatar = false;
        bool requestComplete = false;
        do
        {
            var hasAvatarRequest = OvrAvatarManager.Instance.UserHasAvatarAsync(_userId);
            while (!hasAvatarRequest.IsCompleted) { yield return null; }

            switch (hasAvatarRequest.Result)
            {
                case OvrAvatarManager.HasAvatarRequestResultCode.HasAvatar:
                    hasFoundAvatar = true;
                    requestComplete = true;
                    continueRetries = false;

                    // Now attempt download
                    yield return AutoRetry_LoadUser(true);
                    // End coroutine - do not load default
                    break;

                case OvrAvatarManager.HasAvatarRequestResultCode.HasNoAvatar:
                    requestComplete = true;
                    continueRetries = false;

                    Debug.Log("User has no avatar. Falling back to local avatar.");
                    break;
                case OvrAvatarManager.HasAvatarRequestResultCode.SendFailed:
                    Debug.LogError("Unable to send avatar status request.");
                    break;
                case OvrAvatarManager.HasAvatarRequestResultCode.RequestFailed:
                    Debug.LogError("An error occurred while querying avatar status.");
                    break;
                case OvrAvatarManager.HasAvatarRequestResultCode.BadParameter:
                    continueRetries = false;

                    Debug.LogError("Attempted to load invalid userId.");
                    break;

                case OvrAvatarManager.HasAvatarRequestResultCode.UnknownError:
                default:
                    Debug.LogError("An unknown error occurred. Falling back to local avatar.");
                    break;
            }

            continueRetries &= --retriesRemaining > 0;
            if (continueRetries)
            {
                yield return new WaitForSecondsRealtime(HAS_AVATAR_RETRY_WAIT_TIME);
            }
        } while (continueRetries);

        if (!requestComplete)
        {
            Debug.LogError("Unable to query UserHasAvatar {totalAttempts} attempts");
        }

        if (!hasFoundAvatar)
        {
            // We cannot find an avatar, use local fallback
            UserHasNoAvatarFallback();
        }

        /*
        // Check for changes unless a local asset is configured, user could create one later
        // If a local asset is loaded, it will currently conflict w/ the CDN asset
        if (CheckCSAvatarChange && (hasFoundAvatar || !HasLocalAvatarConfigured))
        {
            yield return PollForAvatarChange();
        }
        */
    }

    private IEnumerator AutoRetry_LoadUser(bool loadFallbackOnFailure)
    {
        const float LOAD_USER_POLLING_INTERVAL = 4.0f;
        const float LOAD_USER_BACKOFF_FACTOR = 1.618033988f;
        const int CDN_RETRY_ATTEMPTS = 13;

        int totalAttempts = ReloadCSAvatarOnFail ? CDN_RETRY_ATTEMPTS : 1;
        int remainingAttempts = totalAttempts;
        bool didLoadAvatar = false;
        var currentPollingInterval = LOAD_USER_POLLING_INTERVAL;
        do
        {
            LoadUser();

            CAPI.ovrAvatar2Result status;
            do
            {
                // Wait for retry interval before taking any action
                yield return new WaitForSecondsRealtime(currentPollingInterval);

                //TODO: Cache status
                status = this.entityStatus;
                if (status.IsSuccess() || HasNonDefaultAvatar)
                {
                    didLoadAvatar = true;
                    // Finished downloading - no more retries
                    remainingAttempts = 0;

                    Debug.Log("Load user retry check found successful download, ending retry routine");
                    break;
                }

                currentPollingInterval *= LOAD_USER_BACKOFF_FACTOR;
            } while (status == CAPI.ovrAvatar2Result.Pending);
        } while (--remainingAttempts > 0);

        if (loadFallbackOnFailure && !didLoadAvatar)
        {
            Debug.Log("Unable to download user after {totalAttempts} retry attempts");

            // We cannot download an avatar, use local fallback
            UserHasNoAvatarFallback();
        }
    }

    private void UserHasNoAvatarFallback()
    {
        Debug.LogError("Unable to find user avatar. Falling back to local avatar.");

        LoadLocalAvatar();
    }

    private void LoadLocalAvatar()
    {
        if (!HasLocalAvatarConfigured)
        {
            Debug.LogError("No local avatar asset configured");
            return;
        }

        string assetPostfix = OvrAvatarManager.IsAndroidStandalone ? _assetPostfixAndroid : _assetPostfixDefault;

        // Zip asset paths are relative to the inside of the zip.
        // Zips can be loaded from the OvrAvatarManager at startup or by calling OvrAvatarManager.Instance.AddZipSource
        // Assets can also be loaded individually from Streaming assets
        var path = new string[1];
        foreach (var asset in _assets)
        {
            path[0] = asset.path + assetPostfix;
            switch (asset.source)
            {
                case AssetSource.Zip:
                    LoadAssetsFromZipSource(path);
                    break;
                case AssetSource.StreamingAssets:
                    LoadAssetsFromStreamingAssets(path);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    #endregion
    #region Fade/Desat

    private static readonly Color AVATAR_FADE_DEFAULT_COLOR = new Color(33 / 255f, 50 / 255f, 99 / 255f, 0f); // "#213263"
    private static readonly float AVATAR_FADE_DEFAULT_COLOR_BLEND = 0.7f; // "#213263"
    private static readonly float AVATAR_FADE_DEFAULT_GRAYSCALE_BLEND = 0;

    [Header("Rendering")]
    [SerializeField] [Range(0, 1)] private float shaderGrayToSolidColorBlend_ = AVATAR_FADE_DEFAULT_COLOR_BLEND;
    [SerializeField] [Range(0, 1)] private float shaderDesatBlend_ = AVATAR_FADE_DEFAULT_GRAYSCALE_BLEND;
    [SerializeField] private Color shaderSolidColor_ = AVATAR_FADE_DEFAULT_COLOR;

    public float ShaderGrayToSolidColorBlend
    {
        // Blends grayscale to solid color
        get => shaderGrayToSolidColorBlend_;
        set
        {
            if (Mathf.Approximately(value, shaderGrayToSolidColorBlend_))
            {
                shaderGrayToSolidColorBlend_ = value;
                UpdateMaterialsWithDesatModifiers();
            }
        }
    }

    public float ShaderDesatBlend
    {
        // Blends shader color to result of ShaderGrayToSolidColorBlend
        get => shaderDesatBlend_;
        set
        {
            if (Mathf.Approximately(value, shaderDesatBlend_))
            {
                shaderDesatBlend_ = value;
                UpdateMaterialsWithDesatModifiers();
            }
        }
    }

    public Color ShaderSolidColor
    {
        get => shaderSolidColor_;
        set
        {
            if (shaderSolidColor_ != value)
            {
                shaderSolidColor_ = value;
                UpdateMaterialsWithDesatModifiers();
            }
        }
    }

    public void SetShaderDesat(float desatBlend, float? grayToSolidBlend = null, Color? solidColor = null)
    {
        if (solidColor.HasValue)
        {
            shaderSolidColor_ = solidColor.Value;
        }
        if (grayToSolidBlend.HasValue)
        {
            shaderGrayToSolidColorBlend_ = grayToSolidBlend.Value;
        }
        shaderDesatBlend_ = desatBlend;
        UpdateMaterialsWithDesatModifiers();
    }

    private void UpdateMaterialsWithDesatModifiers()
    {
        // TODO: Migrate to `OvrAvatarMaterial` system
#pragma warning disable 618 // disable deprecated method call warnings
        SetMaterialKeyword("DESAT", shaderDesatBlend_ > 0.0f);
        SetMaterialProperties((block, entity) =>
        {
            block.SetFloat(DESAT_AMOUNT_ID, entity.shaderDesatBlend_);
            block.SetColor(DESAT_TINT_ID, entity.shaderSolidColor_);
            block.SetFloat(DESAT_LERP_ID, entity.shaderGrayToSolidColorBlend_);
        }, this);
#pragma warning restore 618 // restore deprecated method call warnings
    }

    #endregion

}

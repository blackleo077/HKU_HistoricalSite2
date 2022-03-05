using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    LocomotionTeleport locomotion;

    public void TPStart()
    {
        Debug.Log("Start Fade");
        OVRScreenFade.instance.fadeTime = 0.1f;
        OVRScreenFade.instance.FadeOut();
    }

    public void TPEnd()
    {
        OVRScreenFade.instance.fadeTime = 0.2f;
        OVRScreenFade.instance.FadeIn();
    }

    public void Register(LocomotionTeleport lc)
    {
        locomotion = lc;
        locomotion.EnterStatePreTeleport += TPStart;
        locomotion.EnterStatePostTeleport += TPEnd;
    }

    private void OnDestroy()
    {
        locomotion.EnterStatePreTeleport -= TPStart;
        locomotion.EnterStatePostTeleport -= TPEnd;
    }

}

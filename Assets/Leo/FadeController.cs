using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeController : MonoBehaviour
{
    [SerializeField]
    LocomotionTeleport locomotion;
    // Start is called before the first frame update
    private void OnEnable()
    {
        locomotion.EnterStatePreTeleport += TPStart;
        locomotion.EnterStatePostTeleport += TPEnd;
    }

    private void OnDisable()
    {
        locomotion.EnterStatePreTeleport -= TPStart;
        locomotion.EnterStatePostTeleport -= TPEnd;
    }
    public void TPStart()
    {
        OVRScreenFade.instance.fadeTime = 0.1f;
        OVRScreenFade.instance.FadeOut();
    }

    public void TPEnd()
    {
        OVRScreenFade.instance.fadeTime = 0.2f;
        OVRScreenFade.instance.FadeIn();
    }
}

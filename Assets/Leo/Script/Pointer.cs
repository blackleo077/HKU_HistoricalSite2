using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof( LineRenderer))]
public class Pointer : Tools
{
    [SerializeField]
    LayerMask ArtifactLayer;

    float LaserWidth = 0.01f;
    LineRenderer lr;
    Artifacts targetArtifact;

    protected override void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.SetWidth(LaserWidth, LaserWidth);
        setLaserBrightness(0.2f);
    }

    protected override void OnToolTrigger()
    {
        lr.enabled = true;
        base.OnToolTrigger();
    }

    protected override void OnToolActiviting()
    {
        base.OnToolActiviting();
    }

    protected override void OnToolRelease()
    {
        if (targetArtifact)
        {
            targetArtifact.ShowInfoBoard(false);
            targetArtifact = null;
        }
        lr.enabled = false;
        base.OnToolRelease();
    }

    private void Update()
    {
        if (isActivating)
        {
            ActiveLaser();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }



    void ActiveLaser()
    {
        RaycastHit[] hit;

        hit = Physics.RaycastAll(transform.position, transform.forward, 200f, ArtifactLayer);
        if (hit.Length > 0)
        {
            setLaser(transform.position, hit[0].collider.transform.position, Color.green);
            hit[0].collider.TryGetComponent<Artifacts>(out Artifacts currentArtifact);
            if (targetArtifact != currentArtifact)
            {
                //update new
                if(targetArtifact)
                    targetArtifact.ShowInfoBoard(false);
                Debug.Log("active board");
                currentArtifact.ShowInfoBoard(true);
                targetArtifact = currentArtifact;
            }
        }
        else
        {
            if (targetArtifact)
            {
                targetArtifact.ShowInfoBoard(false);
                targetArtifact = null;
            }
            setLaser(transform.position, transform.position+(transform.forward*200f), Color.red);
        }
    }


    void setLaser(Vector3 start, Vector3 end, Color c)
    {
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.material.color = c;
    }

    public void setLaserWidth(float width)
    {
        lr.SetWidth(width, width);
    }

    public void setLaserBrightness(float brightness)
    {
        lr.material.SetFloat("_DistortionBlend", brightness);
        
    }
}

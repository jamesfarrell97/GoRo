using System.Collections;
using System.Collections.Generic;

using UnityEngine.XR.Interaction.Toolkit;
using Photon.Pun;

public class XRGrabNetworkInteractible : XRGrabInteractable
{
    private PhotonView photonView;

    protected override void OnSelectEnter(XRBaseInteractor interactor)
    {
        photonView.RequestOwnership();
        base.OnSelectEnter(interactor);
    }
}

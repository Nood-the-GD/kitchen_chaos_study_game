using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Game;
namespace Photon.Pun.UtilityScripts{
    public class PlayerControlSync : Photon.Pun.MonoBehaviourPun, IPunObservable
    {
        public float SmoothingMoveDelay = 5;
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                //We own this player: send the others our data
                Debug.Log("is writing");
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                Debug.Log("is reading");
                //Network player, receive data
                correctPlayerPos = (Vector3)stream.ReceiveNext();
                correctPlayerRot = (Quaternion)stream.ReceiveNext();
            }
        }

        private Vector3 correctPlayerPos = Vector3.zero; //We lerp towards this
        private Quaternion correctPlayerRot = Quaternion.identity; //We lerp towards this

        public void Update()
        {
            if (!photonView.IsMine)
            {
                //Update remote player (smooth this, this looks good, at the cost of some accuracy)
                transform.position = Vector3.Lerp(transform.position, correctPlayerPos, Time.deltaTime * this.SmoothingMoveDelay);
                transform.rotation = Quaternion.Lerp(transform.rotation, correctPlayerRot, Time.deltaTime * this.SmoothingMoveDelay);
            }
        }
    }
}
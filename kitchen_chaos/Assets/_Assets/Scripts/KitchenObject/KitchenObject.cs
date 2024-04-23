using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System.Linq;
public class KitchenObject : MonoBehaviour
{
    [SerializeField] private KitchenObjectSO kitchenObjectSO;

    private IKitchenObjectParent kitchenObjectParent;
    PhotonView photonView;
    public static void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        // Transform kitchenObjectTransform = kitchenObjectSO.prefab.GetComponent<ObjectTypeView>().objectType.SpawnMultiplay().transform;
        // kitchenObjectTransform.GetComponent<KitchenObject>().SetKitchenObjectParent(kitchenObjectParent);
        var parentId = kitchenObjectParent.GetKitchenObject().GetComponent<PhotonView>().ViewID;
        PhotonManager.s.CmdSpawnKitchenObject(kitchenObjectSO.prefab.GetComponent<ObjectTypeView>().objectType, parentId);
    }
    

    void Start(){
        photonView = GetComponent<PhotonView>();
        StartCoroutine(OnSync());
    }

    [PunRPC]
    public void RpcSetParentWithPhotonId(int photonId){
        var kitchenObjectParent = FindObjectsByType<PhotonView>(FindObjectsSortMode.None).ToList().Find(x => x.ViewID == photonId).GetComponent<IKitchenObjectParent>();
        SetKitchenObjectParent(kitchenObjectParent);
    }



    IEnumerator OnSync(){
        if(!PhotonNetwork.IsMasterClient)
            yield break;
        yield return new WaitForSeconds(1f);

        var parentId = transform.parent.GetComponent<PhotonView>().ViewID;
        photonView.RPC("RpcSetParentWithPhotonId", RpcTarget.All, parentId);
    }



    public bool TryGetPlateKitchenObject(out PlateKitchenObject plateKitchenObject)
    {
        if(this is PlateKitchenObject)
        {
            plateKitchenObject = this as PlateKitchenObject;
            return true;
        }
        else
        {
            plateKitchenObject = null;
            return false;
        }
    }

    public KitchenObjectSO GetKitchenObjectSO()
    {
        return kitchenObjectSO;
    }

    public void SetKitchenObjectParent(IKitchenObjectParent kitchenObjectParent)
    {

        if(this.kitchenObjectParent != null)
        {
            this.kitchenObjectParent.ClearKitchenObject();
        }
        this.kitchenObjectParent = kitchenObjectParent;
        
        if(kitchenObjectParent.HasKitchenObject()) 
        {
            Debug.LogError("clear kitchenObjectParent already had kitchenObject");
        }

        kitchenObjectParent.SetKitchenObject(this);

        this.transform.parent = kitchenObjectParent.GetKitchenObjectFollowTransform();
        this.transform.localPosition = Vector3.zero;
    }

    public IKitchenObjectParent GetKitchenObjectParent()
    {
        return kitchenObjectParent;
    }

    public void DestroySelf()
    {
        kitchenObjectParent.ClearKitchenObject();

        Destroy(this.gameObject);
    }
}

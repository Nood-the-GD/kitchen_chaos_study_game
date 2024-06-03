using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    #region Const
    const string CUSTOMER_FOLDER_PATH = "Prefabs/Customers";
    #endregion

    #region Variables
    private List<Customer> _customerList = new List<Customer>();
    private PhotonView _photonView;
    #endregion

    #region Unity functions
    void Awake()
    {
        _customerList = Resources.LoadAll<Customer>(CUSTOMER_FOLDER_PATH).ToList();
        _photonView = GetComponent<PhotonView>();
    }
    void OnEnable()
    {
        if(PhotonNetwork.IsMasterClient)
            DeliveryManager.Instance.OnRecipeAdded += SpawnCustomer;
    }
    void OnDisable()
    {
        if(PhotonNetwork.IsMasterClient)
            DeliveryManager.Instance.OnRecipeAdded -= SpawnCustomer;
    }
    #endregion

    #region Support Functions
    private void SpawnCustomer(object sender, EventArgs e)
    {
        CmdSpawnCustomer();
    }
    #endregion

    #region Multiplay
    private void CmdSpawnCustomer()
    {
        int index = UnityEngine.Random.Range(0, _customerList.Count - 1);
        _photonView.RPC(nameof(RpcSpawnCustomer), RpcTarget.All, index);
    }
    [PunRPC]
    private void RpcSpawnCustomer(int id)
    {
        Customer customer = GameData.s.GetCustomer(id);
        Customer spawnCustomer = Instantiate(customer, transform.position, this.transform.rotation);
        spawnCustomer.transform.position = this.transform.position;
    }
    #endregion
}

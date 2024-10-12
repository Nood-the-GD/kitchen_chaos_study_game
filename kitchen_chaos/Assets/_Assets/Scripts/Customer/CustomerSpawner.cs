using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;

public class CustomerSpawner : Singleton<CustomerSpawner>
{
    #region Const
    const string CUSTOMER_FOLDER_PATH = "Prefabs/Customers";
    #endregion

    #region Variables
    private List<Customer> _customerList = new List<Customer>();
    private PhotonView _photonView;
    #endregion

    #region Unity functions
    protected override void Awake()
    {
        base.Awake();
        _customerList = Resources.LoadAll<Customer>(CUSTOMER_FOLDER_PATH).ToList();
        // _photonView = GetComponent<PhotonView>();
    }
    protected override void Start()
    {
        base.Start();
        // if (PhotonNetwork.IsMasterClient)
        //     DeliveryManager.Instance.OnRecipeAdded += SpawnCustomer;
    }
    void OnDisable()
    {
        // if (PhotonNetwork.IsMasterClient)
        //     DeliveryManager.Instance.OnRecipeAdded -= SpawnCustomer;
    }
    #endregion

    [Button]
    public void Spawn()
    {
        int index = UnityEngine.Random.Range(0, _customerList.Count - 1);
        int numberOfPeople = UnityEngine.Random.Range(1, 4);
        RpcSpawnCustomerGroup(index, numberOfPeople);
    }

    #region Support Functions
    private void SpawnCustomer(object sender, EventArgs e)
    {
        int index = UnityEngine.Random.Range(0, _customerList.Count - 1);
        int numberOfPeople = UnityEngine.Random.Range(1, 4);
        RpcSpawnCustomerGroup(index, numberOfPeople);
        // CmdSpawnCustomer();
    }
    #endregion

    #region Multiplay
    private void CmdSpawnCustomer()
    {
        int index = UnityEngine.Random.Range(0, _customerList.Count - 1);
        _photonView.RPC(nameof(RpcSpawnCustomerGroup), RpcTarget.All, index);
    }
    [PunRPC]
    private void RpcSpawnCustomerGroup(int id, int numberOfPeople)
    {
        Debug.Log("SpawnCustomer");
        CustomerGroup customerGroup = new CustomerGroup();
        for (int i = 0; i < numberOfPeople; i++)
        {
            Customer customer = GameData.s.GetCustomer(id);
            Customer spawnCustomer = Instantiate(customer, transform.position, this.transform.rotation);
            customerGroup.AddToList(spawnCustomer);
            spawnCustomer.transform.position = this.transform.position;
        }
        WaitingLine.s.AddCustomerGroup(customerGroup);
    }
    #endregion
}













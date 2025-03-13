using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;

public class CustomerSpawner : Singleton<CustomerSpawner>
{
    #region Const
    const string CUSTOMER_FOLDER_PATH = "Prefabs/Customers";
    #endregion

    #region Variables
    private List<Customer> _customerPrefabList = new List<Customer>();
    private List<CustomerOld> _customerOldList = new List<CustomerOld>();
    private PhotonView _photonView;
    public int MaxCustomerSpawn = 5;
    private Vector3 SpawnPos => this.transform.position + new Vector3(0, 0, 15f);
    #endregion

    #region Unity functions
    protected override void Awake()
    {
        base.Awake();
        _customerPrefabList = Resources.LoadAll<Customer>(CUSTOMER_FOLDER_PATH).ToList();
        _photonView = GetComponent<PhotonView>();
    }
    protected override void Start()
    {
        base.Start();
        if (UserData.IsFirstTutorialDone == false) return;
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnLoop();
            // if (PhotonNetwork.IsMasterClient)
            //     DeliveryManager.Instance.OnRecipeAdded += SpawnCustomer;
        }
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
        CmdSpawnCustomer();
    }

    private async void SpawnLoop()
    {
        while (true)
        {
            CustomerOld lastCustomerOld = _customerOldList.Count > 0 ? _customerOldList.Last() : null;
            var isLastCustomerOldInSpawnPos = lastCustomerOld != null && lastCustomerOld.transform.position.z >= SpawnPos.z - 3;
            if (_customerOldList.Count < MaxCustomerSpawn && !isLastCustomerOldInSpawnPos)
            {
                CmdSpawnCustomer();
            }
            await UniTask.WaitForSeconds(5f);
        }
    }

    #region Support Functions
    private void SpawnCustomer(object sender, EventArgs e)
    {
        CmdSpawnCustomer();
    }
    public bool IsTopList(CustomerOld customerOld)
    {
        return _customerOldList.IndexOf(customerOld) == 0;
    }
    public void RemoveCustomerOld(CustomerOld customerOld)
    {
        _customerOldList.Remove(customerOld);
    }
    #endregion

    #region Multiplay
    private void CmdSpawnCustomer()
    {
        int index = UnityEngine.Random.Range(0, _customerPrefabList.Count - 1);
        var viewId = PhotonNetwork.AllocateViewID(false);
        // _photonView.RPC(nameof(RpcSpawnCustomerGroup), RpcTarget.All, new object[] { index, 1 });
        _photonView.RPC(nameof(RpcSpawnCustomerOld), RpcTarget.All, index, viewId);
    }
    [PunRPC]
    private void RpcSpawnCustomerGroup(int id, int numberOfPeople)
    {
        Debug.Log("SpawnCustomer");
        CustomerGroup customerGroup = new CustomerGroup(numberOfPeople);
        for (int i = 0; i < numberOfPeople; i++)
        {
            Customer customer = GameData.s.GetCustomer(id);
            Customer spawnCustomer = Instantiate(customer, transform.position, this.transform.rotation);
            // customerGroup.Customers[i] = spawnCustomer;
            spawnCustomer.transform.position = SpawnPos;
            CustomerManager.s.AddCustomer(spawnCustomer);
        }
        WaitingLine.s.AddCustomerGroup(customerGroup);
    }
    [PunRPC]
    private void RpcSpawnCustomerOld(int id, int viewId)
    {
        GameObject customerOld = Instantiate(GameData.s.GetCustomer(id).gameObject, SpawnPos, this.transform.rotation);
        CustomerOld customerOldComponent = customerOld.GetComponent<CustomerOld>();
        customerOldComponent.Move(this.transform.position);
        customerOldComponent.PhotonView.ViewID = viewId;
        _customerOldList.Add(customerOldComponent);
    }
    #endregion
}













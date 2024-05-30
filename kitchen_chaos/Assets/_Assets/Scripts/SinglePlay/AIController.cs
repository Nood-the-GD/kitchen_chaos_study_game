using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public enum BotStage
{
    Move,
    Interact,
    AltInteract,
    GetOrder,
    FindNextAction
}

public class AIController : MonoBehaviour, IKitchenObjectParent
{
    [SerializeField] private Transform _objectHoldingPoint;
    [SerializeField] private float _interactDistance = 1f;
    private NavMeshAgent _navMeshAgent;
    private BotStage _stage;
    private BaseCounter _selectedCounter;
    private RecipeSO _currentRecipeSO;
    private KitchenObject _currentKitchenObject;
    private Queue<AIAction> _aIActionQueue = new Queue<AIAction>();
    private bool _startAltInteract;

    public PhotonView photonView => null;
    public BotStage botStage => _stage;
    private Dictionary<KitchenObjectSO, BaseCounter> _counterKitchenObjectDictionary = new Dictionary<KitchenObjectSO, BaseCounter>();

    void Start()
    {
        _stage = BotStage.GetOrder;
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }
    void Update()
    {
        switch (_stage)
        {
            case BotStage.Move:
                Move();
                break;
            case BotStage.Interact:
                Interact();
                break;
            case BotStage.AltInteract:
                AltInteract();
                break;
            case BotStage.GetOrder:
                GetOrder();
                break;
            case BotStage.FindNextAction:
                FindNextAction();
                break;
            default:
                break;
        }        
    }

    #region Actions
    private void Move()
    {
        // Set move target and check move
        // If move to the target, play next action
        // If not, set the target to AI
    }
    private void Interact()
    {
        // Interact with counter
        _selectedCounter.Interact(this);
        if(_selectedCounter is CuttingCounter)
        {
            _startAltInteract = true;
            _stage = BotStage.AltInteract;
        }
    }    
    private void AltInteract()
    {
        // Alt interact with counter (chop)
        if(_selectedCounter is CuttingCounter)
        {
            CuttingCounter cuttingCounter = _selectedCounter as CuttingCounter;
            if(cuttingCounter.isComplete == false)
            {
                _selectedCounter.Chop(this);
            }
            else
            {
                cuttingCounter.Interact(this);
            }
        }
    }
    private void GetOrder()
    {
        // Get the next order
        GetNewOrder();
        FindNextAction();
    }
    private void FindNextAction()
    {
        if(_currentRecipeSO == null)
        {
            _stage = BotStage.GetOrder;
        }
        else
        {
            if(HasKitchenObject())
            {
                // Check if kitchen object is need chop or fry
                if(CheckChopOrFry())
                {
                    // If yes, get to the counter
                    SetTargetPosition(_selectedCounter.transform.position);
                    _stage = BotStage.Move;
                }
                else
                {
                    // If not, check all ingredient
                    // if have all ingredient, begin combine 
                    // else prepare next ingredient
                    if(FindAllOtherKitchenObject(out KitchenObjectSO lackOfKitchenObject) == false)
                    {
                        if(HasKitchenObject())
                        {
                            AICounterManager.s.TryGetEmptyClearCounter(out _selectedCounter);
                            SetTargetPosition(_selectedCounter.transform.position);
                            _stage = BotStage.Move;
                            return;
                        }
                        else
                        {
                            GetKitchenObject(lackOfKitchenObject);
                            SetTargetPosition(_selectedCounter.transform.position);
                            _stage = BotStage.Move;
                        }
                    }
                    else
                    {
                        // Begin combine
                        Combine();
                    }
                }
            }
            else
            {
                if (_selectedCounter == null)
                {
                    // Check if all kitchen object is done
                    if(FindAllOtherKitchenObject(out KitchenObjectSO lackOfKitchenObject) == false)
                    {
                        if(AICounterManager.s.TryGetCounterHasKitchenObject(lackOfKitchenObject, out BaseCounter resultCounter))
                        {
                            SetSelectedCounter(resultCounter);
                            SetTargetPosition(resultCounter.transform.position);
                            _stage = BotStage.Move;
                        }
                    }
                    else
                    {
                        // Begin combine
                        Combine();
                    }
                }
                else
                {
                    // Check if reach at selected counter
                    // If counter is empty, get kitchen object
                    // If counter is not empty, try to alt interact it 
                    // If it is fry and it is frying, wait for it done and get kitchen object
                    if(IsReachTargetPosition())
                    {
                        if(_selectedCounter is CuttingCounter && _selectedCounter.HasKitchenObject() == true)
                        {
                            // Check if kitchen object can be chopped
                            _stage = BotStage.AltInteract;
                        }
                        else if (_selectedCounter is StoveCounter && _selectedCounter.HasKitchenObject() == true)
                        {
                            // Check if kitchen object is cooked
                            _stage = BotStage.Interact;
                        }
                    }
                    else
                    {
                        _stage = BotStage.Move;
                    }
                }
            }
        }
    }
    #endregion

    #region Small Action
    private void Combine()
    {
        BaseCounter targetCounter = _counterKitchenObjectDictionary[_counterKitchenObjectDictionary.Keys.ElementAt(0)];
        SetTargetPosition(targetCounter.transform.position);
        _stage = BotStage.Move;
    }
    #endregion

    #region Support
    private void GetKitchenObject(KitchenObjectSO kitchenObjectSO)
    {
        KitchenObjectSO original = GetKitchenObjectOriginal(kitchenObjectSO);
        AICounterManager.s.TryGetCounterHasKitchenObject(original, out BaseCounter resultCounter); 
        SetSelectedCounter(resultCounter);
    }
    private bool FindAllOtherKitchenObject(out KitchenObjectSO lackOfKitchenObject)
    {
        lackOfKitchenObject = null;
        _counterKitchenObjectDictionary.Clear();
        List<KitchenObjectSO> checkList = new List<KitchenObjectSO>();
        checkList = _currentRecipeSO.kitchenObjectSOList;
        
        if(HasKitchenObject())
        {
            if(HasKitchenObject() && _currentKitchenObject is CompleteDishKitchenObject)
            {
                CompleteDishKitchenObject completeDishKitchenObject = _currentKitchenObject as CompleteDishKitchenObject;
                foreach(var kitchenObjectSO in completeDishKitchenObject.GetKitchenObjectSOList())
                    checkList.Remove(kitchenObjectSO);
            }
            else
            {
                checkList.Remove(_currentKitchenObject.GetKitchenObjectSO());
            }
        }

        foreach(KitchenObjectSO kitchenObjectSO in checkList)
        {
            AICounterManager.s.TryGetCounterHasKitchenObject(kitchenObjectSO, out BaseCounter resultCounter);
            if (HasKitchenObject())
            {
                if (_counterKitchenObjectDictionary.ContainsKey(kitchenObjectSO) == false)
                    _counterKitchenObjectDictionary.Add(kitchenObjectSO, resultCounter);
                else
                    _counterKitchenObjectDictionary[kitchenObjectSO] = resultCounter;
            }
            else
                lackOfKitchenObject = kitchenObjectSO;
                return false;
        }
        return true;
    }
    private bool CheckChopOrFry()
    {
        if(GetKitchenObject().GetKitchenObjectSO().kitchenObjectType == KitchenObjectType.NeedChop)
        {
            // Find cut counter and choose it
            if(AICounterManager.s.TryGetEmptyCuttingCounter(out CuttingCounter resultCounter))
                SetSelectedCounter(resultCounter);

            return true;
        }
        else if(GetKitchenObject().GetKitchenObjectSO().kitchenObjectType == KitchenObjectType.NeedFried)
        {
            // Find fry counter and choose it
            if(AICounterManager.s.TryGetEmptyStoveCounter(out StoveCounter resultCounter))
                SetSelectedCounter(resultCounter);

            return true;
        }
        return false;
    }
    private void GetNewOrder()
    {
        _currentRecipeSO = DeliveryManager.Instance.GetWaitingRecipeSOList()[0];
    }
    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        _selectedCounter = selectedCounter;
    }
    private void SetTargetPosition(Vector3 position)
    {
        if(NavMesh.SamplePosition(position, out NavMeshHit hit, 100f, NavMesh.AllAreas))
        {
            _navMeshAgent.SetDestination(hit.position);
        }
    }
    private KitchenObjectSO GetKitchenObjectOriginal(KitchenObjectSO kitchenObjectSO)
    {
        if(kitchenObjectSO.kitchenObjectType == KitchenObjectType.NeedChop)
        {
            return AIKitchenObjectManager.s.GetKitchenObjectSoOriginal(kitchenObjectSO);
        }
        return null;
    }
    private bool IsReachTargetPosition()
    {
        if (Vector3.Distance(this.transform.position, _selectedCounter.transform.position) <= _interactDistance)
            return true;
        else return false;
    }
    public void SetRecipeSO(RecipeSO recipeSO)
    {
        _currentRecipeSO = recipeSO;
    }
    #endregion



    #region Interface functions
    public Transform GetKitchenObjectFollowTransform()
    {
        return _objectHoldingPoint;
    }
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        _currentKitchenObject = kitchenObject;
    }
    public KitchenObject GetKitchenObject()
    {
        return _currentKitchenObject;
    }
    public void ClearKitchenObject()
    {
        _currentKitchenObject = null;
    }
    public bool HasKitchenObject()
    {
        return _currentKitchenObject != null;
    }
    #endregion
}

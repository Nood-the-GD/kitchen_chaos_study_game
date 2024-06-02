using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

#region Enum
public enum BotStage
{
    Move,
    Interact,
    AltInteract,
    GetOrder,
    FindNextAction
}
#endregion

public class AIController : MonoBehaviour, IPlayer
{
    #region Variables
    [SerializeField] private Transform _objectHoldingPoint;
    [SerializeField] private float _interactDistance = 1f;
    private NavMeshAgent _navMeshAgent;
    private BotStage _stage;
    private BaseCounter _selectedCounter;
    private RecipeSO _currentRecipeSO;
    private KitchenObject _currentKitchenObject;
    private float _altInteractDelay = 0.5f;
    private float _altInteractDelayTimer = 0;
    private PhotonView _photonView;

    public PhotonView photonView => _photonView;
    public BotStage botStage => _stage;
    #endregion

    #region Unity functions
    void Start()
    {
        _stage = BotStage.FindNextAction;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _photonView = GetComponent<PhotonView>();
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
    #endregion

    #region Actions
    private void Move()
    {
        // Set move target and check move
        // If move to the target, play next action
        // If not, set the target to AI
        if(IsReachTargetPosition() )
        {
            Debug.Log("Reach target pos");
            if (_selectedCounter != null)
                _stage = BotStage.Interact;
            else
                _stage = BotStage.FindNextAction;
        }
        else if(_selectedCounter != null)
        {
            SetTargetPosition(_selectedCounter.transform.position);
            _stage = BotStage.Move;
        }
        else
        {
            _stage = BotStage.FindNextAction;
        }
    }
    private void Interact()
    {
        // Interact with counter
        switch(_selectedCounter)
        {
            case StoveCounter stoveCounter:
                if(stoveCounter.HasKitchenObject() == true && stoveCounter.IsCookComplete() || stoveCounter.HasKitchenObject() == false)
                {
                    _selectedCounter.Interact(this);
                }
                break;
            default:
                if(_selectedCounter != null)
                    _selectedCounter.Interact(this);
                break;
        }
        _stage = BotStage.FindNextAction;
    }    
    private void AltInteract()
    {
        // Alt interact with counter (chop)
        if(_selectedCounter is IAltInteractable)
        {
            _altInteractDelayTimer += Time.deltaTime;
            if (_altInteractDelayTimer >= _altInteractDelay)
            {
                IAltInteractable altInteractableCounter = _selectedCounter as IAltInteractable;
                if (altInteractableCounter.CanAltInteract() == true)
                {
                    altInteractableCounter.AltInteract(this);
                    _altInteractDelayTimer = 0;
                }
                else
                {
                    FindNextAction();
                }
            }
        }
        else
        {
            _stage = BotStage.FindNextAction;
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
        if (DeliveryManager.Instance.GetWaitingRecipeSOList().Count == 0)
        {
            _stage = BotStage.FindNextAction;
            return;
        }

        if(_currentRecipeSO != DeliveryManager.Instance.GetWaitingRecipeSOList()[0])
        {
            _stage = BotStage.GetOrder;
        }
        else
        {
            Debug.Log("Check enough ingredients");
            bool isEnough = EnoughAllIngredients(out KitchenObjectSO lackOfKitchenObject);
            if(HasKitchenObject())
            {
                switch(_currentKitchenObject)
                {
                    case CompleteDishKitchenObject completeDishKitchenObject:
                        if(completeDishKitchenObject.IsHasPlate())
                        {
                            DeliverDish();
                        }
                        else
                        {
                            GetPlate();
                        }
                        break;
                    default:
                        if(CheckChopOrFry())
                        {
                            // Check if kitchen object is need chop or fry
                            // If yes, move to the counter
                            _stage = BotStage.Move;
                        }
                        else
                        {
                            // If not, check all ingredient
                            // if have all ingredient, begin combine 
                            // else prepare next ingredient
                            if(isEnough == false)
                            {
                                Debug.Log("Not enough ingredient");
                                // Put current kitchen object down and start making the lack kitchen object
                                PutDownKitchenObject();
                            }
                            else
                            {
                                Debug.Log("Combine");
                                // Begin combine
                                Combine();
                            }
                        }
                        break;
                }
            }
            else
            {
                // Don't has kitchen object
                if(IsReachTargetPosition())
                {
                    switch(_selectedCounter)
                    {
                        case IAltInteractable altInteractable:
                            // if this counter is Alt interactable
                            if(altInteractable.CanAltInteract())
                            {
                                _altInteractDelayTimer = 0;
                                _stage = BotStage.AltInteract;
                            }
                            else if(altInteractable.HasKitchenObject())
                            {
                                _stage = BotStage.Interact;
                            }
                            else
                            {
                                SetSelectedCounter(null);
                            }
                            break;
                        case StoveCounter stoveCounter:
                            // if this is StoveCounter => wait for this complete
                            if(stoveCounter.IsCookComplete())
                            {
                                _stage = BotStage.Interact;
                            }
                            else
                            {
                                _stage = BotStage.FindNextAction;
                            }
                            break;
                        default:
                            if(isEnough == false)
                            {
                                MakeKitchenObject(lackOfKitchenObject);
                            }    
                            else
                            {
                                Combine();
                            }
                            break;
                    }
                }
                else
                {
                    _stage = BotStage.Move;
                }
            }
        }
    }
    #endregion

    #region Small Action
    private void DeliverDish()
    {
        CompleteDishKitchenObject completeDishKitchenObject = _currentKitchenObject as CompleteDishKitchenObject;
        BaseCounter targetCounter = null;
        if(completeDishKitchenObject.IsCorrectRecipe(_currentRecipeSO))
        {
            targetCounter = AICounterManager.s.GetDeliveryCounter();
        }
        else
        {
            AICounterManager.s.TryGetEmptyClearCounter(out targetCounter);
        }
        SetSelectedCounter(targetCounter);
        _stage = BotStage.Move;
    }
    private void MakeKitchenObject(KitchenObjectSO kitchenObjectSO)
    {
        Debug.Log("make kitchen object: " + kitchenObjectSO);
        KitchenObjectSO original = AIKitchenObjectManager.s.GetKitchenObjectSoOriginal(kitchenObjectSO);
        Debug.Log("try get original: " + original);
        if(AICounterManager.s.TryGetCounterHasKitchenObject(original, out BaseCounter resultCounter))
        {
            SetSelectedCounter(resultCounter);
            _stage = BotStage.Move;
        }
    }
    private void PutDownKitchenObject()
    {
        if(AICounterManager.s.TryGetEmptyClearCounter(out BaseCounter resultCounter))
        {
            SetSelectedCounter(resultCounter);
            _stage = BotStage.Move;
        }
    }
    List<KitchenObjectSO> combineCheckList = new List<KitchenObjectSO>(); 
    private void Combine()
    {
        combineCheckList.Clear();
        // Prepare check list
        foreach(KitchenObjectSO ingredient in _currentRecipeSO.kitchenObjectSOList)
        {
            combineCheckList.Add(ingredient);
        }

        // Remove ingredients is holding
        if(HasKitchenObject())
        {
            switch(_currentKitchenObject)
            {
                case CompleteDishKitchenObject completeDishKitchenObject:
                    Debug.Log("Complete dish");
                    foreach(var kitchenObjectSO in completeDishKitchenObject.GetKitchenObjectSOList())
                    {
                        Debug.Log(kitchenObjectSO);
                        combineCheckList.Remove(kitchenObjectSO);
                    }
                    break;
                default:
                    Debug.Log("Default");
                    if(_currentKitchenObject != null)
                        combineCheckList.Remove(_currentKitchenObject.GetKitchenObjectSO());
                    break;
            }
        }

        if (combineCheckList.Count == 0)
        {
            GetPlate();
            return;
        }
        else
        {
            KitchenObjectSO nextKitchenObjectSO = combineCheckList[0];
            if(AICounterManager.s.TryGetCounterHasKitchenObject(nextKitchenObjectSO, out BaseCounter targetCounter))
            {
                SetSelectedCounter(targetCounter);
                SetTargetPosition(targetCounter.transform.position);
                _stage = BotStage.Move;
            }
            else
            {
                _stage = BotStage.FindNextAction;
            }
        }

    }
    #endregion

    #region Support
    private void GetPlate()
    {
        PlatesCounter platesCounter = AICounterManager.s.GetPlatesCounter();
        SetSelectedCounter(platesCounter);
        _stage = BotStage.Move;
    }
    private void GetKitchenObject(KitchenObjectSO kitchenObjectSO)
    {
        KitchenObjectSO original = GetKitchenObjectOriginal(kitchenObjectSO);
        AICounterManager.s.TryGetCounterHasKitchenObject(original, out BaseCounter resultCounter); 
        SetSelectedCounter(resultCounter);
    }
    private bool EnoughAllIngredients(out KitchenObjectSO lackOfKitchenObject)
    {
        lackOfKitchenObject = null;
        List<KitchenObjectSO> checkList = new List<KitchenObjectSO>();
        // Prepare check list
        foreach(KitchenObjectSO ingredient in _currentRecipeSO.kitchenObjectSOList)
        {
            checkList.Add(ingredient);
        }
        
        // Remove current holding ingredient
        if(HasKitchenObject())
        {
            switch(_currentKitchenObject)
            {
                case CompleteDishKitchenObject completeDishKitchenObject:
                    foreach(var kitchenObjectSO in completeDishKitchenObject.GetKitchenObjectSOList())
                        checkList.Remove(kitchenObjectSO);
                    break;
                default:
                    if(_currentKitchenObject != null)
                        checkList.Remove(_currentKitchenObject.GetKitchenObjectSO());
                    break;
            }
        }

        if (checkList.Count == 0) return true;

        foreach(KitchenObjectSO kitchenObjectSO in checkList)
        {
            if(AICounterManager.s.TryGetCounterHasKitchenObject(kitchenObjectSO, out BaseCounter resultCounter))
            {
                if(resultCounter is ContainerCounter)
                {
                    lackOfKitchenObject = kitchenObjectSO;
                    return false;
                }
            }
            else
            {
                lackOfKitchenObject = kitchenObjectSO;
                return false;
            }
        }
        return true;
    }
    private bool CheckChopOrFry()
    {
        if(AIKitchenObjectManager.s.IsNeedChop(_currentKitchenObject.GetKitchenObjectSO()))
        {
            // Find cut counter and choose it
            if(AICounterManager.s.TryGetEmptyCuttingCounter(out CuttingCounter resultCounter))
                SetSelectedCounter(resultCounter);
            return true;
        }
        else if(AIKitchenObjectManager.s.IsNeedFry(_currentKitchenObject.GetKitchenObjectSO()))
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
        if (_selectedCounter == null) return true;
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
    public bool IsWalking()
    {
        return _stage == BotStage.Move;
    }
    public bool IsHolding()
    {
        return HasKitchenObject();
    }
    #endregion
}

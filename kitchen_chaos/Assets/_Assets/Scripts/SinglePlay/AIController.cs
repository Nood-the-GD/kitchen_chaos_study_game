using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public enum BotStage
{
    Moving,
    Holding,
    Empty,
    AltInteract,
    AltInteractComplete,
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

    void Start()
    {
        _stage = BotStage.Empty;
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        switch (_stage)
        {
            case BotStage.Moving:
                CheckPosition();
                break;
            case BotStage.Holding:
                break;
            case BotStage.AltInteract:
                AltInteract();
                break;
            case BotStage.Empty:
                // if(_currentRecipeSO == null)
                // {
                //     GetRecipeSO();
                // }
                break;
            default:
                break;
        }        
    }

    private void SetTargetPosition(Vector3 position)
    {
        if(NavMesh.SamplePosition(position, out NavMeshHit hit, 100f, NavMesh.AllAreas))
        {
            _navMeshAgent.SetDestination(hit.position);
        }
    }
    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        _selectedCounter = selectedCounter;
    }
    private void CheckPosition()
    {
        if(Vector3.Distance(this.transform.position,  _selectedCounter.transform.position) <= _interactDistance)
        {
            Interact();
        }
        else
        {
            Debug.Log(Vector3.Distance(this.transform.position, _selectedCounter.transform.position));
        }
    }
    private void Interact()
    {
        _selectedCounter.Interact(this);
        if(_selectedCounter is CuttingCounter)
        {
            _startAltInteract = true;
            _stage = BotStage.AltInteract;
        }
        CheckHolding();
    }    
    private void AltInteract()
    {
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
                _stage = BotStage.AltInteractComplete;
            }
        }
    }

    private void CheckHolding()
    {
        if(HasKitchenObject())
        {
            _stage = BotStage.Holding;
        }
        else
        {
            _stage = BotStage.Empty;
        }
    }

    private void GetRecipeSO()
    {
        _currentRecipeSO = DeliveryManager.Instance.GetWaitingRecipeSOList()[0];
        KitchenObjectSO kitchenObjectSO = _currentRecipeSO.kitchenObjectSOList[0];
        GetKitchenObjectSO(kitchenObjectSO);
    }

    public void SetRecipeSO(RecipeSO recipeSO)
    {
        _currentRecipeSO = recipeSO;
        KitchenObjectSO kitchenObjectSO = _currentRecipeSO.kitchenObjectSOList[0];
        GetKitchenObjectSO(kitchenObjectSO);
    }

    private void GetKitchenObjectSO(KitchenObjectSO kitchenObjectSO)
    {
        if(_stage == BotStage.Holding)
        {

        }
        else if(_stage == BotStage.Empty)
        {
            // Check if any counter has this kitchen object
            if(AICounterManager.s.CheckIfAnyCounterHasKitchenObject(kitchenObjectSO, out BaseCounter resultCounter))
            {
                SetTargetPosition(resultCounter.transform.position);
                SetSelectedCounter(resultCounter);
                _stage = BotStage.Moving;
            }
            else if(kitchenObjectSO.kitchenObjectType == KitchenObjectType.NeedChop || kitchenObjectSO.kitchenObjectType == KitchenObjectType.NeedFried)
            {
                // Check if any counter has this kitchen object original
                KitchenObjectSO original = GetKitchenObjectOriginal(kitchenObjectSO);
                if(AICounterManager.s.CheckIfAnyCounterHasKitchenObject(original, out resultCounter))
                {
                    SetTargetPosition(resultCounter.transform.position);
                    SetSelectedCounter(resultCounter);
                    _stage = BotStage.Moving;
                }

                _aIActionQueue.Enqueue(AIAction.CreateAction(() =>
                {
                    // Add chop or fry action
                    if(kitchenObjectSO.kitchenObjectType == KitchenObjectType.NeedChop)
                    {
                        if(AICounterManager.s.TryGetEmptyCuttingCounter(out CuttingCounter resultCounter))
                            SetTargetPosition(resultCounter.transform.position);
                        SetSelectedCounter(resultCounter);
                        _stage = BotStage.Moving;
                    }
                    else if(kitchenObjectSO.kitchenObjectType == KitchenObjectType.NeedFried)
                    {
                        if(AICounterManager.s.TryGetEmptyStoveCounter(out StoveCounter resultCounter))
                            SetTargetPosition(resultCounter.transform.position);
                        SetSelectedCounter(resultCounter);
                        _stage = BotStage.Moving;
                    }
                }));
            }
        }
    }

    private void Chop()
    {

    }

    private KitchenObjectSO GetKitchenObjectOriginal(KitchenObjectSO kitchenObjectSO)
    {
        if(kitchenObjectSO.kitchenObjectType == KitchenObjectType.NeedChop)
        {
            return AICounterManager.s.GetKitchenObjectSoOriginal(kitchenObjectSO);
        }
        return null;
    }

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

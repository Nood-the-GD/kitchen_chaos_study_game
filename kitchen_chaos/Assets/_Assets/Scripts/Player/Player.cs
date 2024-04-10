using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon.StructWrapping;

public class Player : MonoBehaviour, IKitchenObjectParent
{
    #region Instance
    public static Player Instance {get; private set;}
    #endregion

    #region Events
    public static event Action<Player> OnPlayerSpawn;
    public event EventHandler OnPickupSomething;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    #endregion

    #region Variables
    public PhotonView photonView;
    public int viewId => photonView.ViewID;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    private GameInput gameInput => GameInput.Instance;
    private KitchenObject kitchenObject;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private bool isWalking;
    private float rotateSpeed = 20f;
    private float playerSize = 0.7f;
    private float playerRadius = 2f;
    private float moveDistance;
    private Vector3 moveDir = Vector3.zero;
    #endregion
    
    #region Unity functions
    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if(photonView.IsMine)
            if(Instance == null) Instance = this;
    }
    private void Start()
    {
        if(photonView.IsMine)
        {
            gameInput.OnInteractAction += GameInput_OnInteractAction;
            gameInput.OnUseAction += GameInput_OnInteractAlternateAction;
            OnPlayerSpawn?.Invoke(this);
        }
        
        PhotonManager.s.currentGamePlayers.Add(this);
    }
    private void Update()
    {
        HandleMovement();
        HandleInteraction();
    }
    #endregion

    #region Events functions
    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if(!GameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null) selectedCounter.CmdChop(viewId);
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if(!GameManager.Instance.IsGamePlaying()) return;

        if(selectedCounter != null) selectedCounter.CmdInteract(viewId);
        //selectedCounter
    }
    #endregion

    #region Interactions
    private void HandleInteraction()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalize();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)
        {
            // Rotate to player input direction 
            Vector3 tempDir = Vector3.Slerp(lastInteractDir, moveDir, rotateSpeed * Time.deltaTime);
            lastInteractDir = tempDir;
        }

        float interactDistance = 2f;
        if(Physics.Raycast(transform.position, lastInteractDir, out RaycastHit hitInfo, interactDistance))
        {
            if(hitInfo.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                //Has counter
                if(baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
               SetSelectedCounter(null);
            }
        }
        else
        {
            SetSelectedCounter(null);
        }
    }

    void OnGUI()
    {
        Debug.DrawRay(transform.position, lastInteractDir, Color.yellow);
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        GetMovementInput();
        if(MovementTypeController.Instance.isMobileController)
        {
            HandleMobileMovement();
        }
        else
        {
            HandlePCMovement();
        }
    }
    private void GetMovementInput()
    {
        if(!photonView.IsMine)
            return;

        Vector2 inputVector = gameInput.GetMovementVectorNormalize();

        moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        moveDistance = moveSpeed * Time.deltaTime;
    }
    private void HandlePCMovement()
    {
        bool canMove;
        canMove = !Physics.CapsuleCast(transform.position, transform.position + transform.up * playerRadius, playerSize, moveDir, moveDistance); // if any block on the way

        if (!canMove)
        {
            //Cannot move toward moveDir
            //Try to move on X direction
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + transform.up * playerRadius, playerSize, moveDirX, moveDistance); // Check raycast on direction x

            if (canMove)
            {
                //Can move only on the X
                moveDir = moveDirX;
            }
            else
            {
                //Cannot move on the X
                //Try to move on the Z
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + transform.up * playerRadius, playerSize, moveDirZ, moveDistance); // check raycast on direction z

                if (canMove)
                {
                    //Can move on the Z
                    moveDir = moveDirZ;
                }
                else
                {
                    //Cannot move any direction
                }
            }
        }

        if (canMove)
            transform.position += moveDir * moveDistance;


        isWalking = moveDir != Vector3.zero;
        SetWalking(isWalking);
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }
    private void HandleMobileMovement()
    {
        if(!photonView.IsMine)
            return;

        Vector2 inputVector = gameInput.GetMovementVectorNormalize();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        bool canMove;

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerSize = 0.7f;
        float playerRadius = 2f;
        canMove = !Physics.CapsuleCast(transform.position, transform.position + transform.up * playerRadius, playerSize, moveDir, moveDistance);

        if(!canMove)
        {

            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = !moveDir.x.IsInRange(-0.4f, 0.4f) && !Physics.CapsuleCast(transform.position, transform.position + transform.up * playerRadius, playerSize, moveDirX, moveDistance); // Check raycast on direction x
            if (canMove)
            {
                //Can move only on the X
                moveDir = moveDirX;
            }
            else
            {
                //Cannot move on the X
                //Try to move on the Z
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = !moveDir.z.IsInRange(-0.4f, 0.4f) && !Physics.CapsuleCast(transform.position, transform.position + transform.up * playerRadius, playerSize, moveDirZ, moveDistance); // check raycast on direction z

                if (canMove)
                {
                    //Can move on the Z
                    moveDir = moveDirZ;
                }
                else
                {
                    //Cannot move any direction
                }
            }
        }

        if (canMove)
            transform.position += moveDir * moveDistance;


        isWalking = moveDir != Vector3.zero;
        SetWalking(isWalking);
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }

    void SetWalking(bool isWalking){
        if(photonView.IsMine)
            photonView.RPC("RPCSetWalking", RpcTarget.All, isWalking);
    }
    #endregion

    #region Multiplay
    [PunRPC]
    void RPCSetWalking(bool isWalking){
        this.isWalking = isWalking;
    }
    #endregion

    #region Supports
    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }
    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    } 
    public bool IsWalking()
    {
        return isWalking;
    }
    public bool IsHolding()
    {
        return HasKitchenObject();
    }
    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if(kitchenObject != null)
        {
            OnPickupSomething?.Invoke(this, EventArgs.Empty);
        }
    }
    public KitchenObject GetKitchenObject()
    {
        return this.kitchenObject;
    }
    public void ClearKitchenObject()
    {
        this.kitchenObject = null;
    }
    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
    #endregion
}

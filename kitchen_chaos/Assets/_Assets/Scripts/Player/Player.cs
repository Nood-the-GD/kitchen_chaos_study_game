using System;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;
using System.Collections;

public class Player : MonoBehaviour, IPlayer
{
    #region Instance
    public static Player Instance { get; private set; }
    #endregion

    #region Events
    public static event Action<Player> OnPlayerSpawn;
    public static event EventHandler OnPickupSomething;
    public static event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }
    #endregion

    #region Variables
    private PhotonView _photonView;
    public int viewId => _photonView.ViewID;
    public PhotonView photonView => _photonView;
    public KitchenObject kitchenObject { get; set; }
    public PlayerAnimator playerAnimator;
    public bool IsControlling { get; set; }

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    private GameInput gameInput => GameInput.Instance;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private bool isWalking;
    private float rotateSpeed = 20f;
    private float playerSize = 0.7f;
    private float playerRadius = 1f;
    private float moveDistance;
    private Vector3 moveDir = Vector3.zero;
    private bool isChopping = false;
    private bool isFrying = false;
    #endregion

    #region Unity functions
    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        if (_photonView.IsMine)
            if (Instance == null) Instance = this;
        SinglePlayManager.s.OnPlayerChange += OnPlayerChangeHandler;
    }
    private void Start()
    {
        if (_photonView.IsMine && this.gameObject.tag == "MainPlayer")
        {
            RegisterGameInputEvent();
            OnPlayerSpawn?.Invoke(this);
            var colorSkin = UserSetting.colorSkin;
            CmdSetSkinColor(colorSkin);
        }

        PhotonManager.s.currentGamePlayers.Add(this);
    }
    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.F) && selectedCounter != null)
        // {
        //     if (selectedCounter is IPlaceable placeable)
        //     {
        //         placeable.StartPlacing();
        //     }
        // }

        if (!SectionData.s.isSinglePlay && !_photonView.IsMine) return;
        if (!IsControlling)
        {
            if (isFrying)
            {
                if (selectedCounter is StoveCounter)
                {
                    CheckFrying();
                }
            }
            return;
        }
        HandleMovement();
        HandleSelection();
    }
    void OnDestroy()
    {
        SinglePlayManager.s.OnPlayerChange -= OnPlayerChangeHandler;
    }
    #endregion

    #region Events functions
    public void RegisterGameInputEvent()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
        gameInput.OnUseAction += GameInput_OnInteractAlternateAction;
    }
    public void UnregisterGameInputEvent()
    {
        gameInput.OnInteractAction -= GameInput_OnInteractAction;
        gameInput.OnUseAction -= GameInput_OnInteractAlternateAction;
    }

    private void GameInput_OnInteractAlternateAction(object sender, EventArgs e)
    {
        if (!GameManager.s.IsGamePlaying()) return;

        if (selectedCounter != null)
        {
            if (selectedCounter is CuttingCounter)
            {
                isChopping = true;
                ((CuttingCounter)selectedCounter).CmdChop(viewId);
                Vector3 direction = (selectedCounter.transform.position - this.transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    this.transform.forward = direction;
                }
            }
        }
    }
    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
            Vector3 direction = (selectedCounter.transform.position - this.transform.position).normalized;
            if (direction != Vector3.zero)
            {
                this.transform.forward = direction;
            }
        }
    }
    private void OnPlayerChangeHandler(Player player)
    {
        if (player == this)
        {
            isChopping = false;
        }
        else
        {
            isWalking = false;
            if (isChopping)
            {
                StopCoroutine(CR_AutoChop());
                StartCoroutine(CR_AutoChop());
            }
        }
    }
    IEnumerator CR_AutoChop()
    {
        while (isChopping)
        {
            yield return new WaitForSeconds(0.5f);
            if (selectedCounter is CuttingCounter)
            {
                GameInput_OnInteractAlternateAction(this, EventArgs.Empty);
            }
        }
    }
    #endregion

    #region Selection
    private void HandleSelection()
    {
        Vector3 playerInputDir = gameInput.GetMovementVectorNormalize().ToVector3XZ();
        if (playerInputDir != Vector3.zero)
        {
            lastInteractDir = playerInputDir;
        }

        const float maxInteractionDistance = 2;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit hitInfo, maxInteractionDistance))
        {
            SetSelectedCounter(hitInfo.transform.GetComponent<BaseCounter>());
        }
        else
        {
            SetSelectedCounter(null);
        }
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        GetMovementInput();
        HandleRotation(moveDir);
#if UNITY_EDITOR
        if (MovementTypeController.Instance.isMobileController)
        {
            HandleMobileMovement();
        }
        else
        {
            HandlePCMovement();
        }
#elif UNITY_IPHONE || UNITY_ANDROID
            HandleMobileMovement();
#endif

    }
    private void GetMovementInput()
    {
        if (!_photonView.IsMine && SectionData.s.isSinglePlay == false)
            return;

        moveDir = gameInput.GetMovementVectorNormalize().ToVector3XZ();
        moveDistance = moveSpeed * Time.deltaTime;
    }
    /**
     * Handle player movement on PC platform
     * Check if movement is possible and make movement if it is
     */
    private void HandlePCMovement()
    {
        if (!_photonView.IsMine && SectionData.s.isSinglePlay == false) return;
        // Get normalized direction of movement
        var direction = moveDir;

        // Check if movement is possible
        var movementCheck = CanMove(direction, moveDistance);
        // If movement is not possible, try axis separated movement
        if (!movementCheck)
        {
            bool xCheck = CanMove(new Vector3(direction.x, 0, 0), moveDistance);
            bool zCheck = CanMove(new Vector3(0, 0, direction.z), moveDistance);
            direction = xCheck ? new Vector3(direction.x, 0, 0) : zCheck ? new Vector3(0, 0, direction.z) : Vector3.zero;
        }

        // Make movement on calculated direction
        transform.position += direction.normalized * moveDistance;

        // Set if player is walking or not
        isWalking = direction != Vector3.zero;
        CmdSetWalking(isWalking);

        // Rotate player smoothly if direction is valid
        if (direction != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * rotateSpeed);
        }
    }

    private void HandleMobileMovement()
    {
        if (!_photonView.IsMine && SectionData.s.isSinglePlay == false) return;

        // Get normalized direction of movement
        var direction = moveDir;

        // Check if movement is possible
        var movementCheck = CanMove(direction, moveDistance);

        // If movement is not possible, check axis separated movement with additional thresholds
        if (!movementCheck)
        {
            bool xCheck = CanMove(new Vector3(direction.x, 0, 0), moveDistance) && !direction.x.IsInRange(-0.5f, 0.5f);
            bool zCheck = CanMove(new Vector3(0, 0, direction.z), moveDistance) && !direction.z.IsInRange(-0.5f, 0.5f);
            direction = xCheck ? new Vector3(direction.x, 0, 0) : zCheck ? new Vector3(0, 0, direction.z) : Vector3.zero;
        }

        // Make movement on calculated direction
        transform.position += direction.normalized * moveDistance;

        // Set if player is walking or not
        isWalking = direction != Vector3.zero;
        CmdSetWalking(isWalking);
    }

    private void HandleRotation(Vector3 direction)
    {
        if (!_photonView.IsMine && SectionData.s.isSinglePlay == false) return;

        // Smoothly rotate player only if direction is valid
        if (direction != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, direction, Time.deltaTime * rotateSpeed);
        }
    }

    private bool CanMove(Vector3 direction, float distance)
    {
        return !Physics.CapsuleCast(transform.position, transform.position + transform.up * playerRadius, playerSize, direction, distance);
    }

    void CmdSetWalking(bool isWalking)
    {
        if (_photonView.IsMine)
            _photonView.RPC("RPCSetWalking", RpcTarget.All, isWalking);
    }

    void CmdSetSkinColor(ColorSkin colorSkin)
    {
        if (_photonView.IsMine)
            _photonView.RPC("RPCSetColorSkin", RpcTarget.All, colorSkin.colorCode);
    }
    #endregion

    #region Multiplay
    [PunRPC]
    void RPCSetWalking(bool isWalking)
    {
        this.isWalking = isWalking;
    }
    [PunRPC]
    void RPCSetColorSkin(string id)
    {
        var skin = GameData.s.GetColorSkin(id);
        GetComponentInChildren<SkinnedMeshRenderer>().material = skin.material;
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
        if (kitchenObject != null)
        {
            if (this.kitchenObject == null || kitchenObject.gameObject != this.kitchenObject.gameObject)
                OnPickupSomething?.Invoke(this, EventArgs.Empty);
        }
        this.kitchenObject = kitchenObject;
    }
    public KitchenObject GetKitchenObject()
    {
        return this.kitchenObject;
    }
    public void ClearKitchenObject(bool destroyChild = true)
    {
        if (destroyChild)
        {
            if (kitchenObject != null)
            {
                kitchenObject.DestroySelf();
            }
        }
        this.kitchenObject = null;
    }
    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
    #endregion

    #region SinglePlay
    private void CheckFrying()
    {
        if (selectedCounter is StoveCounter && (selectedCounter as StoveCounter).IsCookComplete())
        {
            isFrying = false;
            selectedCounter.Interact(this);
        }
    }
    #endregion
}

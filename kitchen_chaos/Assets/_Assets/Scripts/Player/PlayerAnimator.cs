using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerAnimator : MonoBehaviour
{
    #region Constants
    private const string IS_WALKING = "IsWalking";
    private const string IS_HOLDING = "IsHolding";
    #endregion

    #region Variables
    private IPlayer player;
    [SerializeField] private GameObject arrowGO;
    private Animator animator;
    private GameInput gameInput => GameInput.Instance;
    public Text usernameUI;
    #endregion

    #region Unity functions
    private void Awake()
    {
        player = GetComponentInParent<IPlayer>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        if(!player.photonView.IsMine)
        {
            arrowGO.gameObject.SetActive(false);
        }
        if (player is AIController)
            return;
        usernameUI.text = player.photonView.Owner.NickName;
    }
    private void Update()
    {
        animator.SetBool(IS_WALKING, player.IsWalking());  
        animator.SetBool(IS_HOLDING, player.IsHolding());
        if(player.photonView.IsMine)
        {
            arrowGO.transform.forward = gameInput.GetMovementVectorNormalize().ToVector3XZ(arrowGO.transform.position.y);
        }
    }
    #endregion
}

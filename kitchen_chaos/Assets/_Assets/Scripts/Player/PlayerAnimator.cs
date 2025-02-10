using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimator : MonoBehaviour
{
    #region Constants
    private const string ANIM_IS_WALKING = "IsWalking";
    private const string ANIM_IS_HOLDING = "IsHolding";
    #endregion

    #region Variables
    [SerializeField] private GameObject arrowGO;
    [SerializeField] private Text usernameUI;
    private IPlayer player;
    private Animator animator;
    private GameInput gameInput => GameInput.Instance;
    #endregion

    #region Unity functions
    private void Awake()
    {
        player = GetComponentInParent<IPlayer>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        if (!player.photonView.IsMine)
        {
            arrowGO.gameObject.SetActive(false);
        }
        if (player is AIController)
            return;
        usernameUI.text = player.photonView.Owner.NickName;
    }
    private void Update()
    {
        animator.SetBool(ANIM_IS_WALKING, player.IsWalking());
        animator.SetBool(ANIM_IS_HOLDING, player.IsHolding());
        if (player.photonView.IsMine)
        {
            arrowGO.transform.forward = gameInput.GetMovementVectorNormalize().ToVector3XZ(arrowGO.transform.position.y);
        }
    }
    public void SetPlayerName(string name)
    {
        usernameUI.text = name;
    }
    #endregion
}

using System;
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
    private Player player;
    private Animator animator;
    private GameInput gameInput => GameInput.Instance;
    #endregion

    #region Unity functions
    private void Awake()
    {
        player = GetComponentInParent<Player>();
        animator = GetComponent<Animator>();
        if (SectionData.s.isSinglePlay)
            SinglePlayManager.s.OnPlayerChange += SinglePlayManager_OnPlayerChange;
    }
    private void OnDestroy()
    {
        if (SectionData.s.isSinglePlay && SinglePlayManager.s != null)
            SinglePlayManager.s.OnPlayerChange -= SinglePlayManager_OnPlayerChange;
    }
    void Start()
    {
        if (!player.IsControlling || !SectionData.s.isSinglePlay && !player.photonView.IsMine)
        {
            arrowGO.gameObject.SetActive(false);
        }
        if (player.gameObject.tag == "FakePlayer")
            return;
        usernameUI.text = player.photonView.Owner.NickName;
    }
    private void Update()
    {
        animator.SetBool(ANIM_IS_WALKING, player.IsWalking());
        animator.SetBool(ANIM_IS_HOLDING, player.IsHolding());
        if (player.IsControlling)
        {
            arrowGO.transform.forward = gameInput.GetMovementVectorNormalize().ToVector3XZ(arrowGO.transform.position.y);
        }
    }
    #endregion

    private void SinglePlayManager_OnPlayerChange(Player player)
    {
        if (player == this.player)
        {
            arrowGO.gameObject.SetActive(true);
        }
        else
        {
            arrowGO.gameObject.SetActive(false);
        }
    }

    public void SetPlayerName(string name)
    {
        usernameUI.text = name;
    }
}

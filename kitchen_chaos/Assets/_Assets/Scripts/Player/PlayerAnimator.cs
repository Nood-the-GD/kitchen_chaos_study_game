using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerAnimator : MonoBehaviour
{
    private const string IS_WALKING = "IsWalking";
    private const string IS_HOLDING = "IsHolding";


    [SerializeField] private Player player;
    [SerializeField] private GameObject arrowGO;
    private Animator animator;
    private GameInput gameInput => GameInput.Instance;
    public Text usernameUI;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetBool(IS_WALKING, player.IsWalking());  
        animator.SetBool(IS_HOLDING, player.IsHolding());
        if(player.photonView.IsMine){
            arrowGO.transform.forward = gameInput.GetMovementVectorNormalize().ToVector3XZ(arrowGO.transform.position.y);
        }
        usernameUI.text = player.photonView.Owner.NickName;
    }
}

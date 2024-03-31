using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private const string IS_WALKING = "IsWalking";
    private const string IS_HOLDING = "IsHolding";


    [SerializeField] private Player player;
    [SerializeField] private GameObject arrowGO;
    private Animator animator;
    private GameInput gameInput => GameInput.Instance;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        animator.SetBool(IS_WALKING, player.IsWalking());  
        animator.SetBool(IS_HOLDING, player.IsHolding());
        arrowGO.transform.forward = gameInput.GetMovementVectorNormalize().ToVector3XZ(arrowGO.transform.position.y);
    }
}

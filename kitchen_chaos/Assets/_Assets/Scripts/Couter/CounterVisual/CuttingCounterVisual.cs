using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounterVisual : MonoBehaviour
{
    private const string CUT_TRIGGER = "Cut";

    [SerializeField] private CuttingCounter cuttingCounter;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        cuttingCounter.OnCutAction += CuttingCounter_OnCutAction;
    }

    private void OnDestroy()
    {
        cuttingCounter.OnCutAction -= CuttingCounter_OnCutAction;
    }


    private void CuttingCounter_OnCutAction(object sender, System.EventArgs e)
    {
        animator.SetTrigger(CUT_TRIGGER);
    }
}

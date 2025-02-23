using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoveCounterVisual : MonoBehaviour
{
    [SerializeField] private StoveCounter stoveCounter;
    [SerializeField] private GameObject stoveOnVisual;
    [SerializeField] private ParticleSystem sizzlingParticle;

    private void Start()
    {
        stoveCounter.OnStateChanged += StoveCounter_OnStateChanged;
        EndFrying();
    }

    private void StoveCounter_OnStateChanged(object sender, StoveCounter.OnStateChangedEventArg e)
    {
        if (e.state == StoveCounter.State.Idle)
        {
            EndFrying();
        }
        else
        {
            Frying();
        }
    }

    public void Frying()
    {
        stoveOnVisual.SetActive(true);
        sizzlingParticle.gameObject.SetActive(true);
    }

    public void EndFrying()
    {
        stoveOnVisual.SetActive(false);
        sizzlingParticle.gameObject.SetActive(false);
    }
}

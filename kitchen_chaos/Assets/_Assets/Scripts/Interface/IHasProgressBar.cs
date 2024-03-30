using System;
using UnityEngine;

public interface IHasProgressBar
{
    public event EventHandler<OnProcessChangedEvenArgs> OnProcessChanged;
    public class OnProcessChangedEvenArgs : EventArgs
    {
        public float processNormalize;
    }
}

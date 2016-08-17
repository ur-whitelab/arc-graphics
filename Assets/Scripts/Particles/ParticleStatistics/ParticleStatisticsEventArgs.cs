using UnityEngine;
using System.Collections;
using System;

public class ParticleStatisticsModifierEventArgs : EventArgs {

    public int ModifierType;
    public int ModifierIndex;
    public int[] sum;

    public ParticleStatisticsModifierEventArgs(int ModifierType, int ModifierIndex, int[] sum)
    {
        this.ModifierType = ModifierType;
        this.ModifierIndex = ModifierIndex;
        this.sum = sum;
    }

}

using UnityEngine;
using System.Collections;
using System;

namespace Rochester.ARTable.Particles
{
    public class ParticleStatisticsModifierEventArgs : EventArgs
    {

        //modifier type/index refer to things manipulating a praticle's state
        //-1 indicates none
        public int ModifierType { get; private set; }
        public int ModifierIndex { get; private set; }
        public int[] sum { get; private set; }

        public ParticleStatisticsModifierEventArgs(int ModifierType, int ModifierIndex, int[] sum)
        {
            this.ModifierType = ModifierType;
            this.ModifierIndex = ModifierIndex;
            this.sum = sum;
        }

    }

}
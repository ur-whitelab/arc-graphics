using UnityEngine;
using System.Collections;
using System;

namespace Rochester.ARTable.Particles
{
    public class ParticleStatisticsModifierEventArgs : EventArgs
    {

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

    public class ParticleStatisticsTargetEventArgs : EventArgs
    {
        public int targetIndex { get; private set; }
        public int sum { get; private set; }

        public ParticleStatisticsTargetEventArgs(int targetIndex, int sum)
        {
            this.targetIndex = targetIndex;
            this.sum = sum;
        }

    }
}
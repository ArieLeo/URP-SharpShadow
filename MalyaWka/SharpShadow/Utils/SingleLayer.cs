using System;
using UnityEngine;

namespace MalyaWka.SharpShadow.Utils
{
    [Serializable]
    public struct SingleLayer
    {
        [SerializeField] private int layerIndex;

        public int LayerIndex
        {
            get => layerIndex;
            set
            {
                if (value > 0 && value < 32)
                {
                    layerIndex = value;
                }
            }
        }

        public LayerMask Mask => 1 << layerIndex;

        public static implicit operator SingleLayer(int value)
        {
            return new SingleLayer { LayerIndex = value };
        }

        public static implicit operator int(SingleLayer value)
        {
            return value.LayerIndex;
        }
    }
}
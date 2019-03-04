using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interactables
{
    [Serializable]
    public class Interactable
    {
        public string Id;

        public int TimeToInteract;

        public int RespawnTime;

        public float MaximumDistance;
        public float PositionX;
        public float PositionY;
        public float PositionZ;
    }
}

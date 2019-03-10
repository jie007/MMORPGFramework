using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Interactables;
using Common.Mobs;
using Common.Navigation;

namespace Common
{
    [Serializable]
    public class MapInformation
    {
        public string MapName;

        public List<Interactable> Interactables = new List<Interactable>();

        public List<MobWanderInformation> Mobs = new List<MobWanderInformation>();

        public NavMesh NavMesh = new NavMesh();
    }
}

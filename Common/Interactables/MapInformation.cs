using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interactables
{
    [Serializable]
    public class MapInformation
    {
        public string MapName;

        public List<Interactable> Interactables = new List<Interactable>();
    }
}

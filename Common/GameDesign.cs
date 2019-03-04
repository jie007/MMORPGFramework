using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class GameDesign
    {
        public static int MaximumNumberOfCharacters = 3;

        public static int MapAreaSize = 30;
        public static float TimeoutOnMap = 10;
        public static int RegistrationBorder = 1;
        public static int UnregistrationBorder = 2;
        public static int ServerRememberEventTimeInMs = 10000;
        public static int TimeTolerance = 300;
        public static int InteractableRespawnTimerDelay = 100;
    }
}

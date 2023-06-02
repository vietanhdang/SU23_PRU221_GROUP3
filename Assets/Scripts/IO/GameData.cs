using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.IO
{
    [Serializable]
    public class GameData
    {
        public int waveNumber;
        public int totalMoney;
        public int totalEscaped;
        public int roundEscaped;
        public int totalKilled;
    }
}

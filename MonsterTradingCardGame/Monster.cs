using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonsterTradingCardGame
{
    class Monster : Card
    {
        /// <summary>        
        /// </summary>
        /// <param name="dinoType"></param>
        /// <param name="name"></param>
        /// <param name="shopCost"></param>
        /// <param name="damage"></param>
        public Monster(EDinoTypes dinoType, string name, string description, int shopCost, int damage)
               : base (dinoType, name, description, shopCost, damage)
        {

        }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MTCG.Card;

namespace MTCG
{
    class Spell : Card
    {
        // TODO: find a way to make healingspells
        // private int _healing { get; }


        //Spell that causes damage
        public Spell(ElementalTypes elementalType, string name, int damage, int cost) : base(elementalType, name, damage, cost)
        {

        }
        //Spell that does cause damage
        public Spell(ElementalTypes elementalType, string name, int cost) : base(elementalType, name, cost)
        {
            //_healing = healing;
        }
    }
}

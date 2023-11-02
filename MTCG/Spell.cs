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
        private int _healing { get; }


        //Spell that causes damage
        public Spell(ElementalTypes elementalType, int manaCost, string name, int damageAmount, int cost) : base(elementalType, manaCost, name, damageAmount, cost)
        {

        }
        //Spell that does causes healing
        public Spell(ElementalTypes elementalType, int manaCost, int healingAmount, string name, int cost) : base(elementalType, manaCost, name, cost)
        {
            _healing = healingAmount;
        }
    }
}

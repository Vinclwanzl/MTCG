using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    public class TrapSpell : Spell
    {
        private EDinoTypes _trapTrigger;
        public EDinoTypes TrapTrigger { get { return _trapTrigger; } }

        /// <summary>
        /// Trap Spell -> deal alot of damage against Spells with specific Type given in trapTrigger field and none against the rest
        /// </summary>
        /// <param name="dinoType"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="shopCost"></param>
        /// <param name="damage"></param>
        /// <param name="trapTrigger"></param>
        public TrapSpell(string id, EDinoTypes dinoType, string name, string description, int shopCost, int damage, EDinoTypes trapTrigger) 
                  : base(id, dinoType, ESpellTypes.TRAP, name, description, shopCost, damage)
        {
            _trapTrigger = trapTrigger;
        }
        public override string ToString()
        {
            return $"Name: {this.Name} | Damage: {this.Damage} {Enum.GetName(typeof(EDinoTypes), this.DinoType)}-damage | " +
                   $"Trap-trigger: {this.TrapTrigger} | Description: {this.Description} | Cost: {this.ShopCost}|";
        }
    }
}

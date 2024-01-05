using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    class BuffSpell : Spell
    {
        private int _buffAmount;
        public int BuffAmount { get { return _buffAmount; } }

        /// <summary>
        /// Buff Spell -> deal less damage and buff the next dino
        /// </summary>
        /// <param name="dinoType"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="shopCost"></param>
        /// <param name="damage"></param>
        /// <param name="buffAmount"></param>
        public BuffSpell(string id, EDinoTypes dinoType, string name, string description, int shopCost, int damage, int buffAmount) 
                  : base(id, dinoType, ESpellTypes.BUFF, name, description, shopCost, damage)
        {
            _buffAmount = buffAmount;
        }
        public override string ToString()
        {
            string damageType = Enum.GetName(typeof(EDinoTypes), this.DinoType) + "-damage";
            return $"Name: {this.Name} | Damage: {this.Damage} {damageType} | and Buffs: {this.BuffAmount} {damageType} | " +
                   $"Description: {this.Description} | Cost: {this.ShopCost}|\n";
        }
    }
}

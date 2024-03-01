using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;

namespace MonsterTradingCardGame
{
    public enum ESpellTypes
    {
        NORMAL = 0,
        BUFF = 1,
        TRAP = 2
    }
    public class Spell : Card
    {
        private ESpellTypes _spellType;
        public ESpellTypes SpellType { get { return _spellType; } }

        /// <summary>
        /// Normal Spell -> deal damage 
        /// </summary>
        /// <param name="dinoType"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="shopCost"></param>
        /// <param name="damage"></param>
        public Spell(string id, EDinoTypes dinoType, ESpellTypes spellType, string name, string description, int shopCost, int damage)
              : base(id, dinoType, name, description, shopCost, damage)
        {
            _spellType = spellType;
        }
        public override string ToString()
        {
            return $"Name: {this.Name} | Damage: {this.Damage} {Enum.GetName(typeof(EDinoTypes), this.DinoType)}-damage | " +
                   $"Description: {this.Description} | Cost: {this.ShopCost}|";
        }
    }
}

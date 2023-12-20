using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MonsterTradingCardGame
{
    public enum ESpellTypes
    {
        NORMAL = 0,
        BUFF = 1,
        TRAP = 2
    }
    class Spell : Card
    {
        
        private ESpellTypes _spellType;
        public ESpellTypes SpellType { get { return _spellType; } }
        private EDinoTypes _trapTrigger;
        public EDinoTypes TrapTrigger { get { return _trapTrigger; } }

        private int _buffAmount;
        public int BuffAmount { get { return _buffAmount; } }
        /// <summary>
        /// Normal Spell -> deal damage 
        /// </summary>
        /// <param name="dinoType"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="shopCost"></param>
        /// <param name="damage"></param>
        public Spell(EDinoTypes dinoType, string name, string description, int shopCost, int damage)
              : base(dinoType, name, description, shopCost, damage)
        {
            _spellType = ESpellTypes.NORMAL;
        }
        /// <summary>
        /// Buff Spell -> deal less damage and buff the next dino
        /// </summary>
        /// <param name="dinoType"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="shopCost"></param>
        /// <param name="damage"></param>
        /// <param name="buffAmount"></param>
        public Spell(EDinoTypes dinoType, string name, string description, int shopCost, int damage, int buffAmount)
              : base(dinoType, name, description, shopCost, damage)
        {
            _spellType = ESpellTypes.BUFF;
            _buffAmount = buffAmount;
        }
        /// <summary>
        /// Trap Spell -> deal alot of damage against Spells with specific Type given in trapTrigger field and none against the rest
        /// </summary>
        /// <param name="dinoType"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="shopCost"></param>
        /// <param name="damage"></param>
        /// <param name="trapTrigger"></param>
        public Spell(EDinoTypes dinoType, string name, string description, int shopCost, int damage, EDinoTypes trapTrigger)
              : base(dinoType, name, description, shopCost, damage)
        {
            _spellType = ESpellTypes.TRAP;
            _trapTrigger = trapTrigger;
        }
    }
}

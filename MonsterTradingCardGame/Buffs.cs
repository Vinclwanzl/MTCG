using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    public class Buffs
    {
        private bool _buffExists = false;
        public bool BuffExists { get {  return _buffExists; } }
        private List<EDinoTypes> _buffTypes;
        public List<EDinoTypes> BuffTypes { get { return _buffTypes; } }
        private List<int> _buffAmount;
        public List<int> BuffAmount { get { return _buffAmount; } }
        public Buffs() 
        {
            _buffTypes = new List<EDinoTypes>();
            _buffAmount = new List<int>();
        }

        public void AddBuff(EDinoTypes type, int amount)
        {
            _buffTypes.Add(type);
            _buffAmount.Add(amount);
            _buffExists = true;
        }
        public void RemoveBuffs() 
        {
            _buffTypes.Clear();
            _buffAmount.Clear();
            _buffExists = false;
        }
    }   
}

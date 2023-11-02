using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    internal class MonsterField
    {

        private List<Monster> _monsterField;
        public List<Monster> Monstes {
            get { return _monsterField; }
        }
        public MonsterField() {
            _monsterField = new List<Monster>();
        }  
        
        public bool rmMonster(int location) 
        { 
            if(-1 < location && location < 7 && _monsterField.ElementAt(location) != null) 
            {   
                _monsterField.RemoveAt(location);
                return true; 
            }
            return false;
        }
        public bool addMonster(Monster monster)
        {
            if (addMonster != null && _monsterField.Count() == 7)
            {
                _monsterField.Add(monster);
                return true;
            }
            return false;
        }
    }
}

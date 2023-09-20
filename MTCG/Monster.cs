using MTCG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static MTCG.Card;
using static System.Net.Mime.MediaTypeNames;

namespace MTCG
{
    class Monster : Card, IHealth
    {
        private readonly int _healthStat;
        public int CurrentHealth { get; set; }

        public Monster(ElementalTypes elementalType, string name, int cost, int damage, int health) : base(elementalType, name, cost, damage)
        {
            _healthStat = health;
            CurrentHealth = health;
        }

        public void attack(Monster target) {
            throw new NotImplementedException();
        }

        public void die()
        {
            throw new NotImplementedException();
        }

        public void recieveDamage(int amount)
        {
            CurrentHealth -= amount;
            if ( CurrentHealth < 1) this.die();
        }

        public void recieceHealing(int amount)
        {
            if (_healthStat <= CurrentHealth + amount) CurrentHealth = _healthStat;
            else CurrentHealth += amount;
        }
    }
}

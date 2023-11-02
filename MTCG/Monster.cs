using MTCG;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static MTCG.Card;
using static System.Net.Mime.MediaTypeNames;

namespace MTCG
{
    class Monster : Card, IHealth
    {
        private bool _isDEAD;
        public bool IsDEAD {  get { return _isDEAD; } }

        private readonly int _healthStat;
        private int _currentHealth;
        public int CurrentHealth {
            get {  return _currentHealth; } 
        }

        public Monster(ElementalTypes elementalType, int manaCost, string name, int shopCost, int damage, int health) : base(elementalType, manaCost, name, shopCost, damage)
        {
            _isDEAD = false; 
            _healthStat = health;
            _currentHealth = health;
        }
        
        public Monster Attack( Monster target) {

            double appliedDamgeForTarget = this.Damage;
            double appliedDamgeForSelf = target.Damage;

            switch (this.ElementalType)
            {
                case ElementalTypes.WATER:
                    // weak against 
                    if(target.ElementalType == ElementalTypes.EARTH)
                    {
                        appliedDamgeForTarget *= 0.5;
                        appliedDamgeForSelf *= 2;
                    }
                    // strong against
                    else if (target.ElementalType == ElementalTypes.FIRE)
                    {
                        appliedDamgeForTarget *= 2;
                        appliedDamgeForSelf *= 0.5;
                    }

                    break;
                case ElementalTypes.EARTH:
                    // weak against 
                    if (target.ElementalType == ElementalTypes.AIR)
                    {
                        appliedDamgeForTarget *= 0.5;
                        appliedDamgeForSelf *= 2;
                    }
                    // strong against
                    else if (target.ElementalType == ElementalTypes.WATER)
                    {
                        appliedDamgeForTarget *= 2;
                        appliedDamgeForSelf *= 0.5;
                    }
                    break;
                case ElementalTypes.AIR:
                    // weak against 
                    if (target.ElementalType == ElementalTypes.FIRE)
                    {
                        appliedDamgeForTarget *= 0.5;
                        appliedDamgeForSelf *= 2;
                    }
                    // strong against
                    else if (target.ElementalType == ElementalTypes.EARTH)
                    {
                        appliedDamgeForTarget *= 2;
                        appliedDamgeForSelf *= 0.5;
                    }
                    break;
                case ElementalTypes.FIRE:
                    // weak against 
                    if (target.ElementalType == ElementalTypes.WATER)
                    {
                        appliedDamgeForTarget *= 0.5;
                        appliedDamgeForSelf *= 2;
                    }
                    // strong against
                    else if (target.ElementalType == ElementalTypes.AIR)
                    {
                        appliedDamgeForTarget *= 2;
                        appliedDamgeForSelf *= 0.5;
                    }
                    break;
                default:
                    // this should not be possible
                    break;
            }

            target.RecieveDamage((int)appliedDamgeForTarget);
            this.RecieveDamage((int)appliedDamgeForSelf);

            return target;
        }

        // removes Health of Monster 
        // and sets _isDEAD to true if Health to low
        public void RecieveDamage(int amount)
        {
            _currentHealth -= amount;
            if ( CurrentHealth < 1)
                _isDEAD = true;

        }
        public void Heal(int amount, ElementalTypes type)
        {
            //this.type == type : heal
            //this.type == oppositetype : heal = damage
            //this.type != type : half
            if (this.ElementalType == type)
                RecieceHealing(amount);

            else if (((int)this.ElementalType) % 2 == ((int)type % 2))
                RecieveDamage(amount);

            else
                RecieceHealing(amount/2);
        }
        public void RecieceHealing(int amount)
        {
            if (_healthStat <= _currentHealth + amount)
                _currentHealth = _healthStat;
            else
                _currentHealth += amount;
        }
        // not sure if I will need this methode, but there is a possability that I will do so ## DELETE IF 0 REFERENCES
        public void Die()
        {
            _isDEAD = true;
        }
    }
}

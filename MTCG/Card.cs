using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MTCG
{
    public enum ElementalTypes
    {
        WATER = 0,
        EARTH = 1,
        FIRE = 2,
        AIR = 3
    }
    public abstract class Card
    {
        private ElementalTypes _elementalType;
        public ElementalTypes ElementalType { get { return _elementalType; } }
        private string _name;
        public string Name { get { return _name; } }

        private int _damage;
        public int Damage { get { return _damage; } }

        private int _ShopCost;
        public int CoinCost { get { return _ShopCost; } }

        private int _ManaCost;
        public int ManaCost 
        { 
            get { return _ManaCost; } 
            set 
            {
                if((_ManaCost + value) < 0)
                    _ManaCost = 0; 
                else
                    _ManaCost += value;
            }
        }
        public Card(ElementalTypes elementalType, int manaCost, string name, int shopCost, int damage) {
            _elementalType = elementalType;
            _ManaCost = manaCost;
            _name = name;
            _ShopCost = shopCost;
            _damage = damage;
        }

        public Card(ElementalTypes elementalType, int manaCost, string name, int cost)
        {
            _elementalType = elementalType;
            _name = name;
            _ShopCost = cost;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    public enum EDinoTypes
    {
        AQUATIC = 0,
        TERRESTRIAL = 1,
        VOLCANIC = 2,
        AERIAL = 3
    }
    class Card
    {
        private string _id;
        public string ID { get { return _id; } }

        private EDinoTypes _dinoType;
        public EDinoTypes DinoType { get { return _dinoType; } }
        private string _name;
        public string Name { get { return _name; } }

        private string _description;
        public string Description { get { return _name; } }

        private int _shopCost;
        public int ShopCost { get { return _shopCost; } }

        private int _damage;
        public int Damage { get { return _damage; } }

        public Card(string id, EDinoTypes dinoType, string name, string description, int shopCost, int damage)
        {
            _id = id;
            _dinoType = dinoType;
            _name = name;
            _description = description;
            _shopCost = shopCost;
            _damage = damage;
        }
        public override string ToString()
        {
            return "Name: " + _name + " | Dinotype: " + Enum.GetName(typeof(EDinoTypes), _dinoType) + " | Damage: " + _damage + " |Description:" + _description + " |Cost: " + _shopCost + "|";
        }
    }
}

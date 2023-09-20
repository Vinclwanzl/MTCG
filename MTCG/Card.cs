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
        WATER,
        EARTH,
        FIRE,
        AIR
    }
    abstract class Card
    {  
        private ElementalTypes _elementalType { get; }
        private string _name { get; }
        private int _damage { get; }
        private int _cost { get; }
        public Card(ElementalTypes elementalType, string name, int cost, int damage) {
            _elementalType = elementalType;
            _name = name;
            _cost = cost;
            _damage = damage;
        }

        public Card(ElementalTypes elementalType, string name, int cost)
        {
            _elementalType = elementalType;
            _name = name;
            _cost = cost;
        }
    }
}

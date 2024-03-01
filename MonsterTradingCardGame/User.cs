using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    public class User
    {
        private string _name;
        public string Name 
        { 
            get { return _name; } 
            set { _name = value; } 
        }
        
        private List<Card> _deck;
        public List<Card> Deck 
        { 
            get { return _deck; } 
            set 
            { 
                if(value != null)
                    _deck = value; 
            } 
        }

        public User(string name, List<Card> deck)
        {
            _name = name;
            _deck = deck;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MTCG
{
    class User
    {
        private string _name { get; set; }
        private string _password { get; set; }
        private List<Card> _deck { get; }
        private List<Card> _stack;
        public List<Card> Stack { get { return _stack; } }
        private int _coinpurse { get; set; }

        public User(string name, string password) {
            _name = name;
            _password = password;
            _deck = new List<Card>(30);
            _stack = new List<Card>();
            _coinpurse = 20;

        }
        public void sellCard(string nameOfCard, int amountOfCards)
        {
            // TODO: check if player has the card and the amount of cards he wants to sell
            for (int i = 0; i < amountOfCards; i++)
            {
                // TODO identify the card and remove it out of the stack
                //_stack.Remove();
            }
        }


    }
}

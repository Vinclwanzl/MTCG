using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MTCG
{
    public class User
    {
        private int _amountOfWins;

        private string _name;
        public string Name 
        {
            get { return _name; }
            set { _name = value; }
        }
        private string _password;
        
        protected string Password
        {
            get { return _password; }
            set { _password = value; }
        }
        private List<Card> _deck; 
        public List<Card> Deck { get { return _deck; } set { _deck = value; } }
        private List<Card> _stack;
        public List<Card> Stack { get { return _stack; } set { _stack = value; } }

        private int _coinPurse;
        public int CoinPurse { get; set; }

        public User(string name, string password) {
            _amountOfWins = 0;
            _name = name;
            _password = password;
            _stack = new List<Card>();
            _coinPurse = 20;
        }

        public bool MakeDeck()
        {
            _deck = new List<Card>(30);
            for (int i = 0; i < _deck.Count; i++)
            {

            }
            return true;
        }
        public bool LookForBattle()
        {
            if(_deck.Count == 30)
            {
                //look for game (server)
                return true;
            }
            return false;
        }
    }
}

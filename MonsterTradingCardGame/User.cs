using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    class User
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
        private CardCompendium _cardCompendium;
        public CardCompendium CardCompendium 
        { 
            get { return _cardCompendium; } 
            set { _cardCompendium = value; } 
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
        private int _coinPurse;
        public int CoinPurse
        {
            get { return _coinPurse; }
            set { _coinPurse = value; }
        }
        public User(string name, string password, bool newUser)
        { 
            _name = name;
            _password = password;
            _deck = new List<Card>();
            _amountOfWins = getAmountOfWins(name);
            _cardCompendium = getCardCompendium(name);
            
            if (newUser)
                _coinPurse = 20;
            else
                _coinPurse = getCoinPurse(name);
        }

        private int getCoinPurse(string name)
        {
            return 69;
        }

        private CardCompendium getCardCompendium(string name)
        {
            return new CardCompendium();
        }

        private int getAmountOfWins(string name)
        {
            return 12;
        }

        public void MakeStack()
        {
            int i = 0;
            while(i < 3)
            {
                int choice = -1;
                bool inputIsValid = false;

                Console.Write(_cardCompendium.ListCardCompendium());
                Console.Write("\nChoose your " + (i+1) +". Card by typing the Card Index of the chosen Card\n");

                
                while (Console.ReadKey().Key != ConsoleKey.Enter && !inputIsValid)
                {
                    string c = Console.ReadLine();
                    if (int.TryParse(c, out choice) && 0 <= choice && choice < _cardCompendium.CardList.Count)
                        inputIsValid = true;
                    else
                        Console.Write("\nWrong Input! Please try again\n");
                }

                if (_cardCompendium.CardList[choice] != null)
                {
                    Deck.Add(_cardCompendium.CardList[choice]);
                    _cardCompendium.CardList.Remove(_cardCompendium.CardList[choice]);
                    ++i;
                }
                else 
                {
                    Console.Write("\nERROR occured in makeStack() : Card exists in List, but is null\n");
                }
            }
        }
    }
}

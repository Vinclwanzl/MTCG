using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonsterTradingCardGame
{
    class CardCompendium
    {
        private List<int> _amountOfCardCopies;
        public List<int> AmountOfCardCopies { get { return _amountOfCardCopies; } }

        private List<Card> _cardList;
        public List<Card> CardList { get { return _cardList; } }
        public CardCompendium() 
        {
            _cardList = new List<Card>();
            _amountOfCardCopies = new List<int>();
        }

        /// <summary>
        /// Function for adding a Card to the Compendium
        /// </summary>
        /// <param name="card"></param>
        /// <returns>True, if the Card was added successfully and False´, if the Card </returns>
        public bool AddCardToCompendium(Card card)
        {
            if (card == null)
                return false;
            if (_cardList.Contains(card))
            {
                int index = _cardList.IndexOf(card);
                _amountOfCardCopies.Insert(index, _amountOfCardCopies.ElementAt(index) + 1);
            }
            else
            {
                _cardList.Add(card);
                _amountOfCardCopies.Add(0);
            }
            return true;
        }

        /// <summary>
        /// Function for getting all Cards with amount in Compendium
        /// </summary>
        /// <returns>string containing all Cards with amount in Compendium</returns>
        public string ListCardCompendium()
        {
            string output = "";
            if (_cardList.Count > 0)
            {
                Card currentCard;
                for (int i = 0; i < _cardList.Count; i++)
                {
                    currentCard = _cardList[i];
                    output += "( Card Index: " + i + ") " + currentCard.ToString() + " | Amount: " + _amountOfCardCopies.ElementAt(i) + '\n';
                }
            } 
            else
            {
                output = "\nYour Compendium is empty! Head to the shop and buy yourself some Cards!\n";
            }
            return output;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    internal class Player : IHealth
    {
        private Random _random = new Random();

        private readonly int _maxHealth = 10;
        private int _currentHealth;
        public int CurrentHealth { get { return _currentHealth; } }

        private int _maxMana;
        public void IncreaseMaxMana()
        {
            if (_maxMana < 10)
                _maxMana++;
        }

        private int _currentMana;
        public int CurrentMana
        {
            get { return _currentMana; }
            set { _currentMana += value; }
        }
        public void RefreshMana()
        {
            CurrentMana = _maxMana;
        }

        private MonsterField _monsterField;
        public MonsterField MonsterField
        {
            get { return _monsterField; }
            set { _monsterField = value; }
        }

        private List<Card> _deck;
        public List<Card> Deck { get { return _deck; } }

        private List<Card> _hand;
        public List<Card> Hand { get { return _hand; } }
        private int _drawDamage;

        public Player(User user)
        {
            _currentHealth = _maxHealth;
            _maxMana = 1;
            _monsterField = new MonsterField();
            _deck = user.Deck;
            _drawDamage = 1;

        }

        //returns true if Card is played
        //returns false if Card is not played
        public bool PlayCard(Card card, int target)
        {
            if (_hand.Contains(card) && card.ManaCost < _currentMana)
            {
                _currentMana -= card.ManaCost;
                return true;
            }
            return false;
        }
        public void DrawCard()
        {
            int deckSize = _deck.Count;
            if (deckSize == 0)
            {
                RecieveDamage(_drawDamage++);
                return;
            }
            if (Hand.Count > 10)
            {
                _deck.RemoveAt(_random.Next(0, --deckSize));
                return;
            }
            Hand.Add(_deck.ElementAt(_random.Next(0, --deckSize)));
            _deck.RemoveAt(_random.Next(0, --deckSize));
        }

        public void RecieveDamage(int amount)
        {
            if (0 >= _currentHealth - amount)
                _currentHealth = 0;
            else
                _currentHealth -= amount;
        }

        public void RecieceHealing(int amount)
        {
            if (_maxHealth <= _currentHealth + amount)
                _currentHealth = _maxHealth;
            else
                _currentHealth += amount;
        }
    }
}

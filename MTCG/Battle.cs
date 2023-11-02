using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using static System.Net.Mime.MediaTypeNames;

namespace MTCG
{
    internal class Battle
    {
        private Player _player1;
        private Player _player2;
        private int _numberOfTurns;
        private bool _endedTurn;
        public bool EndedTurn { set {  _endedTurn = value; } }

        public Battle(User user1, User user2) 
        {
            _player1 = new Player(user1);
            _player2 = new Player(user2);
            _numberOfTurns = 1;
            _endedTurn = false;
        }
        private void HourglassCallback(Object o)
        {
            _endedTurn = true;
        }
        public Player StartBattle()
        {
            // start turn 1; hour glass = 2 minutes
            Timer t = new Timer(HourglassCallback, null, 0, 120000);
            while (true)
            {
                // check who's turn it is
                if (_numberOfTurns % 2 == 1) 
                {
                    // only let user1 do stuff
                }
                else if(_numberOfTurns % 2 == 0)
                {
                    // only let user2 do stuff
                }

                // check if a player is dead
                if(_player1.CurrentHealth == 0)
                {
                    return _player2; // player2 won
                }
                if(_player2.CurrentHealth == 0)
                {
                    return _player1; // player2 won
                }

                if (_endedTurn)
                { 
                    _numberOfTurns++;
                    //prepare next turn for play2
                    if (_numberOfTurns % 2 == 1)
                    {
                        _player2.IncreaseMaxMana();
                        _player2.RefreshMana();
                            _player2.DrawCard();
                    }
                    // prepare next turn for play1
                    else if (_numberOfTurns % 2 == 0)
                    {
                        _player1.IncreaseMaxMana();
                        _player1.RefreshMana(); 
                        _player1.DrawCard();    
                    }
                    _endedTurn = false;

                    // turn the hourglass
                    t = new Timer(HourglassCallback, null, 0, 120000);
                }
            }
        }
        //TODO:make CardPlayHandler that figures out how cards react with oneanother
    }
}
  
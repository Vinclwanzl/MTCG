using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MonsterTradingCardGame
{
    public class Battle
    {

        private User _player1;
        public User Player1 { get { return _player1; } }
        private User _player2;
        public User Player2 { get { return _player2; } }

        private string _battleLog;
        public string BattleLog { get { return _battleLog; } }

        private int _roundNumber;
        public bool HasEnded { get; private set; }
        public string Winner { get; private set; }
        public Battle(User player1, User player2)
        {
            _player1 = player1;
            _player2 = player2;
            _roundNumber = 0;
            _battleLog = "";
            HasEnded = false;
            Winner = "";
        }

        /// <summary>
        /// Function that starts the Battle
        /// </summary>
        /// <returns>
        /// winner of battle or error message
        /// </returns>
        public string StartBattle()
        {
            if (_player1 == null ||
                _player2 == null)
            {
                HasEnded = true;

                if (_player1 == null &&
                    _player2 == null)
                {
                    return "both players are non existant";
                }

                else if (_player1 == null)
                {
                    return "player1 has vanished";
                }

                else if (_player2 == null)
                {
                    return "player2 has vanished";
                }
            }

            _battleLog = $"Battle betwenn {_player1.Name} and {_player2.Name} Started\n";

            Random rdm = new Random();

            int player1CardIndex, player2CardIndex;
            Buffs player1Buffs = new(), player2Buffs = new();
            Card cardOfPlayer1, cardOfPlayer2;

            double damageOfCard1, damageOfCard2;

            while (_roundNumber <= 100)
            {
                if (_player1.Deck.Count <= 0 ||
                    _player2.Deck.Count <= 0)
                {
                    if (_player1.Deck.Count <= 0)
                    {
                        recordRoundInLog(_player2.Name + " won the Battle");
                        HasEnded = true;
                        Winner = _player2.Name;
                        return "";
                    }
                    else
                    {
                        recordRoundInLog(_player1.Name + " won the Battle");
                        HasEnded = true;
                        Winner = _player1.Name;
                        return "";
                    }
                }

                ++_roundNumber;

                player1CardIndex = rdm.Next(0, (_player1.Deck.Count - 1));
                player2CardIndex = rdm.Next(0, (_player2.Deck.Count - 1));

                cardOfPlayer1 = _player1.Deck[player1CardIndex];
                cardOfPlayer2 = _player2.Deck[player2CardIndex];

                damageOfCard1 = HandleCard(Player1.Name, cardOfPlayer1, ref player1Buffs, cardOfPlayer2);
                damageOfCard2 = HandleCard(Player2.Name, cardOfPlayer2, ref player2Buffs, cardOfPlayer1);


                if (damageOfCard1 > damageOfCard2)
                {
                    recordRoundInLog(_player1.Name + " won this round");
                    _player1.Deck.Add(cardOfPlayer2);
                    _player2.Deck.RemoveAt(player2CardIndex);
                }
                else if (damageOfCard2 > damageOfCard1)
                {
                    recordRoundInLog(_player2.Name + " won this round");
                    _player2.Deck.Add(cardOfPlayer1);
                    _player1.Deck.RemoveAt(player1CardIndex);
                }
                else
                {
                    recordRoundInLog("The Round ended in a stalemate! Nothing happens");
                }
            }

            recordRoundInLog("The Battle took 100 rounds! it ended in a stalemate");
            HasEnded = true;
            Winner = "Nobody";
            return "";
        }

        private double HandleCard(string dinomancer, Card cardOfDinomancer, ref Buffs buffs, Card cardOfEnemy)
        {
            double damage = cardOfDinomancer.Damage;
            switch (cardOfDinomancer)
            {
                case Monster dino:
                    recordRoundInLog($"{dinomancer} summons {dino.Name} a {Enum.GetName(typeof(EDinoTypes), cardOfDinomancer.DinoType)} dino that deals {damage} damage");
                    if (buffs.BuffExists)
                    {
                        damage += AddBuffsToDamage(buffs, dino.DinoType, dinomancer);
                        recordRoundInLog($"{dinomancer}'s dino {dino.Name} now deals a total of {damage} damage");
                    }
                    break;
                case Spell spell:
                    string damageType = $" {Enum.GetName(typeof(EDinoTypes), spell.DinoType)} damage";
                    damage = ApplyTypesToDamage(spell.DinoType, cardOfDinomancer.DinoType, spell.Damage);
                    switch (spell)
                    {
                        case BuffSpell buffSpell:
                            recordRoundInLog($"{dinomancer} casts the Buff Spell {buffSpell.Name} dealing {buffSpell.Damage + damageType} and buffing the next Monster by {buffSpell.BuffAmount + damageType}");
                            buffs.AddBuff(spell.DinoType, buffSpell.BuffAmount);
                            break;
                        case TrapSpell trapspell:
                            if (cardOfEnemy is Spell)
                            {
                                if (trapspell.TrapTrigger == cardOfEnemy.DinoType)
                                {
                                    damage *= 2;
                                    recordRoundInLog($"{dinomancer} casts the Trap Spell {trapspell.Name}, which was successfully triggered, thus dealing {spell.Damage + damageType}");
                                }
                                else
                                {
                                    damage *= 0.5;
                                    recordRoundInLog($"{dinomancer} casts the Trap Spell {trapspell.Name}, which failed to triggered, thus dealing {spell.Damage + damageType}");
                                }
                            }
                            break;
                        default:
                            recordRoundInLog($"{dinomancer} casts the Spell {spell.Name} dealing {spell.Damage + damageType}");
                            break;
                    }
                    break;

                default:
                    Console.WriteLine("ERROR: CARD is neither MONSTER nor SPELL ");
                    break;
            }
            return damage;
        }

        private double AddBuffsToDamage(Buffs buffs, EDinoTypes targetsType, string dinomancer)
        {
            double buff = 0;
            for (int i = 0; i < buffs.BuffTypes.Count; i++)
            {
                buff += ApplyTypesToDamage(buffs.BuffTypes[i],
                                           targetsType,
                                           buffs.BuffAmount[i]);
                recordRoundInLog($"{dinomancer} buffs their dino by {buffs.BuffAmount[i]} {Enum.GetName(typeof(EDinoTypes), buffs.BuffTypes[i])} damage");
            }
            buffs.RemoveBuffs();
            return buff;
        }

        private double ApplyTypesToDamage(EDinoTypes origin, EDinoTypes target, int amount)
        {

            switch (origin)
            {
                case EDinoTypes.AQUATIC:
                    // weak against 
                    if (target == EDinoTypes.VOLCANIC)
                    {
                        return (double)amount * 0.5;
                    }
                    // strong against
                    else if (target == EDinoTypes.AERIAL)
                    {
                        return (double)amount * 2;
                    }
                    break;
                case EDinoTypes.TERRESTRIAL:
                    // weak against 
                    if (target == EDinoTypes.AERIAL)
                    {
                        return (double)amount * 0.5;
                    }
                    // strong against
                    else if (target == EDinoTypes.VOLCANIC)
                    {
                        return (double)amount * 2;
                    }
                    break;
                case EDinoTypes.AERIAL:
                    // weak against 
                    if (target == EDinoTypes.AQUATIC)
                    {
                        return (double)amount * 0.5;
                    }
                    // strong against
                    else if (target == EDinoTypes.TERRESTRIAL)
                    {
                        return (double)amount * 2;
                    }
                    break;
                case EDinoTypes.VOLCANIC:
                    // weak against 
                    if (target == EDinoTypes.TERRESTRIAL)
                    {
                        return (double)amount * 0.5;
                    }
                    // strong against
                    else if (target == EDinoTypes.AQUATIC)
                    {
                        return (double)amount * 2;
                    }
                    break;
                default:
                    // this should not be possible
                    break;
            }
            return (double)amount;
        }
        private void recordRoundInLog(string logContent)
        {
            string round = $"Round: {_roundNumber}\t| {logContent}.\n";
            Console.WriteLine(round);
            _battleLog += round;
        }
    }

}

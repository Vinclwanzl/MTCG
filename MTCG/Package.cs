using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    class Package
    {
        private List<Card> _content;
        private List<Card> Content { get { return _content; } }
        public Package() 
        {
            //demo package for testing
            _content = new List<Card> 
            { 
                new Monster(ElementalTypes.FIRE, 1, "FeuFeu", 1, 2, 1), 
                new Monster(ElementalTypes.EARTH, 1, "Rat", 1, 1, 1), 
                new Monster(ElementalTypes.AIR, 1, "Burb", 1, 2, 1), 
                new Spell(ElementalTypes.WATER, 1, 2, "Juice Up", 1), 
                new Spell(ElementalTypes.EARTH, 1, "Sta am Schädl", 2, 1) 
            };   
            // TODO make package with random Cards
        }


        public List<Card> chooseACardToToss(string name) 
        {


            return _content;
        }
        
    }
}

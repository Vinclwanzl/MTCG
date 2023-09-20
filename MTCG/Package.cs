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
            _content = new List<Card> { new Monster(ElementalTypes.FIRE, "FeuFeu", 1, 2, 1), 
                                        new Monster(ElementalTypes.EARTH, "Rat", 1, 1, 1), 
                                        new Monster(ElementalTypes.AIR, "Burb", 1, 2, 1), 
                                        new Spell(ElementalTypes.WATER, "Watergun", 1), 
                                        new Spell(ElementalTypes.EARTH, "Sta am Schädl", 2) };   
            // TODO make package with random Cards
        }


        public List<Card> chooseACardToToss(string name) 
        {


            return _content;
        }
        
    }
}

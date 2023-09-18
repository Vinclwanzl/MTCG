using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    
    interface IHealth
    {   
        private void recieveDamage(int amount) { }
        private void recieceHealing(int amount) { }
    }
}

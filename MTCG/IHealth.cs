using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    
    interface IHealth
    {
        int CurrentHealth { get; set; }
        void recieveDamage(int amount);
        void recieceHealing(int amount);
    }
}

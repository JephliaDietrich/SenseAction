using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseAction.Core
{
    public class Signal
    {
        public float Intencity {  get; set; }
        public string Type { get; set; }

        public Signal(float intencity, string type)
        {
            Intencity = intencity;
            Type = type;

        }
    }
}

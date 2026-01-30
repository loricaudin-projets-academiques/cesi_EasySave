using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.Core.Services.Observer
{
    internal interface Observable
    {
        void Attach(Observer observer);
        void Detach(Observer observer);
        void Notify();
    }
}

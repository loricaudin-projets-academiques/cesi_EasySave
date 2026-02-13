using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySave.VM
{
    public interface IDialogService
    {
        void ShowAbout();
        bool Confirm(string message, string title);
        void ShowError(string message, string title = "Erreur");
        void ShowInfo(string message, string title = "Information");
    }

}

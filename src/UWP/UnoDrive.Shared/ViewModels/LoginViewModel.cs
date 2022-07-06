using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;

namespace UnoDrive.ViewModels
{
    public class LoginViewModel
    {
        public LoginViewModel()
        {
            Login = new RelayCommand(OnLogin);
        }

        public ICommand Login { get; }

        void OnLogin()
        {
            System.Console.WriteLine("Perform login");
        }
    }
}

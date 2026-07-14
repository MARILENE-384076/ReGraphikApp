using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ReGraphik.ViewModels
{
    /// <summary>
    /// Classe base para todos os ViewModels, implementando a interface INotifyPropertyChanged
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
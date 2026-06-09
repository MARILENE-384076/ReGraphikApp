using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReGraphik.ViewModels
{
    public class UsuarioViewModel : BaseViewModel
    {
        private string _imgFoto; 
        public string ImgFoto
        {
            get => _imgFoto;
            set { _imgFoto = value; OnPropertyChanged(); }
        }

        public ICommand SelecionarFotoCommand { get; }

        public UsuarioViewModel()
        {
            SelecionarFotoCommand = new RelayCommand(SelecionarFoto);
        }

        public void SelecionarFoto()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Imagens (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Selecione um arquivo para anexar"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                ImgFoto = openFileDialog.SafeFileName;
            }
        }
    }

}

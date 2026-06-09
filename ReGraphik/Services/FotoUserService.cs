using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ReGraphik.Services
{
    public static class FotoUserService
    {
        private static ImageSource _foto;

        // Evento que avisa as ViewModels que a foto global mudou
        public static event EventHandler FotoMudou;

        public static ImageSource Foto
        {
            get
            {
                // Se a foto global ainda for nula, carrega o avatar padrão do projeto
                if (_foto == null)
                {
                    var bitmap = new BitmapImage(new Uri("pack://application:,,,/ReGraphik;component/Assets/avatar-default.png", UriKind.Absolute));
                    bitmap.Freeze();
                    _foto = bitmap;
                }
                return _foto;
            }
            set
            {
                if (_foto != value)
                {
                    _foto = value;
                    // Dispara o aviso para quem estiver escutando (ex: Dashboard)
                    FotoMudou?.Invoke(null, EventArgs.Empty);
                }
            }
        }
    }
}


using System.Windows;
using System.Windows.Threading;

namespace ReGraphik.Views
{
    /// <summary>
    /// Toast de notificação de nova mensagem.
    /// Aparece no canto inferior direito da tela, some automaticamente
    /// após 5 segundos, e dispara o evento Clicado se o usuário clicar.
    /// </summary>
    public partial class ChatToastWindow : Window
    {
        private readonly DispatcherTimer _timerFechamento;

        /// <summary>
        /// Disparado quando o usuário clica no toast para abrir o chat.
        /// </summary>
        public event Action? Clicado;

        public ChatToastWindow(string remetenteNome, string textoPreview)
        {
            InitializeComponent();

            TxtRemetente.Text = remetenteNome;
            TxtPreview.Text = textoPreview;

            // Posiciona no canto inferior direito da tela de trabalho
            var area = SystemParameters.WorkArea;
            Left = area.Right - Width - 16;
            Top = area.Bottom - Height - 16;

            // Fecha automaticamente após 5 segundos
            _timerFechamento = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _timerFechamento.Tick += (_, _) => FecharToast();
            _timerFechamento.Start();
        }

        private void Toast_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _timerFechamento.Stop();
            Clicado?.Invoke();
            Close();
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // impede que o clique propague para Toast_Click
            FecharToast();
        }

        private void FecharToast()
        {
            _timerFechamento.Stop();
            Close();
        }
    }
}
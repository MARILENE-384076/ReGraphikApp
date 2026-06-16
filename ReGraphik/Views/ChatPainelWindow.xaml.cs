using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;

namespace ReGraphik.Views
{
    public partial class ChatPainelWindow : Window
    {
        public ChatPainelWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ViewModels.ChatViewModel vm)
            {
                vm.Mensagens.CollectionChanged += Mensagens_CollectionChanged;
            }
        }

        private void Mensagens_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ScrollMensagens.ScrollToBottom();
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }

        // Permite arrastar a janela pelo cabeçalho (WindowStyle=None)
        private void Cabecalho_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            Hide(); // Oculta sem destruir — reutilizável
        }
    }
}

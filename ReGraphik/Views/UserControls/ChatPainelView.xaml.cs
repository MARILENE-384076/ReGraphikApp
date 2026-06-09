using System.Collections.Specialized;
using System.Windows.Controls;

namespace ReGraphik.Views.UserControls
{
    /// <summary>
    /// Code-behind do painel de chat.
    /// Responsável por rolar automaticamente para a última mensagem ao receber novas.
    /// </summary>
    public partial class ChatPainelView : UserControl
    {
        public ChatPainelView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ViewModels.ChatViewModel vm)
            {
                vm.Mensagens.CollectionChanged += Mensagens_CollectionChanged;
            }
        }

        private void Mensagens_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Rola para a última mensagem quando novas chegam
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    ScrollMensagens.ScrollToBottom();
                }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
        }
    }
}

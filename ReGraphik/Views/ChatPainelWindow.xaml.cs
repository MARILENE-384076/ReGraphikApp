using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ReGraphik.Views
{
    /// <summary>
    /// Interaction logic for ChatPainelWindow.xaml
    /// </summary>
    public partial class ChatPainelWindow : Window
    {
        public ChatPainelWindow()
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

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            Hide(); // Oculta sem destruir — reutilizavel
        }

    }
}

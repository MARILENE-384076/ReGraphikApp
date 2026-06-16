using System.Windows;
using ReGraphik.Models;
namespace ReGraphik.Views
{
    public partial class SugestaoResiduoWindow : Window
    {
        /// <summary>
        /// Construtor padrão 
        /// </summary>
        public SugestaoResiduoWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Construtor que aceita 1 argumento 
        /// </summary>
        /// <param name="residuo"></param>
        public SugestaoResiduoWindow(Residuo residuo) : this()
        {
        }

        /// <summary>
        /// Evento que faz o botão Fechar do XAML funcionar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); /// Fecha a janela de forma segura
        }
    }
}
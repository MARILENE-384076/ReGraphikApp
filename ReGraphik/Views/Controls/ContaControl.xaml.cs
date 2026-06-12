using ReGraphik.Models;
using ReGraphik.Services;
using ReGraphik.Services.Interface;
using ReGraphik.ViewModels;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ReGraphik.Views.Controls
{
    /// <summary>
    /// Define a interação lógica para ContaControl.xaml
    /// </summary>
    public partial class ContaControl : UserControl
    {
        public ContaControl(Usuario usuario)
        {
            InitializeComponent();

            // Passando a implementação real do serviço para a ViewModel
            IAutorizarService autorizarService = new AutorizarService();
            DataContext = new ContaViewModel(usuario, autorizarService);
        }
    }
}
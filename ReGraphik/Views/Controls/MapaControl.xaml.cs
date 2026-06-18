using Microsoft.Win32;
using ReGraphik.Models;
using ReGraphik.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReGraphik.Views.Controls
{
    public partial class MapaControl : UserControl
    {
        private readonly MapaViewModel _viewModel;

        public MapaControl()
        {
            DefinirEmulacaoNavegador();

            InitializeComponent();
            _viewModel = new MapaViewModel();
            DataContext = _viewModel;

            _viewModel.SolicitouFocoNoMapa += ViewModel_SolicitouFocoNoMapa;
            _viewModel.SolicitouHtmlMapa += ViewModel_SolicitouHtmlMapa;

            /// Atribui a ponte COM para comunicação segura Javascript -> C#
            MapaBrowser.ObjectForScripting = new PonteScriptMapa(this);

            this.Loaded += MapaControl_Loaded;
        }

        private void MapaControl_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel.InicializarMapaLivre();
        }

        private void ViewModel_SolicitouHtmlMapa(string conteudoHtml)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    if (MapaBrowser != null)
                    {
                        /// Desativa diálogos chados de erros de script nativos do componente WebBrowser
                        dynamic activeX = MapaBrowser.GetType().InvokeMember("ActiveXInstance",
                            System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                            null, MapaBrowser, null);
                        if (activeX != null) activeX.Silent = true;

                        MapaBrowser.NavigateToString(conteudoHtml);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Erro ao renderizar HTML: " + ex.Message);
                }
            });
        }

        private void ViewModel_SolicitouFocoNoMapa(int indice)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    if (MapaBrowser != null)
                    {
                        MapaBrowser.InvokeScript("centralizarPonto", indice);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Erro ao focar no marcador: " + ex.Message);
                }
            });
        }

        private void ListaPontos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaPontos.SelectedItem is PontosColeta pontoSelecionado)
            {
                _viewModel.FocarNoPonto(pontoSelecionado);
            }
        }

        private void MapaBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            _viewModel.NotificarMapaCarregado();

            if (MapaPlaceholder != null)
            {
                MapaPlaceholder.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        private static void DefinirEmulacaoNavegador()
        {
            try
            {
                string nomeProcesso = System.IO.Path.GetFileName(Environment.GetCommandLineArgs()[0]);
                if (nomeProcesso.EndsWith(".vshost.exe")) nomeProcesso = nomeProcesso.Replace(".vshost.exe", ".exe");

                using (var chave = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true))
                {
                    if (chave != null)
                    {
                        chave.SetValue(nomeProcesso, 11001, RegistryValueKind.DWord);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// Classe de Interoperabilidade COM para ponte de comunicação Javascript para WPF
        /// </summary>
        [ComVisible(true)]
        public class PonteScriptMapa
        {
            private readonly MapaControl _controle;

            public PonteScriptMapa(MapaControl controle)
            {
                _controle = controle;
            }

            public void NotificarMovimentoMapa(double swLat, double swLng, double neLat, double neLng)
            {
                Task.Run(async () =>
                {
                    if (_controle._viewModel != null)
                    {
                        /// Busca os dados em Background
                        await _controle._viewModel.BuscarPorCoordenadasAsync(swLat, swLng, neLat, neLng);

                        /// Transforma a lista em JSON seguro
                        string novoJson = _controle._viewModel.GerarJsonMarcadores(_controle._viewModel.PontosAtuais.ToList());

                        /// Empurra os marcadores direto na função Javascript existente, sem recarregar e sem dar erros
                        _controle.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                if (_controle.MapaBrowser != null)
                                {
                                    _controle.MapaBrowser.InvokeScript("renderizarPontos", new object[] { novoJson });
                                }
                            }
                            catch { }
                        });
                    }
                });
            }
        }
    }
}
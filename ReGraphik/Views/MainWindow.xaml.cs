using System;
using System.Collections.Generic;
using System.Globalization; // GARANTE QUE LATITUDE/LONGITUDE USEM PONTO (.) E NÃO VÍRGULA (,)
using System.Windows;
using System.Windows.Controls;
using ApiRestReGraphik.Models;
using ReGraphik.Services;

namespace ReGraphik.Views
{
    public partial class MainWindow : Window
    {
        private readonly GooglePlacesService _placesService = new GooglePlacesService();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Inicializa o WebView2 da Microsoft de forma assíncrona básica
                if (NavegadorMapa != null)
                {
                    await NavegadorMapa.EnsureCoreWebView2Async(null);
                    CarregarMapaNoComponente(new List<PontosColeta>(), -19.9173, -43.9345, 12);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Erro ao carregar mapa: " + ex.Message);
            }
        }

        private async void BtnBuscarMapa_Click(object sender, RoutedEventArgs e)
        {
            string city = TxtCidadeBusca.Text;
            string material = "Ecoponto reciclagem";

            if (CboMaterialBusca.SelectedItem is ComboBoxItem selectedItem)
            {
                material = selectedItem.Content.ToString() ?? material;
            }

            // Consome o serviço simples que criamos para o Brasil todo
            List<PontosColeta> postos = await _placesService.BuscarPostosNoBrasilAsync(city, material);

            // Coordenadas padrão de BH caso digite uma região não mapeada
            double latCenter = -19.9173;
            double lngCenter = -43.9345;

            string buscaLower = city.ToLower();
            if (buscaLower.Contains("são paulo") || buscaLower.Contains("sp"))
            {
                latCenter = -23.5505;
                lngCenter = -46.6333;
            }
            else if (buscaLower.Contains("rio") || buscaLower.Contains("rj"))
            {
                latCenter = -22.9068;
                lngCenter = -43.1729;
            }

            CarregarMapaNoComponente(postos, latCenter, lngCenter, 12);
        }

        private void CarregarMapaNoComponente(List<PontosColeta> pontos, double centroLat, double centroLng, int zoom)
        {
            // INSIRA AQUI A SUA CHAVE DA API GERADA NO GOOGLE CLOUD
            string apiKey = "SUA_CHAVE_DO_GOOGLE_AQUI";

            string scriptsMarcadores = "";
            Random randomizer = new Random();

            foreach (var ponto in pontos)
            {
                double latOffset = (randomizer.NextDouble() - 0.5) * 0.05;
                double lngOffset = (randomizer.NextDouble() - 0.5) * 0.05;

                double markerLat = centroLat + latOffset;
                double markerLng = centroLng + lngOffset;

                string labelLimpo = ponto.NomePonto.Replace("'", "\\'");

                // Usando CultureInfo.InvariantCulture para forçar o formato numérico americano com ponto decimal
                string sLat = markerLat.ToString(CultureInfo.InvariantCulture);
                string sLng = markerLng.ToString(CultureInfo.InvariantCulture);

                scriptsMarcadores += $@"
                    new google.maps.Marker({{
                        position: {{ lat: {sLat}, lng: {sLng} }},
                        map: map,
                        title: '{labelLimpo}'
                    }});
                ";
            }

            // Conversão segura do centro do mapa para strings JavaScript
            string sCentroLat = centroLat.ToString(CultureInfo.InvariantCulture);
            string sCentroLng = centroLng.ToString(CultureInfo.InvariantCulture);

            string html = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    html, body, #mapContainer {{ height: 100%; margin: 0; padding: 0; }}
                </style>
                <script src='https://maps.googleapis.com/maps/api/js?key={apiKey}'></script>
                <script>
                    function initWpfMap() {{
                        var centerPos = {{ lat: {sCentroLat}, lng: {sCentroLng} }};
                        var map = new google.maps.Map(document.getElementById('mapContainer'), {{
                            zoom: {zoom},
                            center: centerPos
                        }});
                        {scriptsMarcadores}
                    }}
                    window.onload = initWpfMap;
                </script>
            </head>
            <body>
                <div id='mapContainer'></div>
            </body>
            </html>";

            if (NavegadorMapa != null)
            {
                NavegadorMapa.NavigateToString(html);
            }
        }

        // --- SEUS MÉTODOS ORIGINAIS INALTERADOS ---
        private void FecharAbas()
        {
            DashboardGrid.Visibility = Visibility.Collapsed;
            ResiduosGrid.Visibility = Visibility.Collapsed;
            PontosGrid.Visibility = Visibility.Collapsed;
            MapaGrid.Visibility = Visibility.Collapsed;
            RelatoriosGrid.Visibility = Visibility.Collapsed;
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            FecharAbas();
            DashboardGrid.Visibility = Visibility.Visible;
        }

        private void Residuos_Click(object sender, RoutedEventArgs e)
        {
            FecharAbas();
            ResiduosGrid.Visibility = Visibility.Visible;
        }

        private void Pontos_Click(object sender, RoutedEventArgs e)
        {
            FecharAbas();
            PontosGrid.Visibility = Visibility.Visible;
        }

        private void Mapa_Click(object sender, RoutedEventArgs e)
        {
            FecharAbas();
            MapaGrid.Visibility = Visibility.Visible;
        }

        private void Relatorios_Click(object sender, RoutedEventArgs e)
        {
            FecharAbas();
            RelatoriosGrid.Visibility = Visibility.Visible;
        }
    }
}
using System.Windows;
using System.Windows.Input;
using Trabalho.OS.Views;

namespace Trabalho.OS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Inicia 
            MainContent.Content = new OrdemServicoView();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void BtnOrcamento_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new OrcamentoView();
        }

        private void BtnOrdemServico_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new OrdemServicoView();
        }

        private void BtnCaixa_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CaixaView();
        }

        private void BtnDesligar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Deseja realmente fechar o sistema?", "Desligar", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
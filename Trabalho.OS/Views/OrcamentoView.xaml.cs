using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Trabalho.OS.Views
{
    public partial class OrcamentoView : UserControl
    {
        private static readonly CultureInfo CulturaPtBr = new CultureInfo("pt-BR");

        // tamanho de fonte inicial 
        private const double FonteInicial = 13.0;
        private const double FonteMinima = 8.0;

        public OrcamentoView()
        {
            InitializeComponent();
            TxtData.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private bool TentarObterValores(out decimal peca, out decimal maoObra, out string mensagemErro)
        {
            peca = 0m;
            maoObra = 0m;
            mensagemErro = string.Empty;

            if (!ParseMoedaPositivaOuZero(TxtValorPeca.Text, out peca))
            {
                mensagemErro = "Informe um valor válido (maior ou igual a zero) para a peça.";
                return false;
            }

            if (!ParseMoedaPositivaOuZero(TxtMaoObra.Text, out maoObra))
            {
                mensagemErro = "Informe um valor válido (maior ou igual a zero) para a mão de obra.";
                return false;
            }

            return true;
        }

        private static bool ParseMoedaPositivaOuZero(string texto, out decimal valor)
        {
            valor = 0m;
            string limpo = (texto ?? string.Empty).Trim().Replace("R$", "").Trim();

            if (string.IsNullOrEmpty(limpo))
                return true;

            bool ok = decimal.TryParse(limpo, NumberStyles.Number, CulturaPtBr, out valor);
            if (!ok && limpo.Contains(".") && !limpo.Contains(","))
            {
                ok = decimal.TryParse(limpo, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
            }

            return ok && valor >= 0m;
        }

        private void CalcularTotal(object sender, TextChangedEventArgs e)
        {
            if (LblTotalOrcamento == null)
                return;

            if (!TentarObterValores(out decimal peca, out decimal maoObra, out _))
            {
                LblTotalOrcamento.Text = "Total do Orçamento: valor inválido";
                return;
            }

            LblTotalOrcamento.Text = $"Total do Orçamento: {(peca + maoObra).ToString("C2", CulturaPtBr)}";
        }

        private void BtnImprimirOrcamento_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtClienteAparelho.Text))
            {
                MessageBox.Show("Informe o Cliente / Aparelho antes de imprimir.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtClienteAparelho.Focus();
                return;
            }

            if (!TentarObterValores(out decimal peca, out decimal maoObra, out string erro))
            {
                MessageBox.Show(erro, "Valor Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal total = peca + maoObra;

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() != true)
                return;

            // A4 retrato a área imprimível se disponível
            double pageWidth = printDialog.PrintableAreaWidth > 0 ? printDialog.PrintableAreaWidth : 794;
            double pageHeight = printDialog.PrintableAreaHeight > 0 ? printDialog.PrintableAreaHeight : 1122;

            double fontSizeBase = FonteInicial;
            FrameworkElement pagina;
            bool coube;

            // correção altomatica na fonte 
            while (true)
            {
                pagina = ConstruirPaginaA4(pageWidth, pageHeight, fontSizeBase, peca, maoObra, total, out coube);
                if (coube || fontSizeBase <= FonteMinima)
                    break;
                fontSizeBase -= 0.5;
            }

            string nomeSeguro = SanitizarNomeArquivo(TxtClienteAparelho.Text);
            printDialog.PrintVisual(pagina, $"Orcamento_{nomeSeguro}");
        }

        private FrameworkElement ConstruirPaginaA4(
            double pageWidth,
            double pageHeight,
            double baseFontSize,
            decimal peca,
            decimal maoObra,
            decimal total,
            out bool coube)
        {
            Grid pagina = new Grid
            {
                Width = pageWidth,
                Height = pageHeight,
                Background = Brushes.White
            };
            pagina.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            pagina.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            //  títulos 1 e2
            StackPanel viaCima = CriarViaOrcamento("VIA 1", baseFontSize, peca, maoObra, total);
            StackPanel viaBaixo = CriarViaOrcamento("VIA 2", baseFontSize, peca, maoObra, total);

            viaCima.Margin = new Thickness(30, 16, 30, 16);
            viaBaixo.Margin = new Thickness(30, 16, 30, 16);
            viaCima.VerticalAlignment = VerticalAlignment.Top;
            viaBaixo.VerticalAlignment = VerticalAlignment.Top;

            Grid.SetRow(viaCima, 0);
            Grid.SetRow(viaBaixo, 1);
            pagina.Children.Add(viaCima);
            pagina.Children.Add(viaBaixo);

            // corte no centro  da folha
            Grid corte = new Grid { VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRowSpan(corte, 2);

            corte.Children.Add(new System.Windows.Shapes.Line
            {
                X1 = 0,
                X2 = 1,
                Stretch = Stretch.Fill,
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 4, 4 },
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0)
            });
            corte.Children.Add(new TextBlock
            {
                Text = "CORTE AQUI",
                FontSize = 9,
                Foreground = Brushes.Gray,
                Background = Brushes.White,
                Padding = new Thickness(8, 0, 8, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });
            pagina.Children.Add(corte);

            //  cada via na sua metade da folha
            
            double alturaMetade = pageHeight / 2.0;
            Size medida = new Size(pageWidth, double.PositiveInfinity);

            viaCima.Measure(medida);
            viaBaixo.Measure(medida);
            coube = viaCima.DesiredSize.Height <= alturaMetade - 8 &&
                    viaBaixo.DesiredSize.Height <= alturaMetade - 8;

            pagina.Measure(new Size(pageWidth, pageHeight));
            pagina.Arrange(new Rect(0, 0, pageWidth, pageHeight));
            pagina.UpdateLayout();

            return pagina;
        }

        private StackPanel CriarViaOrcamento(string tituloVia, double baseFontSize, decimal peca, decimal maoObra, decimal total)
        {
            StackPanel via = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            TextElement.SetFontFamily(via, new FontFamily("Segoe UI"));
            TextElement.SetForeground(via, Brushes.Black);

            string data = string.IsNullOrWhiteSpace(TxtData.Text)
                ? DateTime.Now.ToString("dd/MM/yyyy")
                : TxtData.Text.Trim();

            // cabeçalho
            via.Children.Add(new TextBlock
            {
                Text = $"TECKY CELL (027 98837-2300) — ORÇAMENTO | {tituloVia}",
                FontSize = baseFontSize + 1.5,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });

            // dados
            TextBlock dados = new TextBlock
            {
                FontSize = baseFontSize,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = baseFontSize + 7.0,
                Margin = new Thickness(0, 0, 0, 8)
            };

            dados.Inlines.Add(new Bold(new Run("Data: ")));
            dados.Inlines.Add(new Run($"{data}    "));
            dados.Inlines.Add(new Bold(new Run("Contato: ")));
            dados.Inlines.Add(new Run(LimitarTexto(TxtContato.Text, 25)));
            dados.Inlines.Add(new LineBreak());

            dados.Inlines.Add(new Bold(new Run("Cliente / Aparelho: ")));
            dados.Inlines.Add(new Run(LimitarTexto(TxtClienteAparelho.Text, 70)));
            dados.Inlines.Add(new LineBreak());

            dados.Inlines.Add(new Bold(new Run("Descrição: ")));
            dados.Inlines.Add(new Run(LimitarTexto(TxtDescricaoOrc.Text, 160)));
            dados.Inlines.Add(new LineBreak());

            dados.Inlines.Add(new Bold(new Run("Valor da Peça: ")));
            dados.Inlines.Add(new Run($"{peca.ToString("C2", CulturaPtBr)}    "));
            dados.Inlines.Add(new Bold(new Run("Mão de Obra: ")));
            dados.Inlines.Add(new Run(maoObra.ToString("C2", CulturaPtBr)));

            via.Children.Add(dados);

            // total
            via.Children.Add(new TextBlock
            {
                Text = $"VALOR TOTAL: {total.ToString("C2", CulturaPtBr)}",
                FontSize = baseFontSize + 2.0,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 4, 0, 26)
            });

            // assinaturas
            Grid assinaturas = new Grid { Margin = new Thickness(0, 4, 0, 0) };
            assinaturas.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            assinaturas.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            assinaturas.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            Border CriarCampoAssinatura(string rotulo) => new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Child = new TextBlock
                {
                    Text = rotulo,
                    FontSize = Math.Max(8.0, baseFontSize - 1.0),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 3, 0, 0)
                }
            };

            Border assCliente = CriarCampoAssinatura("Assinatura do Cliente");
            Border assLojista = CriarCampoAssinatura("Assinatura do Lojista");
            Grid.SetColumn(assCliente, 0);
            Grid.SetColumn(assLojista, 2);
            assinaturas.Children.Add(assCliente);
            assinaturas.Children.Add(assLojista);

            via.Children.Add(assinaturas);

            return via;
        }

        private static string LimitarTexto(string texto, int maxCaracteres)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            string normalizado = texto.Replace("\r\n", " ").Replace("\n", " ").Trim();
            if (normalizado.Length <= maxCaracteres)
                return normalizado;

            return normalizado.Substring(0, maxCaracteres - 1).TrimEnd() + "…";
        }

        private static string SanitizarNomeArquivo(string entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada))
                return "SemNome";

            char[] invalidos = Path.GetInvalidFileNameChars();
            string limpo = new string(entrada.Trim().Where(ch => !invalidos.Contains(ch)).ToArray());
            return string.IsNullOrWhiteSpace(limpo) ? "SemNome" : limpo;
        }
    }
}
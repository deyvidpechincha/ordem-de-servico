using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Trabalho.OS.Views
{
    public partial class OrdemServicoView : UserControl
    {
        private static readonly string[] TermosGarantia =
        {
            "1 - Garantia de 90 dias.",
            "2 - Peças cobertas SOMENTE contra defeito de fabricação.",
            "3 - Não cobre mau uso, quedas, líquidos ou desgaste natural.",
            "4 - Produtos e peças testados previamente.",
            "5 - Não retirados em 60 dias poderão ser vendidos para custeio."
        };

        public OrdemServicoView()
        {
            InitializeComponent();
            TxtData.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNome.Text))
            {
                MessageBox.Show("Informe o nome do cliente antes de imprimir.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtNome.Focus();
                return;
            }

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // A4 padrão em DIUs: 794 x 1122; usa área imprimível se disponível
                double pageWidth = printDialog.PrintableAreaWidth > 0 ? printDialog.PrintableAreaWidth : 794;
                double pageHeight = printDialog.PrintableAreaHeight > 0 ? printDialog.PrintableAreaHeight : 1122;

                Thickness padding = new Thickness(24, 14, 24, 14);
                double larguraUtil = Math.Max(200, pageWidth - padding.Left - padding.Right);

                double fontSizeBase = 9.5;
                FlowDocument doc = ConstruirDocumentoA4(pageWidth, pageHeight, padding, larguraUtil, fontSizeBase);

                IDocumentPaginatorSource idpSource = doc;
                DocumentPaginator paginator = idpSource.DocumentPaginator;
                paginator.PageSize = new Size(pageWidth, pageHeight);
                paginator.ComputePageCount();

                // Se ultrapassar 1 página, reduz gradualmente a fonte até caber na A4
                while (paginator.PageCount > 1 && fontSizeBase > 7.0)
                {
                    fontSizeBase -= 0.5;
                    doc = ConstruirDocumentoA4(pageWidth, pageHeight, padding, larguraUtil, fontSizeBase);
                    idpSource = doc;
                    paginator = idpSource.DocumentPaginator;
                    paginator.PageSize = new Size(pageWidth, pageHeight);
                    paginator.ComputePageCount();
                }

                string nomeSeguro = SanitizarNomeArquivo(TxtNome.Text);
                printDialog.PrintDocument(paginator, $"OS_{nomeSeguro}");
            }
        }

        private FlowDocument ConstruirDocumentoA4(
            double pageWidth,
            double pageHeight,
            Thickness padding,
            double larguraUtil,
            double baseFontSize)
        {
            FlowDocument doc = new FlowDocument
            {
                PageWidth = pageWidth,
                PageHeight = pageHeight,
                PagePadding = padding,
                ColumnWidth = larguraUtil,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = baseFontSize
            };

            doc.Blocks.Add(CriarViaOS("VIA DO LOJISTA", baseFontSize));

            Paragraph corte = new Paragraph(new Run("- - - - - - - - - - - - - - - - - - - - -  CORTE AQUI  - - - - - - - - - - - - - - - - - - - - -"))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 4, 0, 4),
                Foreground = Brushes.Gray,
                FontSize = Math.Max(7.0, baseFontSize - 1.5),
                LineHeight = Math.Max(9.0, baseFontSize + 1.0)
            };
            doc.Blocks.Add(corte);

            doc.Blocks.Add(CriarViaOS("VIA DO CLIENTE", baseFontSize));

            return doc;
        }

        private Section CriarViaOS(string tituloVia, double baseFontSize)
        {
            Section sec = new Section
            {
                Margin = new Thickness(0)
            };

            string defeito = LimitarTexto(TxtDefeito.Text, 140);
            string servico = LimitarTexto(TxtServico.Text, 120);
            string valor = string.IsNullOrWhiteSpace(TxtValor.Text) ? "A combinar" : TxtValor.Text.Trim();

            Paragraph header = new Paragraph(new Run($"TECKY CELL (027 98837-2300) — O.S. | {tituloVia}"))
            {
                FontSize = baseFontSize + 1.0,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 3),
                LineHeight = baseFontSize + 4.0
            };
            sec.Blocks.Add(header);

            Paragraph dados = new Paragraph
            {
                FontSize = baseFontSize,
                LineHeight = baseFontSize + 3.5,
                Margin = new Thickness(0, 0, 0, 3)
            };

            dados.Inlines.Add(new Bold(new Run("Data: ")));
            dados.Inlines.Add(new Run($"{TxtData.Text}    "));
            dados.Inlines.Add(new Bold(new Run("Cliente: ")));
            dados.Inlines.Add(new Run($"{LimitarTexto(TxtNome.Text, 45)}    "));
            dados.Inlines.Add(new Bold(new Run("Contato: ")));
            dados.Inlines.Add(new Run($"{LimitarTexto(TxtContato.Text, 25)}\n"));

            dados.Inlines.Add(new Bold(new Run("Endereço: ")));
            dados.Inlines.Add(new Run($"{LimitarTexto(TxtEndereco.Text, 75)}\n"));

            dados.Inlines.Add(new Bold(new Run("Modelo: ")));
            dados.Inlines.Add(new Run($"{LimitarTexto(TxtModelo.Text, 35)}    "));
            dados.Inlines.Add(new Bold(new Run("IMEI/Série: ")));
            dados.Inlines.Add(new Run($"{LimitarTexto(TxtImei.Text, 30)}    "));
            dados.Inlines.Add(new Bold(new Run("Valor (R$): ")));
            dados.Inlines.Add(new Run($"{valor}\n"));

            dados.Inlines.Add(new Bold(new Run("Defeito: ")));
            dados.Inlines.Add(new Run($"{defeito}\n"));

            dados.Inlines.Add(new Bold(new Run("Serviço: ")));
            dados.Inlines.Add(new Run($"{servico}"));

            sec.Blocks.Add(dados);

            Paragraph termos = new Paragraph
            {
                FontSize = Math.Max(6.5, baseFontSize - 2.2),
                LineHeight = Math.Max(8.0, baseFontSize + 0.3),
                Foreground = Brushes.DimGray,
                Margin = new Thickness(0, 0, 0, 3)
            };
            termos.Inlines.Add(new Bold(new Run("Garantia: ")));
            termos.Inlines.Add(new Run(string.Join(" | ", TermosGarantia)));
            sec.Blocks.Add(termos);

            Paragraph assinaturas = new Paragraph(
                new Run("___________________________________                    ___________________________________\n" +
                        "       Assinatura do Cliente                                          Assinatura do Lojista"))
            {
                FontSize = Math.Max(7.5, baseFontSize - 1.0),
                TextAlignment = TextAlignment.Center,
                LineHeight = baseFontSize + 3.0,
                Margin = new Thickness(0, 2, 0, 0)
            };
            sec.Blocks.Add(assinaturas);

            return sec;
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
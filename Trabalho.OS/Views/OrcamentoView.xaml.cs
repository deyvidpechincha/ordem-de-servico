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

        public OrcamentoView()
        {
            InitializeComponent();
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
            if (printDialog.ShowDialog() == true)
            {
                double pageWidth = printDialog.PrintableAreaWidth > 0 ? printDialog.PrintableAreaWidth : 794;
                double pageHeight = printDialog.PrintableAreaHeight > 0 ? printDialog.PrintableAreaHeight : 1122;

                Thickness padding = new Thickness(24, 14, 24, 14);
                double larguraUtil = Math.Max(200, pageWidth - padding.Left - padding.Right);

                double fontSizeBase = 10.0;
                FlowDocument doc = ConstruirDocumentoOrcamento(pageWidth, pageHeight, padding, larguraUtil, fontSizeBase, peca, maoObra, total);

                IDocumentPaginatorSource idpSource = doc;
                DocumentPaginator paginator = idpSource.DocumentPaginator;
                paginator.PageSize = new Size(pageWidth, pageHeight);
                paginator.ComputePageCount();

                while (paginator.PageCount > 1 && fontSizeBase > 7.5)
                {
                    fontSizeBase -= 0.5;
                    doc = ConstruirDocumentoOrcamento(pageWidth, pageHeight, padding, larguraUtil, fontSizeBase, peca, maoObra, total);
                    idpSource = doc;
                    paginator = idpSource.DocumentPaginator;
                    paginator.PageSize = new Size(pageWidth, pageHeight);
                    paginator.ComputePageCount();
                }

                string nomeSeguro = SanitizarNomeArquivo(TxtClienteAparelho.Text);
                printDialog.PrintDocument(paginator, $"Orcamento_{nomeSeguro}");
            }
        }

        private FlowDocument ConstruirDocumentoOrcamento(
            double pageWidth,
            double pageHeight,
            Thickness padding,
            double larguraUtil,
            double baseFontSize,
            decimal peca,
            decimal maoObra,
            decimal total)
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

            doc.Blocks.Add(CriarViaOrcamento("VIA DO LOJISTA", baseFontSize, peca, maoObra, total));

            Paragraph corte = new Paragraph(new Run("- - - - - - - - - - - - - - - - - - - - -  CORTE AQUI  - - - - - - - - - - - - - - - - - - - - -"))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 5, 0, 5),
                Foreground = Brushes.Gray,
                FontSize = Math.Max(7.5, baseFontSize - 1.5)
            };
            doc.Blocks.Add(corte);

            doc.Blocks.Add(CriarViaOrcamento("VIA DO CLIENTE", baseFontSize, peca, maoObra, total));

            return doc;
        }

        private Section CriarViaOrcamento(string tituloVia, double baseFontSize, decimal peca, decimal maoObra, decimal total)
        {
            Section sec = new Section
            {
                Margin = new Thickness(0)
            };

            Paragraph titulo = new Paragraph(new Run($"TECKY CELL (027 98837-2300) — ORÇAMENTO | {tituloVia}"))
            {
                FontSize = baseFontSize + 1.5,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            };
            sec.Blocks.Add(titulo);

            Paragraph dados = new Paragraph
            {
                LineHeight = baseFontSize + 4.0,
                Margin = new Thickness(0, 0, 0, 4)
            };

            dados.Inlines.Add(new Bold(new Run("Data: ")));
            dados.Inlines.Add(new Run($"{DateTime.Now:dd/MM/yyyy HH:mm}    "));
            dados.Inlines.Add(new Bold(new Run("Contato: ")));
            dados.Inlines.Add(new Run($"{LimitarTexto(TxtContato.Text, 25)}\n"));

            dados.Inlines.Add(new Bold(new Run("Cliente / Aparelho: ")));
            dados.Inlines.Add(new Run($"{LimitarTexto(TxtClienteAparelho.Text, 70)}\n"));

            dados.Inlines.Add(new Bold(new Run("Descrição: ")));
            dados.Inlines.Add(new Run($"{LimitarTexto(TxtDescricaoOrc.Text, 160)}\n"));

            dados.Inlines.Add(new Bold(new Run("Valor da Peça: ")));
            dados.Inlines.Add(new Run($"{peca.ToString("C2", CulturaPtBr)}    "));
            dados.Inlines.Add(new Bold(new Run("Mão de Obra: ")));
            dados.Inlines.Add(new Run($"{maoObra.ToString("C2", CulturaPtBr)}"));

            sec.Blocks.Add(dados);

            Paragraph totalBlock = new Paragraph(new Run($"VALOR TOTAL: {total.ToString("C2", CulturaPtBr)}"))
            {
                FontSize = baseFontSize + 1.5,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 4, 0, 6),
                TextAlignment = TextAlignment.Center
            };
            sec.Blocks.Add(totalBlock);

            Paragraph assinaturas = new Paragraph(
                new Run("___________________________________                    ___________________________________\n" +
                        "       Assinatura do Cliente                                          Assinatura do Lojista"))
            {
                FontSize = Math.Max(8.0, baseFontSize - 1.0),
                TextAlignment = TextAlignment.Center,
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
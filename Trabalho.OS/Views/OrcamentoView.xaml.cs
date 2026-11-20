using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Trabalho.OS.Views
{
    public partial class OrcamentoView : UserControl
    {
        public OrcamentoView()
        {
            InitializeComponent();
        }

        private void CalcularTotal(object sender, TextChangedEventArgs e)
        {
            decimal.TryParse(TxtValorPeca.Text, out decimal peca);
            decimal.TryParse(TxtMaoObra.Text, out decimal maoObra);
            LblTotalOrcamento.Text = $"Total do Orçamento: {(peca + maoObra):C2}";
        }

        private void BtnImprimirOrcamento_Click(object sender, RoutedEventArgs e)
        {
            decimal.TryParse(TxtValorPeca.Text, out decimal peca);
            decimal.TryParse(TxtMaoObra.Text, out decimal maoObra);
            decimal total = peca + maoObra;

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = new FlowDocument();
                doc.PageWidth = printDialog.PrintableAreaWidth;
                doc.PageHeight = printDialog.PrintableAreaHeight;
                doc.PagePadding = new Thickness(30);
                doc.ColumnWidth = printDialog.PrintableAreaWidth;
                doc.FontFamily = new FontFamily("Segoe UI");
                doc.FontSize = 11;

                //  Via do Lojista 
                doc.Blocks.Add(CriarViaOrcamento("VIA DO LOJISTA", peca, maoObra, total));

                // linha de corte no meio da folha A4
                Paragraph corte = new Paragraph(new Run("---------------------------------------------------- CORTE AQUI ----------------------------------------------------"))
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 15, 0, 15),
                    Foreground = Brushes.Gray
                };
                doc.Blocks.Add(corte);

                // Via do Cliente 
                doc.Blocks.Add(CriarViaOrcamento("VIA DO CLIENTE", peca, maoObra, total));

                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, $"Orcamento_{TxtClienteAparelho.Text}");
            }
        }

        private Section CriarViaOrcamento(string tituloVia, decimal peca, decimal maoObra, decimal total)
        {
            Section sec = new Section();

            // Cabeçalho da Via
            Paragraph titulo = new Paragraph(new Run($"ORÇAMENTO DE SERVIÇO - {tituloVia}"))
            {
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            };
            sec.Blocks.Add(titulo);

            // dados do Orçamento
            Paragraph dados = new Paragraph();
            dados.LineHeight = 18;
            dados.Inlines.Add(new Bold(new Run("Data: "))); dados.Inlines.Add(new Run($"{DateTime.Now:dd/MM/yyyy HH:mm}\n"));
            dados.Inlines.Add(new Bold(new Run("Cliente / Aparelho: "))); dados.Inlines.Add(new Run($"{TxtClienteAparelho.Text}\n"));
            dados.Inlines.Add(new Bold(new Run("Descrição do Serviço / Peças: "))); dados.Inlines.Add(new Run($"{TxtDescricaoOrc.Text}\n"));
            dados.Inlines.Add(new Bold(new Run("Valor da Peça: "))); dados.Inlines.Add(new Run($"{peca:C2}    "));
            dados.Inlines.Add(new Bold(new Run("Mão de Obra: "))); dados.Inlines.Add(new Run($"{maoObra:C2}\n"));
            sec.Blocks.Add(dados);

            // total 
            Paragraph totalBlock = new Paragraph(new Run($"VALOR TOTAL: {total:C2}"))
            {
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 5, 0, 15),
                TextAlignment = TextAlignment.Center
            };
            sec.Blocks.Add(totalBlock);

            // assinaturas  das partes
            Paragraph assinaturas = new Paragraph(new Run("____________________________________                    ____________________________________\n           Assinatura do Cliente                                                Assinatura do Lojista"))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            sec.Blocks.Add(assinaturas);

            return sec;
        }
    }
}
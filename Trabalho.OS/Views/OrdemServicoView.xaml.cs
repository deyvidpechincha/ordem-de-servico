using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Trabalho.OS.Views
{
    public partial class  OrdemServicoView : UserControl
    {
        public OrdemServicoView ()
        {
            InitializeComponent();
            TxtData.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }

        private void BtnImprimir_Click (object sender, RoutedEventArgs e)
        {
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

                
                doc.Blocks.Add(CriarViaOS("VIA DO LOJISTA"));
                Paragraph corte = new Paragraph(new Run("---------------------------------------------------- CORTE ----------------------------------------------------"))
                {
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 15, 0, 15),
                    Foreground = Brushes.Gray
                };
                doc.Blocks.Add(corte);
                doc.Blocks.Add(CriarViaOS("VIA DO CLIENTE")) ;

                IDocumentPaginatorSource idpSource =  doc;
                printDialog.PrintDocument( idpSource.DocumentPaginator, $"OS_{TxtNome.Text}");
            }
        }

        private Section CriarViaOS(string tituloVia)
        {
            Section sec = new Section();

            Paragraph header = new Paragraph(new Run($"ORDEM DE SERVIÇO - {tituloVia}"))
            {
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            sec.Blocks.Add(header);

            Paragraph dados = new Paragraph();
            dados.LineHeight = 18;
            dados.Inlines.Add(new Bold(new Run("Data: "))); dados.Inlines.Add(new Run($"{TxtData.Text}    "));
            dados.Inlines.Add(new Bold(new Run("Cliente: "))); dados.Inlines.Add(new Run($"{TxtNome.Text}\n"));
            dados.Inlines.Add(new Bold(new Run("Endereço: "))); dados.Inlines.Add(new Run($"{TxtEndereco.Text}\n"));
            dados.Inlines.Add(new Bold(new Run("Modelo: "))); dados.Inlines.Add(new Run($"{TxtModelo.Text}    "));
            dados.Inlines.Add(new Bold(new Run("Série/IMEI: "))); dados.Inlines.Add(new Run($"{TxtImei.Text}\n"));
            dados.Inlines.Add(new Bold(new Run("Defeito Relatado: "))); dados.Inlines.Add(new Run($"{TxtDefeito.Text}\n"));
            dados.Inlines.Add(new Bold(new Run("Serviço a Realizar: "))); dados.Inlines.Add(new Run($"{TxtServico.Text}\n"));
            
            sec.Blocks.Add(dados);

            Paragraph assinaturas = new Paragraph(new Run("\n\n____________________________________                    ____________________________________\n           Assinatura do Cliente                                                Assinatura do Lojista"))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };
            sec.Blocks.Add(assinaturas);

            return sec;
        }
    }
}
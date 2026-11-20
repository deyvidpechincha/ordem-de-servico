using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Trabalho.OS.Views
{
    public class MovimentoCaixa
    {
        public string  Hora { get; set; }
        public string Tipo { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public string ValorFormatado => Valor.ToString("C2");
    }

    public partial class CaixaView : UserControl
    {
        private ObservableCollection<MovimentoCaixa > _movimentos = new ObservableCollection<MovimentoCaixa>();

        // JSON,carregar,dados  
        private static string PastaCaixa => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DadosCaixa");
        private string ArquivoCaixa => Path.Combine(PastaCaixa, $"caixa_{DateTime.Now:yyyyMMdd}.json");
        public CaixaView()
        {
            InitializeComponent();
            GridMovimentos.ItemsSource = _movimentos;
            CarregarCaixaDoJson();
        }

        private void CarregarCaixaDoJson()
        {
            try
            {
                if (File.Exists(ArquivoCaixa))
                {
                    string jsonString = File.ReadAllText(ArquivoCaixa);
                    var listaSalva = JsonSerializer.Deserialize<List<MovimentoCaixa>>(jsonString);

                    if (listaSalva != null)
                    {
                        _movimentos.Clear();
                        foreach (var item in listaSalva)
                        {
                            _movimentos.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar o caixa salvo: {ex.Message}", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            AtualizarSaldo();
        }

        private void SalvarCaixaNoJson()
        {
            try
            {
                Directory.CreateDirectory(PastaCaixa);
                string jsonString = JsonSerializer.Serialize(_movimentos.ToList(), new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ArquivoCaixa, jsonString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar os dados do caixa: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEntrada_Click(object sender, RoutedEventArgs e) => AdicionarMovimento("Entrada");
        private void BtnSaida_Click(object sender, RoutedEventArgs e) => AdicionarMovimento("Saída");

        private void AdicionarMovimento(string tipo)
        {
            if (string.IsNullOrWhiteSpace(TxtDesc.Text) || !decimal.TryParse(TxtValor.Text, out decimal valor) || valor <= 0)
            {
                MessageBox.Show("Preencha uma descrição e um valor numérico válido.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Add lista visual
            _movimentos.Add(new MovimentoCaixa
            {
                Hora = DateTime.Now.ToString("HH:mm"),
                Tipo = tipo,
                Descricao = TxtDesc.Text,
                Valor = valor
            });

            // Salva  JSON
            SalvarCaixaNoJson();

            TxtDesc.Clear();
            TxtValor.Clear();
            AtualizarSaldo();
        }

        private decimal CalcularSaldo()
        {
            decimal entradas = _movimentos.Where(m => m.Tipo == "Entrada").Sum(m => m.Valor);
            decimal saidas = _movimentos.Where(m => m.Tipo == "Saída").Sum(m => m.Valor);
            return entradas - saidas;
        }

        private void AtualizarSaldo()
        {
            LblSaldo.Text = $"Saldo : {CalcularSaldo():C2}";
        }

        private void BtnFecharCaixa_Click(object sender, RoutedEventArgs e)
        {
            if (_movimentos.Count == 0)
            {
                MessageBox.Show("Não há movimentações registradas hoje para fechar o caixa.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                FlowDocument doc = new FlowDocument
                {
                    PagePadding = new Thickness(40),
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 12
                };

                doc.Blocks.Add(new Paragraph(new Run($"FECHAMENTO DE CAIXA - {DateTime.Now:dd/MM/yyyy}"))
                {
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 20)
                });

                foreach (var item in _movimentos)
                {
                    string sinal = item.Tipo == "Entrada" ? "+" : "-";
                    doc.Blocks.Add(new Paragraph(new Run($"[{item.Hora}] {item.Tipo.ToUpper()}: {item.Descricao}  -->  {sinal} {item.Valor:C2}")));
                }

                decimal totalEntradas = _movimentos.Where(m => m.Tipo == "Entrada").Sum(m => m.Valor);
                decimal totalSaidas = _movimentos.Where(m => m.Tipo == "Saída").Sum(m => m.Valor);

                doc.Blocks.Add(new Paragraph(new Run($"\n----------------------------------------------------\nTotal Entradas: {totalEntradas:C2}\nTotal Saídas: {totalSaidas:C2}\nSALDO FINAL DO CAIXA: {CalcularSaldo():C2}"))
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 20, 0, 0)
                });

                IDocumentPaginatorSource idpSource = doc;
                printDialog.PrintDocument(idpSource.DocumentPaginator, $"Fechamento_Caixa_{DateTime.Now:dd_MM_yyyy}");

                // fechar,renomeia, JSON/dia 
                try
                {
                    string arquivoFechado = Path.Combine(PastaCaixa, $"caixa_{DateTime.Now:yyyyMMdd}_FECHADO.json");
                    if (File.Exists(ArquivoCaixa))
                    {
                        if (File.Exists(arquivoFechado)) File.Delete(arquivoFechado);
                        File.Move(ArquivoCaixa, arquivoFechado);
                    }
                }
                catch { }

                _movimentos.Clear();
                AtualizarSaldo();
                MessageBox.Show("Caixa fechado com sucesso e relatório gerado!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
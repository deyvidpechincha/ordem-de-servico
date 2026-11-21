using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
        public string Hora { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string ValorFormatado => Valor.ToString("C2", new CultureInfo("pt-BR"));
    }

    public partial class CaixaView : UserControl
    {
        private static readonly CultureInfo CulturaPtBr = new CultureInfo("pt-BR");
        private readonly ObservableCollection<MovimentoCaixa> _movimentos = new ObservableCollection<MovimentoCaixa>();

        private static string PastaCaixa => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Trabalho.OS",
            "DadosCaixa");

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
            string descricao = TxtDesc.Text.Trim();
            string textoValor = TxtValor.Text.Trim().Replace("R$", "").Trim();

            bool parseOk = decimal.TryParse(textoValor, NumberStyles.Number, CulturaPtBr, out decimal valor);
            if (!parseOk && textoValor.Contains(".") && !textoValor.Contains(","))
            {
                parseOk = decimal.TryParse(textoValor, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
            }

            if (string.IsNullOrWhiteSpace(descricao) || !parseOk || valor <= 0)
            {
                MessageBox.Show("Preencha uma descrição e um valor numérico maior que zero.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _movimentos.Add(new MovimentoCaixa
            {
                Hora = DateTime.Now.ToString("HH:mm"),
                Tipo = tipo,
                Descricao = descricao,
                Valor = valor
            });

            SalvarCaixaNoJson();

            TxtDesc.Clear();
            TxtValor.Clear();
            TxtDesc.Focus();
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
            decimal saldo = CalcularSaldo();
            LblSaldo.Text = $"Saldo : {saldo.ToString("C2", CulturaPtBr)}";
            LblSaldo.Foreground = saldo < 0 ? new SolidColorBrush(Color.FromRgb(255, 107, 107)) : Brushes.White;
        }

        private void BtnFecharCaixa_Click(object sender, RoutedEventArgs e)
        {
            if (_movimentos.Count == 0)
            {
                MessageBox.Show("Não há movimentações registradas hoje para fechar o caixa.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MessageBoxResult confirmacao = MessageBox.Show(
                "Deseja realmente fechar o caixa do dia e imprimir o relatório?",
                "Confirmar Fechamento",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacao != MessageBoxResult.Yes)
                return;

            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                try
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
                        doc.Blocks.Add(new Paragraph(new Run(
                            $"[{item.Hora}] {item.Tipo.ToUpper()}: {item.Descricao}  -->  {sinal} {item.Valor.ToString("C2", CulturaPtBr)}")));
                    }

                    decimal totalEntradas = _movimentos.Where(m => m.Tipo == "Entrada").Sum(m => m.Valor);
                    decimal totalSaidas = _movimentos.Where(m => m.Tipo == "Saída").Sum(m => m.Valor);
                    decimal saldoFinal = totalEntradas - totalSaidas;

                    doc.Blocks.Add(new Paragraph(new Run(
                        $"\n----------------------------------------------------\n" +
                        $"Total Entradas: {totalEntradas.ToString("C2", CulturaPtBr)}\n" +
                        $"Total Saídas: {totalSaidas.ToString("C2", CulturaPtBr)}\n" +
                        $"SALDO FINAL DO CAIXA: {saldoFinal.ToString("C2", CulturaPtBr)}"))
                    {
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 20, 0, 0)
                    });

                    IDocumentPaginatorSource idpSource = doc;
                    printDialog.PrintDocument(idpSource.DocumentPaginator, $"Fechamento_Caixa_{DateTime.Now:dd_MM_yyyy}");

                    Directory.CreateDirectory(PastaCaixa);
                    string timestamp = DateTime.Now.ToString("HHmmss");
                    string arquivoFechado = Path.Combine(PastaCaixa, $"caixa_{DateTime.Now:yyyyMMdd}_FECHADO_{timestamp}.json");

                    if (File.Exists(ArquivoCaixa))
                    {
                        File.Move(ArquivoCaixa, arquivoFechado);
                    }

                    _movimentos.Clear();
                    AtualizarSaldo();

                    MessageBox.Show("Caixa fechado com sucesso e relatório enviado para impressão!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao concluir o fechamento do caixa: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
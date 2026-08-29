using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Media;

namespace SaldanhaMoveisDesktop
{
    public partial class SistemaERP : Window
    {

        private AppDbContext dbContext;
        private Transacao transacaoEmEdicao = null;
        private PagamentoFuncionario pagamentoEmEdicao = null;
        private Cliente clienteEmEdicao = null;
        private Produto produtoEmEdicao = null;
        private List<ItemVenda> carrinhoAtual = new List<ItemVenda>();

        public SistemaERP()
        {
            InitializeComponent();

            // O Único Motor do Sistema!
            dbContext = new AppDbContext();
            dbContext.Database.Migrate();

            // Atualiza TODAS as abas
            AtualizarDashboard();
            AtualizarTelaFuncionarios();
            AtualizarTelaClientes();
            AtualizarTelaProdutos();
            AtualizarCombosPDV();
        }

        // ==========================================
        // EVENTOS DA ABA 1: DASHBOARD
        // ==========================================
        private void AtualizarDashboard()
        {
            var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var hoje = DateTime.Today;

            var transacoesMes = dbContext.Transacoes.Where(t => t.Data >= inicioMes).ToList();
            int qtdVendasHoje = dbContext.Vendas.Count(v => v.DataVenda.Date == hoje);

            decimal receitasMes = transacoesMes.Where(t => t.Tipo == "Entrada").Sum(t => t.Valor);
            decimal despesasMes = transacoesMes.Where(t => t.Tipo == "Saída" || t.Tipo == "Saida").Sum(t => t.Valor);
            decimal lucroMes = receitasMes - despesasMes;

            txtFaturamentoMes.Text = receitasMes.ToString("C");
            txtDespesasMes.Text = despesasMes.ToString("C");
            txtLucroMes.Text = lucroMes.ToString("C");
            txtVendasHoje.Text = qtdVendasHoje.ToString();

            var valoresSemana = new ChartValues<decimal>();
            var labelsSemana = new List<string>();

            for (int i = 6; i >= 0; i--)
            {
                var dia = hoje.AddDays(-i);
                var faturamentoDia = dbContext.Transacoes.Where(t => t.Data.Date == dia.Date && t.Tipo == "Entrada").Sum(t => t.Valor);

                valoresSemana.Add(faturamentoDia);
                labelsSemana.Add(dia.ToString("dd/MM"));
            }

            graficoSemana.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Entradas",
                    Values = valoresSemana,
                    Fill = new SolidColorBrush(Color.FromRgb(212, 175, 55))
                }
            };

            graficoSemana.AxisX.Clear();
            graficoSemana.AxisX.Add(new Axis { Labels = labelsSemana, Foreground = Brushes.LightGray });

            graficoSemana.AxisY.Clear();
            graficoSemana.AxisY.Add(new Axis { LabelFormatter = val => val.ToString("C0"), Foreground = Brushes.LightGray });

            graficoMes.Series = new SeriesCollection
            {
                new PieSeries { Title = "Receitas", Values = new ChartValues<decimal> { receitasMes }, Fill = Brushes.MediumSeaGreen, DataLabels = true },
                new PieSeries { Title = "Despesas", Values = new ChartValues<decimal> { despesasMes }, Fill = Brushes.IndianRed, DataLabels = true }
            };
        }

        // ==========================================
        // EVENTO DO BOTÃO DE RELATÓRIO DE MOVIMENTAÇÕES
        // ==========================================
        private void ClicouVerMovimentacoes(object sender, RoutedEventArgs e)
        {
            var janelaMovimentacoes = new MovimentacoesWindow();
            janelaMovimentacoes.ShowDialog();
        }

        // ==========================================
        // EVENTOS DA ABA 2: FUNCIONÁRIOS 
        // ==========================================
        private void ClicouRegistrarFuncionario(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputNomeFunc.Text) || !decimal.TryParse(inputValorFunc.Text, out decimal val) || val <= 0)
            {
                MessageBox.Show("Preencha o nome e um valor válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (pagamentoEmEdicao == null)
                {
                    var pag = new PagamentoFuncionario
                    {
                        Data = DateTime.Now,
                        Nome = inputNomeFunc.Text,
                        Cargo = inputCargo.Text,
                        Valor = val,
                        MesReferencia = inputMesRef.Text
                    };
                    dbContext.Pagamentos.Add(pag);

                    var transacaoDespesa = new Transacao
                    {
                        Descricao = $"Salário - {inputNomeFunc.Text} (Ref: {inputMesRef.Text})",
                        Valor = val,
                        Tipo = "Saída",
                        Categoria = "Folha de Pagamento",
                        Data = DateTime.Now
                    };
                    dbContext.Transacoes.Add(transacaoDespesa);
                }
                else
                {
                    pagamentoEmEdicao.Nome = inputNomeFunc.Text;
                    pagamentoEmEdicao.Cargo = inputCargo.Text;
                    pagamentoEmEdicao.Valor = val;
                    pagamentoEmEdicao.MesReferencia = inputMesRef.Text;
                }

                dbContext.SaveChanges();
                pagamentoEmEdicao = null;
                btnRegistrarFunc.Content = "Salvar Pagamento";

                inputNomeFunc.Clear();
                inputCargo.Clear();
                inputValorFunc.Clear();
                inputMesRef.Clear();

                AtualizarTelaFuncionarios();
                AtualizarDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
            }
        }

        private void ClicouEditarFuncionario(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PagamentoFuncionario p)
            {
                inputNomeFunc.Text = p.Nome;
                inputCargo.Text = p.Cargo;
                inputValorFunc.Text = p.Valor.ToString();
                inputMesRef.Text = p.MesReferencia;

                pagamentoEmEdicao = p;
                btnRegistrarFunc.Content = "Salvar Alteração";
            }
        }

        private void ClicouExcluirFuncionario(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is PagamentoFuncionario p && MessageBox.Show($"Apagar o pagamento de: {p.Nome}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                dbContext.Pagamentos.Remove(p);
                dbContext.SaveChanges();
                AtualizarTelaFuncionarios();
            }
        }

        private void AtualizarTelaFuncionarios()
        {
            var pags = dbContext.Pagamentos.ToList();

            gridFuncionarios.ItemsSource = null;
            gridFuncionarios.Items.Clear();
            gridFuncionarios.ItemsSource = pags;

            txtTotalFolha.Text = pags.Sum(p => p.Valor).ToString("C");
            txtQtdPagos.Text = pags.Count.ToString();
        }

        // ==========================================
        // EVENTOS DA ABA 3: CLIENTES
        // ==========================================
        private void ClicouSalvarCliente(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputNomeCliente.Text) || string.IsNullOrWhiteSpace(inputCpfCliente.Text))
            {
                MessageBox.Show("Preencha pelo menos o Nome e o CPF/CNPJ.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (clienteEmEdicao == null)
                {
                    var novoCliente = new Cliente
                    {
                        Nome = inputNomeCliente.Text,
                        CpfCnpj = inputCpfCliente.Text,
                        Telefone = inputTelefoneCliente.Text,
                        Endereco = inputEnderecoCliente.Text,
                        DataCadastro = DateTime.Now
                    };
                    dbContext.Clientes.Add(novoCliente);
                }
                else
                {
                    clienteEmEdicao.Nome = inputNomeCliente.Text;
                    clienteEmEdicao.CpfCnpj = inputCpfCliente.Text;
                    clienteEmEdicao.Telefone = inputTelefoneCliente.Text;
                    clienteEmEdicao.Endereco = inputEnderecoCliente.Text;
                }

                dbContext.SaveChanges();
                ClicouLimparCliente(null, null);
                AtualizarTelaClientes();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar cliente: {ex.Message}");
            }
        }

        private void ClicouEditarCliente(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Cliente c)
            {
                inputNomeCliente.Text = c.Nome;
                inputCpfCliente.Text = c.CpfCnpj;
                inputTelefoneCliente.Text = c.Telefone;
                inputEnderecoCliente.Text = c.Endereco;
                clienteEmEdicao = c;
            }
        }

        private void ClicouExcluirCliente(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Cliente c && MessageBox.Show($"Deseja apagar {c.Nome}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                dbContext.Clientes.Remove(c);
                dbContext.SaveChanges();
                AtualizarTelaClientes();
            }
        }

        private void ClicouLimparCliente(object sender, RoutedEventArgs e)
        {
            inputNomeCliente.Clear();
            inputCpfCliente.Clear();
            inputTelefoneCliente.Clear();
            inputEnderecoCliente.Clear();
            clienteEmEdicao = null;
            inputNomeCliente.Focus();
        }

        private void AtualizarTelaClientes()
        {
            gridClientes.ItemsSource = null;
            gridClientes.Items.Clear();
            gridClientes.ItemsSource = dbContext.Clientes.ToList();
        }

        // ==========================================
        // EVENTOS DA ABA 4: PRODUTOS
        // ==========================================
        private void ClicouSalvarProduto(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputNomeProduto.Text) || !decimal.TryParse(inputPrecoCusto.Text, out decimal custo) || !decimal.TryParse(inputPrecoVenda.Text, out decimal venda) || !int.TryParse(inputQtdEstoque.Text, out int qtd))
            {
                MessageBox.Show("Preencha todos os campos numericamente.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (produtoEmEdicao == null)
                {
                    var novoProduto = new Produto
                    {
                        Nome = inputNomeProduto.Text,
                        Descricao = "Geral",
                        PrecoCusto = custo,
                        PrecoVenda = venda,
                        QuantidadeEstoque = qtd,
                        Ativo = true,
                        DataCadastro = DateTime.Now
                    };
                    dbContext.Produtos.Add(novoProduto);
                }
                else
                {
                    produtoEmEdicao.Nome = inputNomeProduto.Text;
                    produtoEmEdicao.PrecoCusto = custo;
                    produtoEmEdicao.PrecoVenda = venda;
                    produtoEmEdicao.QuantidadeEstoque = qtd;
                }

                dbContext.SaveChanges();
                ClicouLimparProduto(null, null);
                AtualizarTelaProdutos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
            }
        }

        private void ClicouEditarProduto(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Produto p)
            {
                inputNomeProduto.Text = p.Nome;
                inputPrecoCusto.Text = p.PrecoCusto.ToString();
                inputPrecoVenda.Text = p.PrecoVenda.ToString();
                inputQtdEstoque.Text = p.QuantidadeEstoque.ToString();
                produtoEmEdicao = p;
            }
        }

        private void ClicouExcluirProduto(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Produto p && MessageBox.Show($"Excluir {p.Nome}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                dbContext.Produtos.Remove(p);
                dbContext.SaveChanges();
                AtualizarTelaProdutos();
            }
        }

        private void ClicouLimparProduto(object sender, RoutedEventArgs e)
        {
            inputNomeProduto.Clear();
            inputPrecoCusto.Clear();
            inputPrecoVenda.Clear();
            inputQtdEstoque.Clear();
            produtoEmEdicao = null;
            inputNomeProduto.Focus();
        }

        private void AtualizarTelaProdutos()
        {
            gridProdutos.ItemsSource = null;
            gridProdutos.Items.Clear();
            gridProdutos.ItemsSource = dbContext.Produtos.ToList();
        }

        // ==========================================
        // EVENTOS DA ABA 5: PONTO DE VENDA (PDV)
        // ==========================================
        private void AtualizarCombosPDV()
        {
            comboClientesPdv.ItemsSource = dbContext.Clientes.ToList();
            comboProdutosPdv.ItemsSource = dbContext.Produtos.Where(p => p.QuantidadeEstoque > 0).ToList();
        }

        private void ClicouAdicionarAoCarrinho(object sender, RoutedEventArgs e)
        {
            if (comboProdutosPdv.SelectedItem is Produto prod && int.TryParse(inputQtdPdv.Text, out int qtd) && qtd > 0)
            {
                if (qtd > prod.QuantidadeEstoque)
                {
                    MessageBox.Show($"Temos apenas {prod.QuantidadeEstoque} unidades.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var item = new ItemVenda { ProdutoId = prod.Id, Produto = prod, Quantidade = qtd, PrecoUnitario = prod.PrecoVenda };
                carrinhoAtual.Add(item);
                AtualizarGridCarrinho();
            }
        }

        private void ClicouRemoverDoCarrinho(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is ItemVenda item)
            {
                carrinhoAtual.Remove(item);
                AtualizarGridCarrinho();
            }
        }

        private void AtualizarGridCarrinho()
        {
            gridCarrinho.ItemsSource = null;
            gridCarrinho.ItemsSource = carrinhoAtual;
            txtTotalVenda.Text = carrinhoAtual.Sum(i => i.Subtotal).ToString("C");
        }

        private void ClicouFinalizarVenda(object sender, RoutedEventArgs e)
        {
            if (comboClientesPdv.SelectedItem is not Cliente clienteSelecionado || !carrinhoAtual.Any())
            {
                MessageBox.Show("Selecione o cliente e adicione itens.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var novaVenda = new Venda
                {
                    ClienteId = clienteSelecionado.Id,
                    DataVenda = DateTime.Now,
                    Status = "Concluída",
                    ValorTotal = carrinhoAtual.Sum(i => i.Subtotal),
                    Itens = new List<ItemVenda>()
                };

                foreach (var item in carrinhoAtual)
                {
                    novaVenda.Itens.Add(new ItemVenda { ProdutoId = item.ProdutoId, Quantidade = item.Quantidade, PrecoUnitario = item.PrecoUnitario });
                    var produtoBanco = dbContext.Produtos.Find(item.ProdutoId);
                    if (produtoBanco != null) produtoBanco.QuantidadeEstoque -= item.Quantidade;
                }

                var transacaoAutomatica = new Transacao
                {
                    Descricao = $"Venda - Cliente: {clienteSelecionado.Nome}",
                    Valor = novaVenda.ValorTotal,
                    Tipo = "Entrada",
                    Categoria = "Vendas", 
                    Data = DateTime.Now
                };
                dbContext.Transacoes.Add(transacaoAutomatica);

                dbContext.Vendas.Add(novaVenda);
                dbContext.SaveChanges();
                MessageBox.Show("Venda finalizada e lançada no caixa!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                carrinhoAtual.Clear();
                AtualizarGridCarrinho();
                AtualizarTelaProdutos();
                AtualizarCombosPDV();
                AtualizarDashboard();
                comboClientesPdv.SelectedItem = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
            }
        }

        private void gridCarrinho_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
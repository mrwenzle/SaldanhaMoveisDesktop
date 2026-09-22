using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using LiveCharts;
using LiveCharts.Wpf;
using SaldanhaMoveisDesktop.Models;

namespace SaldanhaMoveisDesktop
{
    public partial class SistemaERP : Window
    {
        private AppDbContext dbContext;
        private PagamentoFuncionario pagamentoEmEdicao = null;
        private Cliente clienteEmEdicao = null;
        private Produto produtoEmEdicao = null;
        private Fornecedor fornecedorEmEdicao = null;
        private List<ItemVenda> carrinhoAtual = new List<ItemVenda>();

        public SistemaERP()
        {
            InitializeComponent();

            // Inicializa e garante as migrações do banco de dados SQLite
            dbContext = new AppDbContext();
            dbContext.Database.Migrate();

            // Atualiza todas as abas
            AtualizarDashboard();
            AtualizarTelaFuncionarios();
            AtualizarTelaClientes();
            AtualizarTelaProdutos();
            AtualizarTelaFornecedores();
            AtualizarCombosPDV();
            AtualizarTelaDespesas();
        }

        // ==========================================
        // ABA 1: DASHBOARD GERENCIAL (COM GRÁFICOS)
        // ==========================================
        private void AtualizarDashboard()
        {
            try
            {
                var hoje = DateTime.Today;
                var mesAtual = DateTime.Now.Month;
                var anoAtual = DateTime.Now.Year;

                // 1. Somatório do Faturamento do Mês (Vendas do mês atual)
                decimal faturamentoMes = dbContext.Vendas
                    .Where(v => v.DataVenda.Month == mesAtual && v.DataVenda.Year == anoAtual)
                    .Sum(v => (decimal?)v.ValorTotal) ?? 0;

                // 2. Somatório das Despesas do Mês
                decimal despesasMes = dbContext.Despesas
                    .Where(d => d.DataCadastro.Month == mesAtual && d.DataCadastro.Year == anoAtual)
                    .Sum(d => (decimal?)d.Valor) ?? 0;

                // 3. Lucro Líquido Real
                decimal lucroMes = faturamentoMes - despesasMes;

                // 4. Totais do Dia (Hoje)
                int vendasHojeCount = dbContext.Vendas
                    .Count(v => v.DataVenda.Date == hoje);

                decimal despesasHoje = dbContext.Despesas
                    .Where(d => d.DataCadastro.Date == hoje)
                    .Sum(d => (decimal?)d.Valor) ?? 0;

                // Atualiza os cartões numéricos da interface
                txtFaturamentoMes.Text = faturamentoMes.ToString("C2");
                txtDespesasMes.Text = despesasMes.ToString("C2");
                txtLucroMes.Text = lucroMes.ToString("C2");
                txtVendasHoje.Text = vendasHojeCount.ToString();
                txtDespesasHoje.Text = despesasHoje.ToString("C2");

                // ==========================================
                // 5. ATUALIZAÇÃO DO GRÁFICO 1: ÚLTIMOS 7 DIAS
                // ==========================================
                var ultimos7Dias = Enumerable.Range(0, 7)
                    .Select(i => hoje.AddDays(-6 + i))
                    .ToList();

                var valoresVendasDias = new ChartValues<decimal>();
                var labelsDias = new List<string>();

                foreach (var dia in ultimos7Dias)
                {
                    decimal totalDia = dbContext.Vendas
                        .Where(v => v.DataVenda.Date == dia)
                        .Sum(v => (decimal?)v.ValorTotal) ?? 0;

                    valoresVendasDias.Add(totalDia);
                    labelsDias.Add(dia.ToString("dd/MM"));
                }

                graficoSemana.Series = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Vendas (R$)",
                Values = valoresVendasDias,
                Stroke = (Brush)new BrushConverter().ConvertFrom("#D4AF37"),
                Fill = Brushes.Transparent,
                PointGeometrySize = 8
            }
        };

                graficoSemana.AxisX.Clear();
                graficoSemana.AxisX.Add(new Axis
                {
                    Title = "Dias",
                    Labels = labelsDias,
                    Foreground = Brushes.White
                });

                graficoSemana.AxisY.Clear();
                graficoSemana.AxisY.Add(new Axis
                {
                    Title = "Valor (R$)",
                    Foreground = Brushes.White,
                    LabelFormatter = val => val.ToString("C0")
                });

                // ==========================================
                // 6. ATUALIZAÇÃO DO GRÁFICO 2: RECEITAS X DESPESAS
                // ==========================================
                graficoMes.Series = new SeriesCollection
        {
            new PieSeries
            {
                Title = "Faturamento",
                Values = new ChartValues<decimal> { faturamentoMes },
                DataLabels = true,
                Fill = (Brush)new BrushConverter().ConvertFrom("#00ff00")
            },
            new PieSeries
            {
                Title = "Despesas",
                Values = new ChartValues<decimal> { despesasMes },
                DataLabels = true,
                Fill = (Brush)new BrushConverter().ConvertFrom("#ff4444")
            }
        };
            }
            catch (Exception ex)
            {
                // Exibe o erro real caso ocorra alguma falha de leitura
                string erro = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Erro ao atualizar o Dashboard: {erro}", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ClicouVerMovimentacoes(object sender, RoutedEventArgs e)
        {
            var janelaMovimentacoes = new MovimentacoesWindow();
            janelaMovimentacoes.ShowDialog();
        }

        // ==========================================
        // ABA 2: FUNCIONÁRIOS (RH + DEBITO NO CAIXA)
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

                    // Lança a despesa de folha de pagamento no caixa
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
        // ABA 3: CLIENTES
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
        // ABA 4: PRODUTOS (REGIME DE CAIXA NO ESTOQUE)
        // ==========================================
        private void ClicouEditarProduto(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Produto p)
            {
                produtoEmEdicao = p;
                inputNomeProduto.Text = p.Nome;
                comboFornecedorProduto.SelectedValue = p.FornecedorId;
                inputPrecoCusto.Text = p.PrecoCusto.ToString("N2");
                inputPrecoVenda.Text = p.PrecoVenda.ToString("N2");
                inputQtdEstoque.Text = p.QuantidadeEstoque.ToString();
            }
        }

        private void ClicouSalvarProduto(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputNomeProduto.Text))
            {
                MessageBox.Show("Informe o nome do produto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(inputPrecoCusto.Text, out decimal precoCusto);
            decimal.TryParse(inputPrecoVenda.Text, out decimal precoVenda);
            int.TryParse(inputQtdEstoque.Text, out int qtdEstoque);
            int? fornecedorId = comboFornecedorProduto.SelectedValue as int?;

            try
            {
                if (produtoEmEdicao == null)
                {
                    // CADASTRO DE NOVO PRODUTO
                    var novoProduto = new Produto
                    {
                        Nome = inputNomeProduto.Text,
                        FornecedorId = fornecedorId,
                        PrecoCusto = precoCusto,
                        PrecoVenda = precoVenda,
                        QuantidadeEstoque = qtdEstoque
                    };
                    dbContext.Produtos.Add(novoProduto);

                    // Lança o custo total do lote inicial como Despesa no Dashboard (Regime de Caixa)
                    if (precoCusto > 0 && qtdEstoque > 0)
                    {
                        var despesaEstoque = new Despesa
                        {
                            Descricao = $"Compra de Estoque: {novoProduto.Nome} ({qtdEstoque} un)",
                            Valor = precoCusto * qtdEstoque,
                            DataCadastro = DateTime.Now
                        };
                        dbContext.Despesas.Add(despesaEstoque);
                    }
                }
                else
                {
                    // EDIÇÃO DE PRODUTO EXISTENTE
                    int qtdAnterior = produtoEmEdicao.QuantidadeEstoque;

                    produtoEmEdicao.Nome = inputNomeProduto.Text;
                    produtoEmEdicao.FornecedorId = fornecedorId;
                    produtoEmEdicao.PrecoCusto = precoCusto;
                    produtoEmEdicao.PrecoVenda = precoVenda;
                    produtoEmEdicao.QuantidadeEstoque = qtdEstoque;

                    // Se o stock aumentou na edição, lança a diferença comprada como Despesa
                    if (qtdEstoque > qtdAnterior && precoCusto > 0)
                    {
                        int qtdComprada = qtdEstoque - qtdAnterior;
                        var despesaEstoque = new Despesa
                        {
                            Descricao = $"Entrada de Estoque: {produtoEmEdicao.Nome} (+{qtdComprada} un)",
                            Valor = precoCusto * qtdComprada,
                            DataCadastro = DateTime.Now
                        };
                        dbContext.Despesas.Add(despesaEstoque);
                    }
                }

                dbContext.SaveChanges();
                MessageBox.Show("Produto salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                ClicouLimparProduto(null, null);
                AtualizarTelaProdutos();
                AtualizarDashboard(); // Recarrega os valores de Lucro e Despesas no Dashboard
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar produto: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
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
            comboFornecedorProduto.SelectedIndex = -1;
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
            comboFornecedorProduto.ItemsSource = dbContext.Fornecedores.ToList();
        }

        // ==========================================
        // ABA: FORNECEDORES
        // ==========================================
        private void ClicouSalvarFornecedor(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputNomeFornecedor.Text))
            {
                MessageBox.Show("Preencha o Nome do Fornecedor.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (fornecedorEmEdicao == null)
                {
                    var novoFornecedor = new Fornecedor
                    {
                        NomeFantasia = inputNomeFornecedor.Text,
                        Cnpj = inputCnpjFornecedor.Text,
                        Telefone = inputTelefoneFornecedor.Text,
                        DataCadastro = DateTime.Now
                    };
                    dbContext.Fornecedores.Add(novoFornecedor);
                }
                else
                {
                    fornecedorEmEdicao.NomeFantasia = inputNomeFornecedor.Text;
                    fornecedorEmEdicao.Cnpj = inputCnpjFornecedor.Text;
                    fornecedorEmEdicao.Telefone = inputTelefoneFornecedor.Text;
                }

                dbContext.SaveChanges();
                ClicouLimparFornecedor(null, null);
                AtualizarTelaFornecedores();
                AtualizarTelaProdutos(); // Recarrega o ComboBox na tela de produtos automaticamente
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar fornecedor: {ex.Message}");
            }
        }

        private void ClicouEditarFornecedor(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Fornecedor f)
            {
                inputNomeFornecedor.Text = f.NomeFantasia;
                inputCnpjFornecedor.Text = f.Cnpj;
                inputTelefoneFornecedor.Text = f.Telefone;
                fornecedorEmEdicao = f;
            }
        }

        private void ClicouExcluirFornecedor(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Fornecedor f && MessageBox.Show($"Deseja excluir {f.NomeFantasia}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                dbContext.Fornecedores.Remove(f);
                dbContext.SaveChanges();
                AtualizarTelaFornecedores();
                AtualizarTelaProdutos();
            }
        }

        private void ClicouLimparFornecedor(object sender, RoutedEventArgs e)
        {
            inputNomeFornecedor.Clear();
            inputCnpjFornecedor.Clear();
            inputTelefoneFornecedor.Clear();
            fornecedorEmEdicao = null;
            inputNomeFornecedor.Focus();
        }

        private void AtualizarTelaFornecedores()
        {
            gridFornecedores.ItemsSource = null;
            gridFornecedores.Items.Clear();
            gridFornecedores.ItemsSource = dbContext.Fornecedores.ToList();
        }

        // ==========================================
        // ABA: DESPESAS
        // ==========================================
        private void ClicouSalvarDespesa(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputDescricaoDespesa.Text) || !decimal.TryParse(inputValorDespesa.Text, out decimal valor))
            {
                MessageBox.Show("Preencha a descrição e digite um valor válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var novaDespesa = new Despesa
            {
                Descricao = inputDescricaoDespesa.Text,
                Valor = valor,
                DataCadastro = DateTime.Now
            };

            dbContext.Despesas.Add(novaDespesa);
            dbContext.SaveChanges();

            inputDescricaoDespesa.Clear();
            inputValorDespesa.Clear();
            inputDescricaoDespesa.Focus();

            AtualizarTelaDespesas();
            AtualizarDashboard(); // Atualiza o painel inicial na hora
        }

        private void AtualizarTelaDespesas()
        {
            gridDespesas.ItemsSource = dbContext.Despesas
                .OrderByDescending(d => d.DataCadastro)
                .ToList();
        }

        // ==========================================
        // ABA 5: PONTO DE VENDA (PDV)
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
            if (carrinhoAtual.Count == 0)
            {
                MessageBox.Show("O carrinho de compras está vazio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (comboClientesPdv.SelectedValue == null)
            {
                MessageBox.Show("Selecione um cliente para registrar a venda.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int clienteId = (int)comboClientesPdv.SelectedValue;

                // Recolher com segurança os valores de Desconto e Frete digitados
                decimal.TryParse(inputDescontoVenda.Text, out decimal desconto);
                decimal.TryParse(inputFreteVenda.Text, out decimal frete);

                // Recolher a Forma de Pagamento selecionada no ComboBox
                string formaPagamento = (comboFormaPagamento.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "À Vista";

                // Calcular o valor total considerando os produtos, o desconto e o frete
                decimal subtotalProdutos = carrinhoAtual.Sum(i => i.Subtotal);
                decimal valorTotalFinal = (subtotalProdutos - desconto) + frete;
                if (valorTotalFinal < 0) valorTotalFinal = 0;

                var novaVenda = new Venda
                {
                    ClienteId = clienteId,
                    DataVenda = DateTime.Now,
                    Status = "Concluída",
                    ValorTotal = valorTotalFinal,
                    FormaPagamento = formaPagamento,
                    Desconto = desconto,
                    Frete = frete,
                    Itens = carrinhoAtual.ToList()
                };

                // Baixar a quantidade do stock dos produtos vendidos
                foreach (var item in carrinhoAtual)
                {
                    var produtoDb = dbContext.Produtos.Find(item.ProdutoId);
                    if (produtoDb != null)
                    {
                        produtoDb.QuantidadeEstoque -= item.Quantidade;
                    }
                }

                dbContext.Vendas.Add(novaVenda);
                dbContext.SaveChanges();

                MessageBox.Show("Venda finalizada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpar o carrinho e reiniciar os campos
                carrinhoAtual.Clear();
                gridCarrinho.ItemsSource = null;
                inputDescontoVenda.Text = "0";
                inputFreteVenda.Text = "0";
                txtTotalVenda.Text = "R$ 0,00";

                AtualizarDashboard();
                AtualizarTelaProdutos();
            }
            catch (Exception ex)
            {
                string erroDetalhado = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Erro ao salvar a venda: {erroDetalhado}", "Erro de Base de Dados", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
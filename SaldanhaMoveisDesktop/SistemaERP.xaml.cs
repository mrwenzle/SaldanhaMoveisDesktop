using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SaldanhaMoveisDesktop
{
    public partial class SistemaERP : Window
    {
        private ExcelDatabaseHelper dbHelper;
        private Transacao transacaoEmEdicao = null;
        private PagamentoFuncionario pagamentoEmEdicao = null;

        public SistemaERP()
        {
            InitializeComponent();
            dbHelper = new ExcelDatabaseHelper();
            AtualizarTelaCaixa();
            AtualizarTelaFuncionarios();
        }

        // ==========================================
        // EVENTOS DA ABA 1: FLUXO DE CAIXA
        // ==========================================
        private void ClicouRegistrarCaixa(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(inputDescricao.Text) || !decimal.TryParse(inputValor.Text, out decimal val) || val <= 0)
            {
                MessageBox.Show("Preencha a descrição e um valor numérico válido.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string cat = string.IsNullOrWhiteSpace(comboCategoria.Text) ? "Geral" : comboCategoria.Text;

            try
            {
                if (transacaoEmEdicao == null)
                {
                    dbHelper.SalvarTransacao(new Transacao { Descricao = inputDescricao.Text, Valor = val, Tipo = (comboTipo.SelectedItem as ComboBoxItem).Content.ToString(), Categoria = cat, Data = DateTime.Now });
                }
                else
                {
                    transacaoEmEdicao.Descricao = inputDescricao.Text;
                    transacaoEmEdicao.Valor = val;
                    transacaoEmEdicao.Tipo = (comboTipo.SelectedItem as ComboBoxItem).Content.ToString();
                    transacaoEmEdicao.Categoria = cat;

                    dbHelper.AtualizarTransacao(transacaoEmEdicao);

                    transacaoEmEdicao = null;
                    btnRegistrar.Content = "Registrar Transação";
                }

                inputDescricao.Clear();
                inputValor.Clear();
                inputDescricao.Focus();
                AtualizarTelaCaixa();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}");
            }
        }

        private void ClicouEditarCaixa(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Transacao t)
            {
                inputDescricao.Text = t.Descricao;
                inputValor.Text = t.Valor.ToString();
                comboTipo.Text = t.Tipo;
                comboCategoria.Text = t.Categoria;

                transacaoEmEdicao = t;
                btnRegistrar.Content = "Salvar Alteração";
            }
        }

        private void ClicouExcluirCaixa(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is Transacao t && MessageBox.Show($"Apagar: {t.Descricao}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                dbHelper.ExcluirTransacao(t);
                AtualizarTelaCaixa();
            }
        }

        private void MudouFiltroCaixa(object sender, SelectionChangedEventArgs e)
        {
            if (IsLoaded) AtualizarTelaCaixa();
        }

        private void AtualizarTelaCaixa()
        {
            string filtro = (comboFiltro.SelectedItem as ComboBoxItem)?.Content.ToString() == "Esta Semana" ? "Semana" : "Mes";
            var transacoes = dbHelper.ObterTransacoes(filtro);

            // 1. Removemos a conexão atual
            gridHistorico.ItemsSource = null;
            // 2. Forçamos a limpeza de qualquer item fantasma manual
            gridHistorico.Items.Clear();
            // 3. Conectamos a nova lista
            gridHistorico.ItemsSource = transacoes;

            decimal entradas = transacoes.Where(t => t.Tipo == "Entrada").Sum(t => t.Valor);
            decimal saidas = transacoes.Where(t => t.Tipo == "Saída" || t.Tipo == "Saida").Sum(t => t.Valor);

            txtEntradas.Text = entradas.ToString("C");
            txtSaidas.Text = saidas.ToString("C");
            txtSaldo.Text = (entradas - saidas).ToString("C");
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
                    var pag = new PagamentoFuncionario { Data = DateTime.Now, Nome = inputNomeFunc.Text, Cargo = inputCargo.Text, Valor = val, MesReferencia = inputMesRef.Text };
                    dbHelper.SalvarPagamento(pag);
                }
                else
                {
                    pagamentoEmEdicao.Nome = inputNomeFunc.Text;
                    pagamentoEmEdicao.Cargo = inputCargo.Text;
                    pagamentoEmEdicao.Valor = val;
                    pagamentoEmEdicao.MesReferencia = inputMesRef.Text;

                    dbHelper.AtualizarPagamento(pagamentoEmEdicao);

                    pagamentoEmEdicao = null;
                    btnRegistrarFunc.Content = "Salvar Pagamento";
                }

                inputNomeFunc.Clear();
                inputCargo.Clear();
                inputValorFunc.Clear();
                inputMesRef.Clear();
                AtualizarTelaFuncionarios();
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
                dbHelper.ExcluirPagamento(p);
                AtualizarTelaFuncionarios();
            }
        }

        private void AtualizarTelaFuncionarios()
        {
            var pags = dbHelper.ObterPagamentos();

            gridFuncionarios.ItemsSource = null;
            gridFuncionarios.Items.Clear();
            gridFuncionarios.ItemsSource = pags;

            txtTotalFolha.Text = pags.Sum(p => p.Valor).ToString("C");
            txtQtdPagos.Text = pags.Count.ToString();
        }
    }
}
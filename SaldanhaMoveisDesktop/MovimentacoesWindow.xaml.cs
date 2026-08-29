using System.Linq;
using System.Windows;

namespace SaldanhaMoveisDesktop
{
    public partial class MovimentacoesWindow : Window
    {
        public MovimentacoesWindow()
        {
            InitializeComponent();
            CarregarDados();
        }

        private void CarregarDados()
        {
            // Criamos uma conexão rápida com o banco de dados apenas para essa tela
            using (var db = new AppDbContext())
            {
                // Busca tudo e ordena da data mais nova para a mais velha
                var transacoes = db.Transacoes.OrderByDescending(t => t.Data).ToList();
                gridTodasTransacoes.ItemsSource = transacoes;
            }
        }
    }
}
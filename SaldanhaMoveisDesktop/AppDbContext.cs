using Microsoft.EntityFrameworkCore;

namespace SaldanhaMoveisDesktop
{
    public class AppDbContext : DbContext
    {
        
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Produto> Produtos { get; set; } 
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<ItemVenda> ItensVenda { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }
        public DbSet<PagamentoFuncionario> Pagamentos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=saldanha_dados.db");
        }
    }
}
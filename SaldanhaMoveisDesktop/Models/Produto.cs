using System;

namespace SaldanhaMoveisDesktop
{
    public class Produto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
        public int QuantidadeEstoque { get; set; }
        public bool Ativo { get; set; } 
        public DateTime DataCadastro { get; set; }
    }
}
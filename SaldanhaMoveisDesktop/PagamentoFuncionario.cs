using System;

namespace SaldanhaMoveisDesktop
{
    public class PagamentoFuncionario
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Nome { get; set; }
        public string Cargo { get; set; }
        public decimal Valor { get; set; }
        public string MesReferencia { get; set; }
    }
}
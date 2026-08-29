using System;
using System.Collections.Generic;

namespace SaldanhaMoveisDesktop
{
    public class Venda
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; }

        public decimal ValorTotal { get; set; }
        public DateTime DataVenda { get; set; }
        public string Status { get; set; }

        public List<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
    }
}
using System;
using System.ComponentModel.DataAnnotations;

namespace SaldanhaMoveisDesktop.Models
{
    public class Despesa
    {
        [Key]
        public int Id { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
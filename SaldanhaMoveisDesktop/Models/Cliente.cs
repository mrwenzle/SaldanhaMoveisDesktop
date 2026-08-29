using System;

namespace SaldanhaMoveisDesktop
{
    public class Cliente
    {
        public int Id { get; set; } // O Entity Framework usa o Id como chave primária automática
        public string Nome { get; set; }
        public string CpfCnpj { get; set; }
        public string Telefone { get; set; }
        public string Endereco { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
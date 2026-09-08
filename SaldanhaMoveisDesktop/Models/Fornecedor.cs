using System;

namespace SaldanhaMoveisDesktop
{
    public class Fornecedor
    {
        public int Id { get; set; }
        public string NomeFantasia { get; set; }
        public string Cnpj { get; set; }
        public string Telefone { get; set; }
        public DateTime DataCadastro { get; set; }
    }
}
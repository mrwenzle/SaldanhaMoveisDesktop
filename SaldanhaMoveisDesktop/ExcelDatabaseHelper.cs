using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace SaldanhaMoveisDesktop
{
    public class ExcelDatabaseHelper
    {
        private readonly string pastaPrincipal = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SaldanhaMoveis_Dados");

        public ExcelDatabaseHelper()
        {
            if (!Directory.Exists(pastaPrincipal)) Directory.CreateDirectory(pastaPrincipal);
        }

        private string ObterCaminhoDoArquivoDeHoje(DateTime? data = null)
        {
            DateTime dataAlvo = data ?? DateTime.Now;
            string pastaDoMes = dataAlvo.ToString("MM-yyyy");
            string caminhoCompletoPasta = Path.Combine(pastaPrincipal, pastaDoMes);

            if (!Directory.Exists(caminhoCompletoPasta)) Directory.CreateDirectory(caminhoCompletoPasta);

            return Path.Combine(caminhoCompletoPasta, dataAlvo.ToString("dd-MM-yyyy") + ".xlsx");
        }

        // ==========================================
        // LÓGICA DO FLUXO DE CAIXA
        // ==========================================
        public void SalvarTransacao(Transacao transacao)
        {
            string caminho = ObterCaminhoDoArquivoDeHoje();
            bool arquivoExiste = File.Exists(caminho);

            using (var workbook = arquivoExiste ? new XLWorkbook(caminho) : new XLWorkbook())
            {
                if (!workbook.Worksheets.TryGetWorksheet("Transacoes", out IXLWorksheet worksheet))
                {
                    worksheet = workbook.Worksheets.Add("Transacoes");
                    worksheet.Cell(1, 1).Value = "ID"; worksheet.Cell(1, 2).Value = "Data";
                    worksheet.Cell(1, 3).Value = "Descrição"; worksheet.Cell(1, 4).Value = "Valor";
                    worksheet.Cell(1, 5).Value = "Tipo"; worksheet.Cell(1, 6).Value = "Categoria";
                    var header = worksheet.Range("A1:F1");
                    header.Style.Font.Bold = true; header.Style.Fill.BackgroundColor = XLColor.FromHtml("#D4AF37"); header.Style.Font.FontColor = XLColor.Black;
                }

                var novaLinha = (worksheet.LastRowUsed()?.RowNumber() ?? 1) + 1;
                worksheet.Cell(novaLinha, 1).Value = novaLinha - 1;
                worksheet.Cell(novaLinha, 2).Value = transacao.Data.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(novaLinha, 3).Value = transacao.Descricao;
                worksheet.Cell(novaLinha, 4).Value = transacao.Valor;
                worksheet.Cell(novaLinha, 5).Value = transacao.Tipo;
                worksheet.Cell(novaLinha, 6).Value = transacao.Categoria;
                worksheet.Columns().AdjustToContents();

                if (arquivoExiste) workbook.Save(); else workbook.SaveAs(caminho);
            }
        }

        public void AtualizarTransacao(Transacao transacao)
        {
            string caminho = ObterCaminhoDoArquivoDeHoje(transacao.Data);
            if (!File.Exists(caminho)) return;

            using (var wb = new XLWorkbook(caminho))
            {
                var ws = wb.Worksheet("Transacoes");
                foreach (var linha in ws.RowsUsed().Skip(1).Where(l => l.Cell(1).GetValue<int>() == transacao.Id))
                {
                    linha.Cell(3).Value = transacao.Descricao;
                    linha.Cell(4).Value = transacao.Valor;
                    linha.Cell(5).Value = transacao.Tipo;
                    linha.Cell(6).Value = transacao.Categoria;
                    break;
                }
                wb.Save();
            }
        }

        public void ExcluirTransacao(Transacao transacao)
        {
            string caminho = ObterCaminhoDoArquivoDeHoje(transacao.Data);
            if (!File.Exists(caminho)) return;

            using (var wb = new XLWorkbook(caminho))
            {
                var ws = wb.Worksheet("Transacoes");
                foreach (var linha in ws.RowsUsed().Skip(1).Where(l => l.Cell(1).GetValue<int>() == transacao.Id))
                {
                    linha.Delete();
                    break;
                }
                wb.Save();
            }
        }

        public List<Transacao> ObterTransacoes(string filtro)
        {
            var lista = new List<Transacao>();
            string caminhoPastaMes = Path.Combine(pastaPrincipal, DateTime.Now.ToString("MM-yyyy"));
            if (!Directory.Exists(caminhoPastaMes)) return lista;

            DateTime inicioDaSemana = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek);

            foreach (var arquivo in Directory.GetFiles(caminhoPastaMes, "*.xlsx"))
            {
                using (var wb = new XLWorkbook(arquivo))
                {
                    if (wb.Worksheets.TryGetWorksheet("Transacoes", out IXLWorksheet ws))
                    {
                        foreach (var linha in ws.RowsUsed().Skip(1))
                        {
                            if (DateTime.TryParse(linha.Cell(2).GetString(), out DateTime dt))
                            {
                                if (filtro == "Semana" && dt.Date < inicioDaSemana) continue;
                                lista.Add(new Transacao
                                {
                                    Id = linha.Cell(1).GetValue<int>(),
                                    Data = dt,
                                    Descricao = linha.Cell(3).GetString(),
                                    Valor = linha.Cell(4).GetValue<decimal>(),
                                    Tipo = linha.Cell(5).GetString(),
                                    Categoria = linha.Cell(6).GetString()
                                });
                            }
                        }
                    }
                }
            }
            return lista.OrderByDescending(t => t.Data).ToList();
        }

        // ==========================================
        // LÓGICA DE FUNCIONÁRIOS
        // ==========================================
        public void SalvarPagamento(PagamentoFuncionario pagamento)
        {
            string caminho = ObterCaminhoDoArquivoDeHoje();
            bool arquivoExiste = File.Exists(caminho);

            using (var workbook = arquivoExiste ? new XLWorkbook(caminho) : new XLWorkbook())
            {
                if (!workbook.Worksheets.TryGetWorksheet("Funcionarios", out IXLWorksheet worksheet))
                {
                    worksheet = workbook.Worksheets.Add("Funcionarios");
                    worksheet.Cell(1, 1).Value = "ID"; worksheet.Cell(1, 2).Value = "Data";
                    worksheet.Cell(1, 3).Value = "Nome"; worksheet.Cell(1, 4).Value = "Cargo";
                    worksheet.Cell(1, 5).Value = "Valor"; worksheet.Cell(1, 6).Value = "Mês Ref.";
                    var header = worksheet.Range("A1:F1");
                    header.Style.Font.Bold = true; header.Style.Fill.BackgroundColor = XLColor.FromHtml("#D4AF37"); header.Style.Font.FontColor = XLColor.Black;
                }

                var novaLinha = (worksheet.LastRowUsed()?.RowNumber() ?? 1) + 1;
                worksheet.Cell(novaLinha, 1).Value = novaLinha - 1;
                worksheet.Cell(novaLinha, 2).Value = pagamento.Data.ToString("dd/MM/yyyy");
                worksheet.Cell(novaLinha, 3).Value = pagamento.Nome;
                worksheet.Cell(novaLinha, 4).Value = pagamento.Cargo;
                worksheet.Cell(novaLinha, 5).Value = pagamento.Valor;
                worksheet.Cell(novaLinha, 6).Value = pagamento.MesReferencia;
                worksheet.Columns().AdjustToContents();

                if (arquivoExiste) workbook.Save(); else workbook.SaveAs(caminho);
            }
        }

        public List<PagamentoFuncionario> ObterPagamentos()
        {
            var lista = new List<PagamentoFuncionario>();
            string caminhoPastaMes = Path.Combine(pastaPrincipal, DateTime.Now.ToString("MM-yyyy"));
            if (!Directory.Exists(caminhoPastaMes)) return lista;

            foreach (var arquivo in Directory.GetFiles(caminhoPastaMes, "*.xlsx"))
            {
                using (var wb = new XLWorkbook(arquivo))
                {
                    if (wb.Worksheets.TryGetWorksheet("Funcionarios", out IXLWorksheet ws))
                    {
                        foreach (var linha in ws.RowsUsed().Skip(1))
                        {
                            if (DateTime.TryParse(linha.Cell(2).GetString(), out DateTime dt))
                            {
                                lista.Add(new PagamentoFuncionario
                                {
                                    Id = linha.Cell(1).GetValue<int>(),
                                    Data = dt,
                                    Nome = linha.Cell(3).GetString(),
                                    Cargo = linha.Cell(4).GetString(),
                                    Valor = linha.Cell(5).GetValue<decimal>(),
                                    MesReferencia = linha.Cell(6).GetString()
                                });
                            }
                        }
                    }
                }
            }
            return lista.OrderByDescending(p => p.Data).ToList();
        }

        public void AtualizarPagamento(PagamentoFuncionario pagamento)
        {
            string caminho = ObterCaminhoDoArquivoDeHoje(pagamento.Data);
            if (!File.Exists(caminho)) return;

            using (var wb = new XLWorkbook(caminho))
            {
                var ws = wb.Worksheet("Funcionarios");
                foreach (var linha in ws.RowsUsed().Skip(1).Where(l => l.Cell(1).GetValue<int>() == pagamento.Id))
                {
                    linha.Cell(3).Value = pagamento.Nome;
                    linha.Cell(4).Value = pagamento.Cargo;
                    linha.Cell(5).Value = pagamento.Valor;
                    linha.Cell(6).Value = pagamento.MesReferencia;
                    break;
                }
                wb.Save();
            }
        }

        public void ExcluirPagamento(PagamentoFuncionario pagamento)
        {
            string caminho = ObterCaminhoDoArquivoDeHoje(pagamento.Data);
            if (!File.Exists(caminho)) return;

            using (var wb = new XLWorkbook(caminho))
            {
                var ws = wb.Worksheet("Funcionarios");
                foreach (var linha in ws.RowsUsed().Skip(1).Where(l => l.Cell(1).GetValue<int>() == pagamento.Id))
                {
                    linha.Delete();
                    break;
                }
                wb.Save();
            }
        }
    }
}
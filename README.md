# 🏢 Saldanha Móveis - ERP Desktop 

![C#](https://img.shields.io/badge/C%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-0078D7?style=for-the-badge&logo=windows&logoColor=white)
![SQLite](https://img.shields.io/badge/sqlite-%2307405e.svg?style=for-the-badge&logo=sqlite&logoColor=white)

Um Sistema de Gestão Empresarial (ERP) Desktop completo desenvolvido em **C#** e **WPF**, focado em performance, integridade de dados e arquitetura limpa. 

O projeto evoluiu de uma persistência de dados baseada em planilhas para uma estrutura relacional robusta utilizando **Entity Framework Core** e **SQLite**, garantindo transações seguras e integração completa entre os módulos de vendas, estoque e fluxo de caixa.

## 🚀 Principais Funcionalidades

* **Frente de Caixa (PDV):** Sistema de vendas dinâmico com carrinho de compras. O fechamento de uma venda realiza automaticamente a baixa no estoque e lança a receita no fluxo de caixa (Transação Atômica).
* **Dashboard Gerencial:** Painel analítico com indicadores de performance (KPIs) em tempo real, exibindo faturamento, despesas e lucro líquido mensal.
* **Visualização de Dados (Gráficos):** Integração com `LiveCharts` para renderização de gráficos de barras e rosca, facilitando a análise financeira.
* **Gestão de Estoque & Clientes (CRUD):** Módulos completos para cadastro, edição e exclusão, com validação de dados.
* **Auditoria Financeira:** Tela dedicada ao histórico de movimentações, utilizando conversores customizados (`IValueConverter`) para identificação visual de entradas e saídas.

## 🛠️ Tecnologias e Arquitetura

* **Linguagem:** C# (.NET)
* **Interface (UI):** Windows Presentation Foundation (WPF) e XAML, com foco em uma identidade visual *Enterprise* responsiva e moderna.
* **ORM:** Entity Framework Core (Migrações automatizadas e Code-First).
* **Banco de Dados:** SQLite (Embarcado, sem necessidade de servidores externos).
* **Bibliotecas Externas:** LiveCharts (Gráficos).
* **Padrões Aplicados:** Princípios de Clean Code, consultas otimizadas com LINQ e isolamento de responsabilidades.

## 📷 Telas do Sistema


| Dashboard Financeiro | Ponto de Venda (PDV) |
| :---: | :---: |
| <img width="1084" height="750" alt="image" src="https://github.com/user-attachments/assets/4103fe4b-206d-4e54-a257-85bc197182b7" /> | <img width="1083" height="752" alt="image" src="https://github.com/user-attachments/assets/52741d3d-478b-445d-be17-0804ba0b8d68" /> |

## ⚙️ Como Executar o Projeto

1. Clone este repositório:
   ```bash
   git clone [https://github.com/mrwenzle/SaldanhaMoveisDesktop.git](https://github.com/mrwenzle/SaldanhaMoveisDesktop.git)

# Saldanha Móveis - ERP Desktop 🖥️💼

![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![.Net](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![WPF](https://img.shields.io/badge/WPF-0078D7?style=for-the-badge&logo=windows&logoColor=white)

> Um sistema de gestão empresarial (ERP) desktop desenvolvido do zero para controle financeiro e folha de pagamentos, focado em alta performance e experiência do usuário (UI/UX).

## 📌 Sobre o Projeto
O **Saldanha Móveis ERP** foi arquitetado para resolver a necessidade de controle de fluxo de caixa e gestão de pagamentos de forma ágil. A aplicação foi construída com foco em uma interface rica, validações de segurança e persistência de dados local sem a necessidade de instâncias SQL pesadas, utilizando geração dinâmica de planilhas.

## 🚀 Funcionalidades (Versão 1.0)
- **Autenticação Segura:** Tela de login blindada para acesso restrito aos dados da empresa.
- **Módulo de Fluxo de Caixa:** 
  - CRUD completo de transações (Entradas e Saídas).
  - Cálculo de saldo automático em tempo real.
  - Filtros dinâmicos por período (Mês atual, Semana atual).
- **Módulo de Recursos Humanos:**
  - Lançamento e edição de folha de pagamento de funcionários.
  - Controle de cargos e meses de referência.
- **Persistência de Dados (ClosedXML):** 
  - Geração e estruturação autônoma de banco de dados em arquivos `.xlsx`.
  - Organização automática por diretórios e datas.

## 🛠️ Tecnologias Utilizadas
- **Linguagem:** C# (.NET 10.0)
- **Interface Visual:** WPF (Windows Presentation Foundation) / XAML
- **Banco de Dados/Engine:** ClosedXML (Manipulação de Excel nativa)
- **Arquitetura:** Event-Driven Programming e separação de responsabilidades (Helpers).

## 🗺️ Roadmap & Próximos Passos
Este projeto foi desenvolvido com uma fundação sólida, visando escalabilidade corporativa. As próximas fases de arquitetura incluem:
- [ ] **Fase 2:** Planejamento e modelagem da migração dos dados locais (*Custom Objects*).
- [ ] **Fase 3:** Reconstrução da regra de negócios e interface visual como um aplicativo nativo **Salesforce Lightning**.


# TechSystem – Sistema de Gerenciamento de Chamados
<p align="center"> <img src="https://i.ibb.co/8gVGvs8C/Tech-System.jpg" width="200"> </p>


**Visão Geral**

O TechSystem é um sistema desktop desenvolvido em C# (WinForms) para gerenciamento interno de chamados.

O objetivo é simplificar a abertura de chamados, automatizar análises com IA e oferecer uma interface moderna, clara e eficiente.


**Funcionalidades**

- Dashboard com métricas gerais

- Gráficos de status dos chamados

- Abertura de chamados com análise automatizada via IA

- Relatórios completos com filtros

- Edição rápida de status

- Sistema de notificações interno

- Interface organizada e amigável
 

**Tecnologias Utilizadas**

- C# (.NET Framework)

- WinForms

- SQL Server

- Dapper

- OpenAI API

- Git e GitHub
  

**Configuração do Projeto**

O repositório inclui apenas um arquivo de configuração:

App.config.example → modelo seguro para o usuário preencher localmente

O arquivo real App.config não é incluído no repositório por motivos de segurança.


**O que o usuário deve fazer**

Copiar o arquivo App.config.example

Renomear a cópia para App.config

Inserir suas próprias credenciais dentro desse arquivo:

- ConnectionString do banco de dados

- Chave da API da OpenAI

O formato interno é exatamente o mesmo de qualquer arquivo App.config padrão do .NET, então basta substituir os valores pelos seus próprios.

Nenhum dado sensível é enviado ao GitHub: o arquivo real fica ignorado pelo .gitignore.


**Como Executar**

Clone o repositório:

git clone https://github.com/sstardustt/TechSystem.git


Copie o arquivo:

- App.config.example → App.config


Preencha:

- ConnectionString

- OpenAIApiKey

Abra o projeto no Visual Studio

Execute com F5


**Roadmap**

- Tema claro/escuro

- Sistema de permissões por usuário

- Logs de auditoria

- Exportação de relatórios

- Dashboard totalmente customizável


**Contribuições**

Sugestões e melhorias são bem-vindas.


**Licença**

Projeto licenciado sob MIT.

# Sistema de Ordem de Serviço

Aplicativo desktop para gerenciamento de ordens de serviço, orçamentos e movimentações de caixa. O sistema foi desenvolvido em C# com WPF e roda no Windows.

## Requisitos

- Windows 10 ou Windows 11 (64 bits).
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) para compilar e executar o código-fonte.
- Git para clonar o repositório, caso ainda não tenha os arquivos do projeto.


O projeto não depende de banco de dados ou serviços externos. Os dados do caixa são salvos em arquivos JSON locais. Para imprimir o fechamento do caixa, também é necessário ter uma impressora configurada no Windows.

> Este é um aplicativo WPF para Windows; não é executado nativamente em macOS ou Linux.

## Executar pelo terminal

1. Instale o .NET 10 SDK e o Git. Depois, abra um novo PowerShell ou Terminal.
2. Clone o repositório e entre na pasta do projeto:

   ```powershell
   git clone https://github.com/deyvidpechincha/ordem-de-servico.git
   cd ordem-de-servico
   ```

3. Restaure as dependências e execute:

   ```powershell
   dotnet restore .\Trabalho.OS\Trabalho.OS.csproj
   dotnet run --project .\Trabalho.OS\Trabalho.OS.csproj
   ```

Não é necessário instalar dependências NuGet adicionais manualmente. O SDK restaura as dependências declaradas pelo projeto.

## Executar pelo Visual Studio

1. Instale uma versão do Visual Studio compatível com .NET 10 e selecione a carga de trabalho **Desenvolvimento para desktop com .NET**.
2. Abra `Trabalho.OS.slnx` na raiz do repositório.
3. Selecione `Trabalho.OS` como projeto de inicialização e pressione **F5** (ou **Ctrl+F5** para executar sem depuração).

## Criar uma versão para outro computador

Para gerar uma publicação Windows x64 que inclui o runtime do .NET, execute na raiz do repositório:

```powershell
dotnet publish .\Trabalho.OS\Trabalho.OS.csproj -c Release -r win-x64 --self-contained true
```

A pasta publicada fica em `Trabalho.OS\bin\Release\net10.0-windows\win-x64\publish\`. Copie **todo o conteúdo** dessa pasta para o outro computador e execute `Trabalho.OS.exe`. Com a publicação autocontida, o computador de destino não precisa ter o .NET instalado; ainda precisa ser Windows compatível com x64. Para outra arquitetura, gere a publicação com o runtime correspondente.

## Dados do aplicativo

Os arquivos do caixa ficam na pasta `DadosCaixa`, localizada junto ao executável. O aplicativo cria essa pasta quando salva movimentações. Para manter os dados ao trocar de computador ou atualizar a publicação, faça uma cópia de segurança e copie a pasta `DadosCaixa` para junto do novo executável. Execute o aplicativo em uma pasta na qual seu usuário tenha permissão de gravação; pastas protegidas do Windows podem impedir que os dados sejam salvos.

Os arquivos de caixa são dados locais e não são sincronizados automaticamente entre computadores.
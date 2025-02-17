using System;
using CriarConta;
using MySql.Data.MySqlClient;
using System.Timers;
using System.Linq;

namespace GerenciadorContas
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            ContaPoupanca.RodarTimer();
            bool repetirMenu = true;
            while (repetirMenu)
            {
                int usuarioResposta = menu();
                switch (usuarioResposta)
                {
                    case 1:
                    GerenciarContaMenu();
                    break;

                    case 2:
                    CriarNovaContaMenu();
                    break;

                    case 3:
                    Console.WriteLine("Fim do programa");
                    repetirMenu = false;
                    break;

                    default:
                    Console.WriteLine("Resposta inválida");
                    break;
                }
            }
        }

        static int menu()
        {
            Console.Clear();
            Console.WriteLine("Digite a ação desejada pelo número correspondente:");
            Console.WriteLine("1.Gerenciar Conta\t 2.Criar Nova Conta\t 3.Sair");
            string inputResposta = Console.ReadLine() ?? "";
            if (string.IsNullOrEmpty(inputResposta) || !int.TryParse(inputResposta, out int usuarioResposta))
            {
                Console.WriteLine("Resposta inválida");
                return -1;
            }
            return usuarioResposta;
        }

        static void GerenciarContaMenu()
        {
            Console.Clear();
            Console.WriteLine("Digite seu CPF:");
            string CPFid = Console.ReadLine()?.Trim().Replace(".","").Replace("-","") ?? "";
            if (string.IsNullOrEmpty(CPFid) || CPFid.Length != 11 || !CPFid.All(char.IsDigit)) 
            {
                Console.WriteLine("CPF inválido");
                Console.ReadKey();
                return;
            }
            
            Conta? contaMod = Conta.BuscarConta(CPFid);
            if (contaMod == null)
            {
                Console.WriteLine("Conta não encontrada");
                Console.ReadKey();
                return;
            }
            Console.Clear();
            contaMod.ExibirSaldo();
            Console.WriteLine();

            Console.WriteLine("Digite a ação desejada pelo número correspondente:");
            Console.WriteLine("1.Sacar \t 2.Depositar \t 3.Transferir \t 4.Excluir Conta \t 5.Voltar ao menu principal");
            string inputGMResposta = Console.ReadLine() ?? "";

            if (string.IsNullOrEmpty(inputGMResposta) || !int.TryParse(inputGMResposta, out int gmResposta))
            {
                Console.WriteLine("Resposta inválida");
                Console.ReadKey();
                return;
            }

            switch (gmResposta)
            {
                case 1:
                contaMod.Sacar();
                break;

                case 2:
                contaMod.Depositar();
                break;

                case 3:
                contaMod.Transferir();
                break;

                case 4:
                contaMod.Excluir();
                break;

                case 5:
                Console.WriteLine("Voltando ao menu principal...");
                break;

                default:
                Console.WriteLine("Reposta inválida");
                Console.ReadKey();
                break;
            }
        
        }

        static void CriarNovaContaMenu()
        {
            Console.Clear();
            Console.WriteLine("Escolha o tipo de conta pelo número correspondente");
            Console.WriteLine("1.Conta Corrente\t 2.Conta Poupança\t 3.Voltar");
            string inputResposta = Console.ReadLine() ?? "";
            if (string.IsNullOrEmpty(inputResposta) || !int.TryParse(inputResposta, out int usuarioResposta))
            {
                Console.Clear();
                Console.WriteLine("Resposta inválida");
                Console.ReadKey();
                return;
            }
            switch (usuarioResposta)
            {
                case 1:
                CriarNovaContaCorrente();
                break;

                case 2:
                CriarNovaContaPoupanca();
                break;

                case 3:
                Console.WriteLine("Voltando ao menu principal...");
                break;

                default:
                Console.WriteLine("Resposta inválida");
                Console.ReadKey();
                break;
            }
        }

        static void CriarNovaContaCorrente()
        {
            Console.Clear();
            Console.WriteLine("Criando nova conta corrente...");
            Console.WriteLine("Digite seu CPF:");
            string inputcpf = Console.ReadLine()?.Replace(".","").Replace("-","") ?? "";
            if (string.IsNullOrEmpty(inputcpf) || inputcpf.Length != 11 || !inputcpf.All(char.IsDigit))
            {
                Console.WriteLine("CPF inválido");
                Console.ReadKey();
                return;
            }
            
            Console.Clear();
            Console.WriteLine("Digite seu nome completo:");
            string inputNome = Console.ReadLine()?.ToLower() ?? "";
            if (string.IsNullOrEmpty(inputNome) || inputNome == "")
            {
                Console.WriteLine("Nome inválido");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine("Digite a quantia que deseja depositar:");
            string inputSaldo = Console.ReadLine() ?? "";
            if (string.IsNullOrEmpty(inputSaldo) || !decimal.TryParse(inputSaldo, out decimal SaldoDep) || SaldoDep <= 0)
            {
                Console.WriteLine("Valor inválido");
                Console.ReadKey();
                return;
            }

            ContaCorrente conta = new ContaCorrente(inputcpf, inputNome, SaldoDep, "Corrente");

            conta.SalvarConta();
            Console.Clear();
            Console.WriteLine("Conta corrente criada com sucesso!");
            conta.ExibirSaldo();
            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para continuar");
            Console.ReadKey();
        }

        static void CriarNovaContaPoupanca()
        {
            Console.Clear();
            Console.WriteLine("Criando nova conta poupança...");
            Console.WriteLine("Digite seu CPF:");
            string inputcpf = Console.ReadLine()?.Replace(".","").Replace("-","") ?? "";
            if (string.IsNullOrEmpty(inputcpf) || inputcpf.Length != 11 || !inputcpf.All(char.IsDigit))
            {
                Console.Clear();
                Console.WriteLine("CPF inválido");
                Console.ReadKey();
                return;
            }
            
            Console.WriteLine("Digite seu nome completo:");
            string inputNome = Console.ReadLine()?.ToLower() ?? "";
            if (string.IsNullOrEmpty(inputNome) || inputNome == "")
            {
                Console.Clear();
                Console.WriteLine("Nome inválido");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Digite a quantia que deseja depositar:");
            string inputSaldo = Console.ReadLine() ?? "";
            if (string.IsNullOrEmpty(inputSaldo) || !decimal.TryParse(inputSaldo, out decimal SaldoDep) || SaldoDep <= 0)
            {
                Console.Clear();
                Console.WriteLine("Valor inválido");
                Console.ReadKey();
                return;
            }

            ContaPoupanca conta = new ContaPoupanca(inputcpf, inputNome, SaldoDep, "Poupança");

            conta.SalvarConta();
            Console.Clear();
            Console.WriteLine("Conta poupança criada com sucesso!");
            conta.ExibirSaldo();
            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para continuar");
            Console.ReadKey();
        }
    }
}
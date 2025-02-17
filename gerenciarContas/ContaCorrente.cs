using System;
using MySql.Data.MySqlClient;

namespace CriarConta
{
    class ContaCorrente : Conta
    {
        private decimal taxaSaque { get; set; }
        private string tipoConta { get; set; }

        public ContaCorrente(string cpf, string NomeUsuario, decimal SaldoConta, string TipoConta) : base(cpf, NomeUsuario, SaldoConta)
        {
            taxaSaque = 5.00m;
            tipoConta = TipoConta;
        }

        public override void SalvarConta()
        {
            using(var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string query = "INSERT INTO contas (CPF, Nome, Saldo, TipoConta) VALUES (@CPF, @NomeUSuario, @SaldoConta, @tipoConta)";

                using(var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CPF", CPF);
                    command.Parameters.AddWithValue("@NomeUsuario", nomeUsuario);
                    command.Parameters.AddWithValue("@SaldoConta", saldoConta);
                    command.Parameters.AddWithValue("@tipoConta", tipoConta);

                    command.ExecuteNonQuery();
                }
            }
        }

        public override void Sacar()
        {
            Console.Clear();
            Console.WriteLine($"Saldo: {saldoConta :C}");
            Console.WriteLine($"Ao sacar será cobrado uma taxa de {taxaSaque :C} \nDigite a quantidade que deseja sacar:");

            string inputValorSaque = Console.ReadLine() ?? "";

            if (string.IsNullOrEmpty(inputValorSaque) || !decimal.TryParse(inputValorSaque, out decimal valorSaque) || valorSaque <= 0 || valorSaque > saldoConta)
            {
                Console.WriteLine("Valor de saque inválido ou insuficiente");
                Console.ReadKey();
                return;
            }
            

            if (valorSaque + taxaSaque > saldoConta)
            {
                Console.WriteLine($"Valor do saque: {valorSaque :C} com taxa de saque ({taxaSaque :C}) insuficiente");
                Console.ReadKey();
                return;
            }

            using(var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string query = "UPDATE contas SET Saldo = Saldo - (@valorSaque + @taxaSaque) WHERE CPF = @CPF AND Saldo >= @valorSaque + @taxaSaque";

                using(var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CPF", CPF);
                    command.Parameters.AddWithValue("@valorSaque", valorSaque);
                    command.Parameters.AddWithValue("@taxaSaque", taxaSaque);

                    int operacao = command.ExecuteNonQuery();

                    if (operacao > 0)
                    {
                        Console.Clear();
                        saldoConta -= (valorSaque + taxaSaque);
                        Console.WriteLine($"Saque de {valorSaque :C} (+ {taxaSaque :C} de juros) realizado com sucesso");
                        Console.WriteLine($"Saldo atual: {saldoConta :C}");
                        Console.WriteLine("Pressione qualquer tecla para continuar");
                        Console.ReadKey();
                    }
                    else
                    {
                        throw new Exception("Erro ao sacar valor do banco");
                    }
                }
            }
        }

        public override void Depositar()
        {
            base.Depositar();
        }

        public override void Transferir()
        {
            base.Transferir();
        }

        public override void Excluir()
        {
            base.Excluir();
        }

        public override void ExibirSaldo()
        {
            Console.WriteLine($"Nome do usuário: {nomeUsuario}\t CPF: {CPF}\t Saldo bancário: {saldoConta :c}\t Tipo da conta: {tipoConta}");
        }
    }
}
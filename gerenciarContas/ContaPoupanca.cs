using System;
using MySql.Data.MySqlClient;
using System.Timers;

namespace CriarConta
{
    class ContaPoupanca : Conta
    {
        private static System.Timers.Timer rendimentoTimer = new System.Timers.Timer();
        private static decimal jurosRendimento = 0.02m;
        private string tipoConta;
        private decimal limiteSaque = 2000.00m;

        public ContaPoupanca(string cpf, string NomeUsuario, decimal SaldoConta, string TipoConta) : base(cpf, NomeUsuario, SaldoConta)
        {
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
            Console.WriteLine($"limite de saque de até {limiteSaque :C}");
            Console.WriteLine($"Saldo: {saldoConta :C} \nDigite a quantidade que deseja sacar:");

            string inputValorSaque = Console.ReadLine() ?? "";

            if (string.IsNullOrEmpty(inputValorSaque) || !decimal.TryParse(inputValorSaque, out decimal valorSaque) || valorSaque <= 0 || valorSaque > limiteSaque)
            {
                Console.WriteLine("Valor de saque inválido");
                Console.ReadKey();
                return;
            }

            if (valorSaque > saldoConta)
            {
                Console.WriteLine("Valor de saque insuficiente");
                Console.ReadKey();
                return;
            }

            using(var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string query = "UPDATE contas SET Saldo = Saldo - @valorSaque WHERE CPF = @CPF AND Saldo >= @valorSaque";

                using(var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CPF", CPF);
                    command.Parameters.AddWithValue("@valorSaque", valorSaque);

                    int operacao = command.ExecuteNonQuery();

                    if (operacao > 0)
                    {
                        saldoConta -= valorSaque;
                        Console.WriteLine($"Saque de {valorSaque :C} realizado com sucesso");
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

        public static void RenderPoupanca(object? source, ElapsedEventArgs e)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                try
                {
                    string query = "UPDATE contas SET Saldo =  Saldo + (Saldo * @jurosRendimento) WHERE TipoConta = 'Poupanca' AND Saldo + (Saldo * @jurosRendimento) < 99999999.99";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@jurosRendimento", jurosRendimento);
                        int operacaoRender = command.ExecuteNonQuery();
                        if (operacaoRender == 0)
                        {
                            throw new Exception("Erro ao adicionar juros de renda nas contas poupanças");
                        }
                    }
                }
                catch (System.Exception)
                {
                    throw new Exception("Erro ao render conta poupança");
                }
            }
        }

        public static void RodarTimer()
        {
            rendimentoTimer = new System.Timers.Timer(60000);
            rendimentoTimer.Elapsed += RenderPoupanca;
            rendimentoTimer.AutoReset = true;
            rendimentoTimer.Enabled = true;
        }

        public override void ExibirSaldo()
        {
            Console.WriteLine($"Nome do usuário: {nomeUsuario}\t CPF: {CPF}\t Saldo bancário: {saldoConta :c}\t Tipo da conta: {tipoConta}");
        }

    }
}
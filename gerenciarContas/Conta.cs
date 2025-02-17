using System;
using MySql.Data.MySqlClient;
using System.Linq;

namespace CriarConta
{
    class Conta
    {
        protected string CPF { get; set; }
        protected string nomeUsuario { get; set; }
        protected decimal saldoConta { get; set; }


        public Conta(string cpf, string NomeUsuario, decimal SaldoConta)
        {
            CPF = cpf;
            nomeUsuario = NomeUsuario;
            saldoConta = SaldoConta;
        }

        public virtual void SalvarConta()
        {
            using(var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string query = "INSERT INTO contas (CPF, Nome, Saldo, TipoConta) VALUES (@CPF, @NomeUSuario, @SaldoConta)";

                using(var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CPF", CPF);
                    command.Parameters.AddWithValue("@NomeUsuario", nomeUsuario);
                    command.Parameters.AddWithValue("@SaldoConta", saldoConta);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static Conta? BuscarConta(string cpf)
        {
            using(var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                
                string query = "SELECT * FROM contas WHERE CPF = @cpf";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@cpf", cpf);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string tipoConta = reader["TipoConta"]?.ToString() ?? "Indefinido";

                            if (tipoConta == "Corrente")
                            {
                                return new ContaCorrente
                                (
                                    reader["CPF"]?.ToString() ?? "",
                                    reader["Nome"]?.ToString() ?? "Desconhecido",
                                    Convert.ToDecimal(reader["Saldo"] ?? 0),
                                    tipoConta
                                );
                            }
                            else if (tipoConta == "Poupanca")
                            {
                                return new ContaPoupanca
                                (
                                    reader["CPF"]?.ToString() ?? "",
                                    reader["Nome"]?.ToString() ?? "Desconhecido",
                                    Convert.ToDecimal(reader["Saldo"] ?? 0),
                                    tipoConta
                                );
                            }
                            else
                            {
                                return new Conta
                                (
                                    reader["CPF"]?.ToString() ?? "",
                                    reader["Nome"]?.ToString() ?? "Desconhecido",
                                    Convert.ToDecimal(reader["Saldo"] ?? 0)
                                );
                            }
                        }
                    }
                }
            }
            return null;
        }
        
        public virtual void Sacar()
        {
            Console.Clear();
            Console.WriteLine($"Saldo: {saldoConta :C}");
            Console.WriteLine("Digite a quantidade que deseja sacar:");

            string inputValorSaque = Console.ReadLine() ?? "";

            if (string.IsNullOrEmpty(inputValorSaque) || !decimal.TryParse(inputValorSaque, out decimal valorSaque) || valorSaque <= 0)
            {
                Console.WriteLine("Valor de saque inválido");
                Console.ReadKey();
                return;
            }

            if (saldoConta < valorSaque)
            {
                Console.WriteLine("Valor insuficiente");
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
                        Console.Clear();
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

        public virtual void Depositar()
        {
            Console.Clear();
            Console.WriteLine($"Saldo: {saldoConta}");
            Console.WriteLine("Digite o valor a ser depositado:");

            string inputValorDepositar = Console.ReadLine() ?? "";

            if (string.IsNullOrEmpty(inputValorDepositar) || !decimal.TryParse(inputValorDepositar, out decimal valorDepositar) || valorDepositar <= 0)
            {
                Console.WriteLine("Valor inválido");
                Console.ReadKey();
                return;                
            }

            if (saldoConta + valorDepositar > 9999999999999.99m)
            {
                Console.WriteLine("O valor a ser depositado ultrapassaria o limite do banco");
                Console.ReadKey();
                return;
            }

            using(var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                string query = "UPDATE contas SET Saldo = Saldo + @valorDepositar WHERE CPF = @CPF";

                using(var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@valorDepositar", valorDepositar);
                    command.Parameters.AddWithValue("@CPF", CPF);

                    int operacao = command.ExecuteNonQuery();

                    if (operacao > 0)
                    {
                        Console.Clear();
                        saldoConta += valorDepositar;
                        Console.WriteLine($"Depósito de: {valorDepositar :C} realizado com sucesso");
                        Console.WriteLine($"Saldo atual: {saldoConta :C}");
                        Console.WriteLine("Pressione qualquer tecla para continuar");
                        Console.ReadKey();
                    }
                    else
                    {
                        throw new Exception("Erro ao depositar valor no banco");
                    }
                }
            }
        }

        public virtual void Transferir()
        {
            Console.Clear();
            Console.WriteLine("Digite o CPF do destinatário:");
            string cpfDestinatario = Console.ReadLine()?.Trim().Replace(".","").Replace("-","") ?? "";

            if (string.IsNullOrEmpty(cpfDestinatario) || cpfDestinatario.Length != 11 || !cpfDestinatario.All(char.IsDigit) || cpfDestinatario == CPF)
            {
                Console.WriteLine("CPF inválido");
                Console.ReadKey();
                return;
            }

            string nomeDestinatario;

            using(var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string buscarNomeQuery = "SELECT Nome FROM contas WHERE CPF = @cpfDestinatario";

                using(var command = new MySqlCommand(buscarNomeQuery, connection))
                {
                    command.Parameters.AddWithValue("@cpfDestinatario", cpfDestinatario);

                    using(var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nomeDestinatario = reader["Nome"]?.ToString() ?? "Indefinido";
                        }
                        else
                        {
                            Console.WriteLine("Conta não encontrada");
                            Console.ReadKey();
                            return;
                        }
                    }
                }
            }

            Console.Clear();
            Console.WriteLine($"Transferencia para: {nomeDestinatario}\t CPF: {cpfDestinatario}");
            Console.WriteLine("Deseja continuar a transferencia? (S/N)");
            char resposta = char.ToUpper(Console.ReadKey().KeyChar);
            if (resposta != 'S')
            {
                Console.WriteLine();
                Console.WriteLine("Transferência cancelada");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine($"Saldo: {saldoConta :C}");
            Console.WriteLine("Digite o valor a ser transferido:");
            string inputValorTransferir = Console.ReadLine() ?? "";

            if (string.IsNullOrEmpty(inputValorTransferir) || !decimal.TryParse(inputValorTransferir, out decimal valorTransferir) || valorTransferir <= 0 || valorTransferir > saldoConta)
            {
                Console.WriteLine("Valor inválido ou insuficiente");
                Console.ReadKey();
                return;
            }

            Console.Clear();
            Console.WriteLine($"Tem certeza que deseja transferir {valorTransferir :C} para: {nomeDestinatario}? (S/N)");
            char respostaFinal = char.ToUpper(Console.ReadKey().KeyChar);
            if (respostaFinal != 'S')
            {
                Console.WriteLine("Transferência cancelada");
                Console.ReadKey();
                return;
            }


            using(var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                using(var transacao = connection.BeginTransaction())
                {
                    try 
                    {
                        string queryRetirar = "UPDATE contas SET Saldo = Saldo - @valorTransferir WHERE CPF = @CPF AND Saldo >= @valorTransferir";
                        
                        using(var command = new MySqlCommand(queryRetirar, connection, transacao))
                        {
                            command.Parameters.AddWithValue("@CPF", CPF);
                            command.Parameters.AddWithValue("@valorTransferir", valorTransferir);
                            int operacaoTransferencia = command.ExecuteNonQuery();

                            if (operacaoTransferencia == 0)
                            {
                                throw new Exception("Saldo insuficiente");
                            }
                        }

                        string queryTransferir = "UPDATE contas SET Saldo = Saldo + @valorTransferir WHERE CPF = @cpfDestinatario";

                        using(var command = new MySqlCommand(queryTransferir, connection, transacao))
                        {
                            command.Parameters.AddWithValue("@valorTransferir", valorTransferir);
                            command.Parameters.AddWithValue("@cpfDestinatario", cpfDestinatario);
                            int operacaoTransferir = command.ExecuteNonQuery();

                            if (operacaoTransferir == 0)
                            {
                                throw new Exception("Erro ao transferir saldo ao destinatário");
                            }
                        }

                        transacao.Commit();
                        Console.Clear();
                        Console.WriteLine($"Transferência de: {valorTransferir :C}\t para: {nomeDestinatario} concluída com sucesso");
                        Console.WriteLine();
                        Console.WriteLine("Pressione qualquer tecla para continuar");
                        Console.ReadKey();
                    }
                    catch (System.Exception)
                    {
                        transacao.Rollback();
                        throw new Exception("Erro ao transferir saldo");
                    }
                }
            }
        }

        public virtual void Excluir()
        {
            Console.Clear();
            Console.WriteLine("Tem certeza que deseja excluir a conta? (S/N)");
            char respostaExcluir = char.ToUpper(Console.ReadKey().KeyChar);;
            if (respostaExcluir != 'S')
            {
                Console.WriteLine("Exclusão de conta cancelada");
                return;
            }
            
            using(var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                string queryExcluir = "DELETE FROM contas WHERE CPF = @CPF";

                using(var command = new MySqlCommand(queryExcluir, connection))
                {
                    command.Parameters.AddWithValue("@CPF", CPF);
                    int operacaoExcluir = command.ExecuteNonQuery();

                    if (operacaoExcluir > 0)
                    {
                        Console.WriteLine("Conta excluída com sucesso");
                        Console.WriteLine($"Nome: {nomeUsuario}\t CPF:{CPF}");
                        Console.WriteLine("Pressione qualquer tecla para continuar");
                        Console.ReadKey();
                    }
                    else
                    {
                        throw new Exception("Erro ao excluir conta bancária");
                    }
                }
            }
        }

        public virtual void ExibirSaldo()
        {
            Console.WriteLine($"Nome do usuário: {nomeUsuario}\t CPF: {CPF}\t Saldo bancário: {saldoConta :c}\t Tipo da conta: Indefinido");
        }
    }
}
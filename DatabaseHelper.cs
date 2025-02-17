using MySql.Data.MySqlClient;
using System;

public static class DatabaseHelper
{
    private const string ConnectionString = "Server=localhost;Database=GerenciadorContas;Uid=root;Pwd=senha";

    public static MySqlConnection GetConnection()
    {
        return new MySqlConnection(ConnectionString);
    }
}

using System;
using System.Windows;
using Microsoft.Data.Sqlite;
using Regraphik.Data;

namespace Regraphik.Data;

public class UsuarioRepository
{
    // --- MÉTODO PARA CADASTRAR ---
    public bool CadastrarUsuario(string nome, string cpf, string email, string login, string senha)
    {
        try
        {
            using var conn = Database.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            
            // Adicionamos 'DataCadastro' e o valor 'datetime('now')' para satisfazer a regra do banco
            cmd.CommandText = @"
                INSERT INTO CadastroUsuarios (Nome, CPF, Email, Login, Senha, DataCadastro) 
                VALUES (@nome, @cpf, @email, @login, @senha, datetime('now'));";

            cmd.Parameters.AddWithValue("@nome", nome);
            cmd.Parameters.AddWithValue("@cpf", cpf);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@senha", senha);

            int linhasAfetadas = cmd.ExecuteNonQuery();
            return linhasAfetadas > 0;
        }
        catch (Exception ex)
        {
            // Mostra o erro real caso algo ainda dê errado
            MessageBox.Show("ERRO AO CADASTRAR: " + ex.Message);
            return false;
        }
    }

    // --- MÉTODO PARA AUTENTICAR (LOGIN) ---
    public bool AutenticarUsuario(string login, string senha)
    {
        try
        {
            using var conn = Database.GetConnection();
            conn.Open();

            var cmd = conn.CreateCommand();
            // Verifica se existe um usuário com este login e senha
            cmd.CommandText = "SELECT COUNT(*) FROM CadastroUsuarios WHERE Login = @login AND Senha = @senha";
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@senha", senha);

            long resultado = (long)cmd.ExecuteScalar();
            return resultado > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show("ERRO AO AUTENTICAR: " + ex.Message);
            return false;
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Windows.Forms;

using Guna.UI2.WinForms;
using System.Security.Cryptography;

namespace PrivateGradeDeliverySystem1
{
    public partial class ResetCodeForm : Form
    {
        DBconnection db = new DBconnection(false);
        private string userEmail;
        public ResetCodeForm()
        {
            InitializeComponent();

        }
        public ResetCodeForm(string email)
        {
            InitializeComponent();
            userEmail = email;
            txtEmail.Text = email;
            txtEmail.Enabled = false;
            btnResetPassword.Click += BtnResetPassword_Click; // تأكد من ربط الزر
            btnCancel.Click += BtnCancel_Click;
        }

        private void ResetCodeForm_Load(object sender, System.EventArgs e)
        {
            txtNewPassword.UseSystemPasswordChar = true;
            txtConfirmPassword.UseSystemPasswordChar = true;
        }

        private bool IsStrongPassword(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsDigit) &&
                   password.Any(ch => !char.IsLetterOrDigit(ch));
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private void BtnCancel_Click(object sender, System.EventArgs e)
        {
            this.Hide();
            var login = new LoginForm();
            login.Show();
        }

        private void BtnResetPassword_Click(object sender, System.EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            string code = txtResetCode.Text.Trim();
            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Passwords do not match!");
                return;
            }


            string connStr = "server=localhost;database=universitydb;user=root;password=12345678;SslMode=none;";
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                // التحقق من الكود وصلاحيته
                string query = @"
                    SELECT pr.id, u.Id AS user_id
                    FROM password_resets pr
                    JOIN Users u ON u.Id = pr.user_id
                    WHERE u.email = @Email
                      AND pr.reset_code = @Code
                      AND pr.used = 0
                      AND pr.expires_at > NOW()";

                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Code", code);

                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int resetId = reader.GetInt32("id");
                    int userId = reader.GetInt32("user_id");
                    reader.Close();

                    // تحديث كلمة المرور (باستخدام BCrypt)
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

                    string updatePassword = "UPDATE Users SET Password = @Password WHERE Id = @UserId";
                    MySqlCommand cmdUpdate = new MySqlCommand(updatePassword, conn);
                    cmdUpdate.Parameters.AddWithValue("@Password", hashedPassword);
                    cmdUpdate.Parameters.AddWithValue("@UserId", userId);
                    cmdUpdate.ExecuteNonQuery();

                    // تعليم الكود أنه تم استخدامه
                    string updateCode = "UPDATE password_resets SET used = 1 WHERE id = @ResetId";
                    MySqlCommand cmdUsed = new MySqlCommand(updateCode, conn);
                    cmdUsed.Parameters.AddWithValue("@ResetId", resetId);
                    cmdUsed.ExecuteNonQuery();

                    MessageBox.Show("Password reset successfully!");
                }
                else
                {
                    MessageBox.Show("Invalid or expired reset code!");
                }

                conn.Close();
            }

        }


    }
}

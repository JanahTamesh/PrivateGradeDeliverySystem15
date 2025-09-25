
using System.Drawing;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Guna.UI2.WinForms;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PrivateGradeDeliverySystem1
{
    public partial class LoginForm : Form
    {
        DBconnection db = new DBconnection(false);

        public LoginForm()
        {
            InitializeComponent();
            // UI Enhancements
            txtUsername.TextChanged += (s, e) =>
            {
                txtUsername.IconRight = string.IsNullOrEmpty(txtUsername.Text)
                    ? Properties.Resources.help_icon
                    : null;
            };

            txtPassword.TextChanged += (s, e) =>
            {
                txtPassword.IconRight = string.IsNullOrEmpty(txtPassword.Text)
                    ? Properties.Resources.help_icon
                    : null;
            };

            var shadowForm = new Guna2ShadowForm { TargetForm = this };
            var elipse = new Guna2Elipse { TargetControl = this, BorderRadius = 20 };

            txtUsername.ShadowDecoration.Enabled = true;
            txtUsername.ShadowDecoration.Color = Color.Gray;
            txtUsername.ShadowDecoration.BorderRadius = txtUsername.BorderRadius;
            txtUsername.ShadowDecoration.Shadow = new Padding(5);

            txtPassword.ShadowDecoration.Enabled = true;
            txtPassword.ShadowDecoration.Color = Color.Gray;
            txtPassword.ShadowDecoration.BorderRadius = txtPassword.BorderRadius;
            txtPassword.ShadowDecoration.Shadow = new Padding(5);

            // Event bindings


        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Text;
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblMessage.Visible = true;
                lblMessage.ForeColor = Color.Red;
                lblMessage.Text = "Enter user name and password";
                return;
            }

            string passHash = pass;

            var auth = new AuthService(false); // false => local DB, true => online
            string msg;
            var u = auth.Login(user, pass, out msg);
            if (u == null)
            {
                lblMessage.ForeColor = Color.Red;
                lblMessage.Text = msg ?? "Invalid username or password";
                return;
            }

            lblMessage.ForeColor = Color.Green;
            lblMessage.Text = "Login successful";

            // open form based on role (تعديل أسماء الفورمز حسب مشروعك)
            // افتراض: Roles inserted order => Admin=1, Dean=2, Lecturer=3, Student=4
            switch (u.RoleID)
            {
                case 1: // Admin
                    var admin = new FormStudentsAffairs();
                    admin.Show();
                    this.Hide();
                    break;

                case 2: // Dean
                    var dean = new Dean();
                    dean.Show();
                    this.Hide();
                    break;
                case 3: // Lecturer
                    var lect = new LectuersForm();
                    lect.Show();
                    this.Hide();
                    break;


                default:
                    MessageBox.Show("Role not assigned. Contact admin.");
                    break;
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            txtUsername.Focus();
            txtPassword.UseSystemPasswordChar = true;
            lblMessage.Visible = false;
        }




        private void LinkReset_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                this.Hide();
                ResetCodeForm reset = new ResetCodeForm();
                reset.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في فتح نموذج إعادة التعيين: " + ex.Message);
            }
            finally
            {
                this.Show();
            }

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

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }



    }


}


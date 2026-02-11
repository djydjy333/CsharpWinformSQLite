using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;

namespace TaskManager
{
    public partial class LoginForm : Form
    {
        public string LoggedInUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
            InitializeDatabase();
            LoadRememberedUser();
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var conn = new SQLiteConnection(Program.ConnectionString))
                {
                    conn.Open();
                    string sql = @"
                        CREATE TABLE IF NOT EXISTS users (
                            id INTEGER PRIMARY KEY AUTOINCREMENT,
                            username TEXT NOT NULL UNIQUE,
                            password_hash TEXT NOT NULL,
                            created_at TEXT NOT NULL
                        );
                        CREATE TABLE IF NOT EXISTS settings (
                            key TEXT PRIMARY KEY,
                            value TEXT
                        );";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库初始化失败：" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRememberedUser()
        {
            try
            {
                using (var conn = new SQLiteConnection(Program.ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT value FROM settings WHERE key = 'remembered_user'";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            txtUsername.Text = result.ToString();
                            chkRemember.Checked = true;
                            txtPassword.Focus();
                        }
                    }
                }
            }
            catch { }
        }

        private void SaveRememberedUser(string username)
        {
            try
            {
                using (var conn = new SQLiteConnection(Program.ConnectionString))
                {
                    conn.Open();
                    string sql = chkRemember.Checked
                        ? "INSERT OR REPLACE INTO settings (key, value) VALUES ('remembered_user', @username)"
                        : "DELETE FROM settings WHERE key = 'remembered_user'";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "TaskManagerSalt"));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    sb.Append(bytes[i].ToString("x2"));
                return sb.ToString();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "请输入用户名和密码";
                return;
            }

            try
            {
                using (var conn = new SQLiteConnection(Program.ConnectionString))
                {
                    conn.Open();
                    string sql = "SELECT password_hash FROM users WHERE username = @username";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        object result = cmd.ExecuteScalar();

                        if (result == null)
                        {
                            lblStatus.ForeColor = Color.Red;
                            lblStatus.Text = "用户名不存在";
                            return;
                        }

                        string storedHash = result.ToString();
                        string inputHash = HashPassword(password);

                        if (storedHash == inputHash)
                        {
                            LoggedInUser = username;
                            SaveRememberedUser(username);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            lblStatus.ForeColor = Color.Red;
                            lblStatus.Text = "密码错误";
                            txtPassword.SelectAll();
                            txtPassword.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "登录失败：" + ex.Message;
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "请输入用户名和密码";
                return;
            }

            if (username.Length < 3)
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "用户名至少3个字符";
                return;
            }

            if (password.Length < 4)
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "密码至少4个字符";
                return;
            }

            try
            {
                using (var conn = new SQLiteConnection(Program.ConnectionString))
                {
                    conn.Open();

                    string checkSql = "SELECT COUNT(*) FROM users WHERE username = @username";
                    using (var cmd = new SQLiteCommand(checkSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        long count = (long)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            lblStatus.ForeColor = Color.Red;
                            lblStatus.Text = "用户名已存在";
                            return;
                        }
                    }

                    string insertSql = @"INSERT INTO users (username, password_hash, created_at)
                                         VALUES (@username, @hash, @created)";
                    using (var cmd = new SQLiteCommand(insertSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@hash", HashPassword(password));
                        cmd.Parameters.AddWithValue("@created", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        cmd.ExecuteNonQuery();
                    }

                    lblStatus.ForeColor = Color.Green;
                    lblStatus.Text = "注册成功！请点击登录";
                }
            }
            catch (Exception ex)
            {
                lblStatus.ForeColor = Color.Red;
                lblStatus.Text = "注册失败：" + ex.Message;
            }
        }

        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                btnLogin_Click(sender, e);
                e.Handled = true;
            }
        }
    }
}

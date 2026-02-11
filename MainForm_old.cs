using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;

// ============================================================
// 登录窗体
// ============================================================
public class LoginForm : Form
{
    private Label lblTitle;
    private Label lblUsername;
    private Label lblPassword;
    private TextBox txtUsername;
    private TextBox txtPassword;
    private Button btnLogin;
    private Button btnRegister;
    private CheckBox chkRemember;
    private Label lblStatus;

    private string connectionString;
    public string LoggedInUser { get; private set; }

    public LoginForm(string connStr)
    {
        connectionString = connStr;
        InitializeForm();
        InitializeDatabase();
        LoadRememberedUser();
    }

    private void InitializeForm()
    {
        this.Text = "登录 - 任务管理器";
        this.Size = new Size(350, 280);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Font = new Font("Microsoft YaHei", 9F);

        lblTitle = new Label
        {
            Text = "任务管理器",
            Font = new Font("Microsoft YaHei", 16F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 122, 183),
            Location = new Point(100, 15),
            AutoSize = true
        };

        lblUsername = new Label
        {
            Text = "用户名：",
            Location = new Point(30, 60),
            AutoSize = true
        };

        txtUsername = new TextBox
        {
            Location = new Point(100, 57),
            Size = new Size(200, 25)
        };

        lblPassword = new Label
        {
            Text = "密  码：",
            Location = new Point(30, 95),
            AutoSize = true
        };

        txtPassword = new TextBox
        {
            Location = new Point(100, 92),
            Size = new Size(200, 25),
            PasswordChar = '●'
        };
        txtPassword.KeyPress += (s, e) => { if (e.KeyChar == (char)Keys.Enter) BtnLogin_Click(s, e); };

        chkRemember = new CheckBox
        {
            Text = "记住用户名",
            Location = new Point(100, 125),
            AutoSize = true
        };

        btnLogin = new Button
        {
            Text = "登 录",
            Location = new Point(100, 155),
            Size = new Size(90, 32),
            BackColor = Color.FromArgb(51, 122, 183),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnLogin.Click += BtnLogin_Click;

        btnRegister = new Button
        {
            Text = "注 册",
            Location = new Point(210, 155),
            Size = new Size(90, 32)
        };
        btnRegister.Click += BtnRegister_Click;

        lblStatus = new Label
        {
            Text = "",
            ForeColor = Color.Red,
            Location = new Point(30, 200),
            Size = new Size(280, 20),
            TextAlign = ContentAlignment.MiddleCenter
        };

        this.Controls.Add(lblTitle);
        this.Controls.Add(lblUsername);
        this.Controls.Add(txtUsername);
        this.Controls.Add(lblPassword);
        this.Controls.Add(txtPassword);
        this.Controls.Add(chkRemember);
        this.Controls.Add(btnLogin);
        this.Controls.Add(btnRegister);
        this.Controls.Add(lblStatus);

        this.AcceptButton = btnLogin;
    }

    private void InitializeDatabase()
    {
        try
        {
            using (var conn = new SQLiteConnection(connectionString))
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
            using (var conn = new SQLiteConnection(connectionString))
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
            using (var conn = new SQLiteConnection(connectionString))
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

    private void BtnLogin_Click(object sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            lblStatus.Text = "请输入用户名和密码";
            return;
        }

        try
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT password_hash FROM users WHERE username = @username";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    object result = cmd.ExecuteScalar();

                    if (result == null)
                    {
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
                        lblStatus.Text = "密码错误";
                        txtPassword.SelectAll();
                        txtPassword.Focus();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = "登录失败：" + ex.Message;
        }
    }

    private void BtnRegister_Click(object sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            lblStatus.Text = "请输入用户名和密码";
            return;
        }

        if (username.Length < 3)
        {
            lblStatus.Text = "用户名至少3个字符";
            return;
        }

        if (password.Length < 4)
        {
            lblStatus.Text = "密码至少4个字符";
            return;
        }

        try
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // 检查用户名是否存在
                string checkSql = "SELECT COUNT(*) FROM users WHERE username = @username";
                using (var cmd = new SQLiteCommand(checkSql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    long count = (long)cmd.ExecuteScalar();
                    if (count > 0)
                    {
                        lblStatus.Text = "用户名已存在";
                        return;
                    }
                }

                // 创建用户
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
            lblStatus.Text = "注册失败：" + ex.Message;
        }
    }
}

// ============================================================
// 主窗体
// ============================================================
public class MainForm : Form
{
    private Label lblTask;
    private TextBox txtTask;
    private Label lblPriority;
    private ComboBox cmbPriority;
    private Button btnAdd;
    private Button btnDelete;
    private Button btnEdit;
    private Button btnComplete;
    private Button btnClear;
    private Button btnLogout;
    private ListView lvTasks;
    private Label lblStats;
    private Label lblUser;

    private bool isEditMode = false;
    private int editIndex = -1;
    private long editTaskId = -1;

    private string dbPath;
    private string connectionString;
    private string currentUser;
    private long currentUserId;

    public MainForm(string username, string connStr)
    {
        currentUser = username;
        connectionString = connStr;

        this.Text = "任务管理器 - " + username;
        this.Size = new Size(620, 520);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Microsoft YaHei", 9F);
        this.MinimumSize = new Size(620, 520);

        GetUserId();
        InitializeControls();
        InitializeTasksTable();
        LoadTasksFromDatabase();
    }

    private void GetUserId()
    {
        try
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT id FROM users WHERE username = @username";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@username", currentUser);
                    currentUserId = (long)cmd.ExecuteScalar();
                }
            }
        }
        catch { currentUserId = 0; }
    }

    private void InitializeTasksTable()
    {
        try
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();

                // 检查 tasks 表是否存在 user_id 列
                bool hasUserIdColumn = false;
                bool tableExists = false;

                using (var cmd = new SQLiteCommand("PRAGMA table_info(tasks)", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tableExists = true;
                        if (reader["name"].ToString() == "user_id")
                        {
                            hasUserIdColumn = true;
                            break;
                        }
                    }
                }

                // 如果表存在但没有 user_id 列，删除旧表
                if (tableExists && !hasUserIdColumn)
                {
                    using (var cmd = new SQLiteCommand("DROP TABLE tasks", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // 创建新表
                string sql = @"
                    CREATE TABLE IF NOT EXISTS tasks (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        user_id INTEGER NOT NULL,
                        content TEXT NOT NULL,
                        priority TEXT NOT NULL,
                        is_completed INTEGER DEFAULT 0,
                        created_at TEXT NOT NULL,
                        completed_at TEXT,
                        updated_at TEXT,
                        FOREIGN KEY (user_id) REFERENCES users(id)
                    )";
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

    private void LoadTasksFromDatabase()
    {
        try
        {
            lvTasks.Items.Clear();

            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT id, content, priority, is_completed, created_at, completed_at FROM tasks WHERE user_id = @userId ORDER BY id";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", currentUserId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        int index = 1;
                        while (reader.Read())
                        {
                            long id = reader.GetInt64(0);
                            string content = reader.GetString(1);
                            string priority = reader.GetString(2);
                            bool isCompleted = reader.GetInt32(3) == 1;
                            string createdAt = reader.GetString(4);
                            string completedAt = reader.IsDBNull(5) ? "" : reader.GetString(5);

                            ListViewItem item = new ListViewItem(index.ToString());
                            item.SubItems.Add(isCompleted ? "✓" : "○");
                            item.SubItems.Add(content);
                            item.SubItems.Add(priority);
                            item.SubItems.Add(createdAt);
                            item.SubItems.Add(completedAt);
                            item.Tag = id;

                            if (isCompleted)
                            {
                                item.BackColor = Color.FromArgb(220, 220, 220);
                                item.ForeColor = Color.Gray;
                                item.Font = new Font(item.Font, FontStyle.Strikeout);
                            }
                            else
                            {
                                SetItemColor(item, priority);
                            }

                            lvTasks.Items.Add(item);
                            index++;
                        }
                    }
                }
            }
            UpdateStats();
        }
        catch (Exception ex)
        {
            MessageBox.Show("加载数据失败：" + ex.Message, "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InitializeControls()
    {
        // 用户信息和登出按钮
        lblUser = new Label
        {
            Text = "当前用户: " + currentUser,
            Location = new Point(420, 15),
            AutoSize = true,
            ForeColor = Color.FromArgb(51, 122, 183)
        };

        btnLogout = new Button
        {
            Text = "退出登录",
            Location = new Point(520, 10),
            Size = new Size(70, 25),
            ForeColor = Color.Gray
        };
        btnLogout.Click += BtnLogout_Click;

        lblTask = new Label
        {
            Text = "任务内容：",
            Location = new Point(20, 20),
            AutoSize = true
        };

        txtTask = new TextBox
        {
            Location = new Point(20, 45),
            Size = new Size(300, 25)
        };
        txtTask.KeyPress += TxtTask_KeyPress;

        lblPriority = new Label
        {
            Text = "优先级：",
            Location = new Point(330, 20),
            AutoSize = true
        };

        cmbPriority = new ComboBox
        {
            Location = new Point(330, 45),
            Size = new Size(80, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbPriority.Items.AddRange(new string[] { "高", "中", "低" });
        cmbPriority.SelectedIndex = 1;

        btnAdd = new Button
        {
            Text = "添加任务",
            Location = new Point(420, 43),
            Size = new Size(80, 28)
        };
        btnAdd.Click += BtnAdd_Click;

        lvTasks = new ListView
        {
            Location = new Point(20, 85),
            Size = new Size(570, 300),
            View = View.Details,
            FullRowSelect = true,
            GridLines = true,
            MultiSelect = false
        };
        lvTasks.Columns.Add("序号", 45, HorizontalAlignment.Center);
        lvTasks.Columns.Add("状态", 55, HorizontalAlignment.Center);
        lvTasks.Columns.Add("任务内容", 250, HorizontalAlignment.Left);
        lvTasks.Columns.Add("优先级", 60, HorizontalAlignment.Center);
        lvTasks.Columns.Add("创建时间", 95, HorizontalAlignment.Center);
        lvTasks.Columns.Add("完成时间", 95, HorizontalAlignment.Center);
        lvTasks.DoubleClick += LvTasks_DoubleClick;
        lvTasks.SelectedIndexChanged += LvTasks_SelectedIndexChanged;

        btnComplete = new Button
        {
            Text = "标记完成",
            Location = new Point(20, 400),
            Size = new Size(90, 30),
            Enabled = false
        };
        btnComplete.Click += BtnComplete_Click;

        btnEdit = new Button
        {
            Text = "修改选中",
            Location = new Point(120, 400),
            Size = new Size(90, 30),
            Enabled = false
        };
        btnEdit.Click += BtnEdit_Click;

        btnDelete = new Button
        {
            Text = "删除选中",
            Location = new Point(220, 400),
            Size = new Size(90, 30),
            Enabled = false
        };
        btnDelete.Click += BtnDelete_Click;

        btnClear = new Button
        {
            Text = "清空全部",
            Location = new Point(320, 400),
            Size = new Size(90, 30)
        };
        btnClear.Click += BtnClear_Click;

        lblStats = new Label
        {
            Text = "总计: 0 | 进行中: 0 | 已完成: 0",
            Location = new Point(20, 445),
            AutoSize = true,
            ForeColor = Color.DarkBlue
        };

        this.Controls.Add(lblUser);
        this.Controls.Add(btnLogout);
        this.Controls.Add(lblTask);
        this.Controls.Add(txtTask);
        this.Controls.Add(lblPriority);
        this.Controls.Add(cmbPriority);
        this.Controls.Add(btnAdd);
        this.Controls.Add(lvTasks);
        this.Controls.Add(btnComplete);
        this.Controls.Add(btnEdit);
        this.Controls.Add(btnDelete);
        this.Controls.Add(btnClear);
        this.Controls.Add(lblStats);
    }

    private void BtnLogout_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.Retry; // 用 Retry 表示需要重新登录
        this.Close();
    }

    private void LvTasks_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool hasSelection = lvTasks.SelectedItems.Count > 0;
        btnEdit.Enabled = hasSelection;
        btnDelete.Enabled = hasSelection;
        btnComplete.Enabled = hasSelection;

        if (hasSelection)
        {
            ListViewItem item = lvTasks.SelectedItems[0];
            bool isCompleted = item.SubItems[1].Text == "✓";
            btnComplete.Text = isCompleted ? "取消完成" : "标记完成";
        }
    }

    private void BtnComplete_Click(object sender, EventArgs e)
    {
        if (lvTasks.SelectedItems.Count == 0) return;

        ListViewItem item = lvTasks.SelectedItems[0];
        bool isCompleted = item.SubItems[1].Text == "✓";

        if (isCompleted)
            MarkAsIncomplete(item);
        else
            MarkAsComplete(item);

        btnComplete.Text = isCompleted ? "标记完成" : "取消完成";
        UpdateStats();
    }

    private void MarkAsComplete(ListViewItem item)
    {
        long taskId = (long)item.Tag;
        string completedAt = DateTime.Now.ToString("MM-dd HH:mm");

        try
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE tasks SET is_completed = 1, completed_at = @completedAt WHERE id = @id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@completedAt", completedAt);
                    cmd.Parameters.AddWithValue("@id", taskId);
                    cmd.ExecuteNonQuery();
                }
            }

            item.SubItems[1].Text = "✓";
            item.SubItems[5].Text = completedAt;
            item.BackColor = Color.FromArgb(220, 220, 220);
            item.ForeColor = Color.Gray;
            item.Font = new Font(item.Font, FontStyle.Strikeout);
        }
        catch (Exception ex)
        {
            MessageBox.Show("更新失败：" + ex.Message, "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void MarkAsIncomplete(ListViewItem item)
    {
        long taskId = (long)item.Tag;

        try
        {
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = "UPDATE tasks SET is_completed = 0, completed_at = NULL WHERE id = @id";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", taskId);
                    cmd.ExecuteNonQuery();
                }
            }

            item.SubItems[1].Text = "○";
            item.SubItems[5].Text = "";
            item.ForeColor = Color.Black;
            item.Font = new Font(item.Font, FontStyle.Regular);
            SetItemColor(item, item.SubItems[3].Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show("更新失败：" + ex.Message, "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnAdd_Click(object sender, EventArgs e)
    {
        if (isEditMode)
            UpdateTask();
        else
            AddTask();
    }

    private void AddTask()
    {
        string taskContent = txtTask.Text.Trim();
        if (string.IsNullOrEmpty(taskContent))
        {
            MessageBox.Show("请输入任务内容！", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtTask.Focus();
            return;
        }

        string priority = cmbPriority.SelectedItem.ToString();
        string createdAt = DateTime.Now.ToString("MM-dd HH:mm");

        try
        {
            long newId;
            using (var conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO tasks (user_id, content, priority, is_completed, created_at)
                               VALUES (@userId, @content, @priority, 0, @createdAt);
                               SELECT last_insert_rowid();";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", currentUserId);
                    cmd.Parameters.AddWithValue("@content", taskContent);
                    cmd.Parameters.AddWithValue("@priority", priority);
                    cmd.Parameters.AddWithValue("@createdAt", createdAt);
                    newId = (long)cmd.ExecuteScalar();
                }
            }

            int index = lvTasks.Items.Count + 1;
            ListViewItem item = new ListViewItem(index.ToString());
            item.SubItems.Add("○");
            item.SubItems.Add(taskContent);
            item.SubItems.Add(priority);
            item.SubItems.Add(createdAt);
            item.SubItems.Add("");
            item.Tag = newId;

            SetItemColor(item, priority);
            lvTasks.Items.Add(item);

            txtTask.Clear();
            txtTask.Focus();
            UpdateStats();
        }
        catch (Exception ex)
        {
            MessageBox.Show("添加失败：" + ex.Message, "错误",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SetItemColor(ListViewItem item, string priority)
    {
        switch (priority)
        {
            case "高":
                item.BackColor = Color.FromArgb(255, 200, 200);
                break;
            case "中":
                item.BackColor = Color.FromArgb(255, 255, 200);
                break;
            case "低":
                item.BackColor = Color.FromArgb(200, 255, 200);
                break;
        }
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (lvTasks.SelectedItems.Count == 0) return;

        DialogResult result = MessageBox.Show("确定要删除选中的任务吗？", "确认删除",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            ListViewItem item = lvTasks.SelectedItems[0];
            long taskId = (long)item.Tag;

            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM tasks WHERE id = @id";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", taskId);
                        cmd.ExecuteNonQuery();
                    }
                }

                lvTasks.Items.Remove(item);
                RefreshIndex();
                ExitEditMode();
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show("删除失败：" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void BtnEdit_Click(object sender, EventArgs e)
    {
        if (lvTasks.SelectedItems.Count > 0)
            EnterEditMode();
    }

    private void LvTasks_DoubleClick(object sender, EventArgs e)
    {
        if (lvTasks.SelectedItems.Count > 0)
            EnterEditMode();
    }

    private void EnterEditMode()
    {
        ListViewItem item = lvTasks.SelectedItems[0];
        editIndex = item.Index;
        editTaskId = (long)item.Tag;

        txtTask.Text = item.SubItems[2].Text;
        cmbPriority.SelectedItem = item.SubItems[3].Text;

        isEditMode = true;
        btnAdd.Text = "保存修改";
        btnAdd.BackColor = Color.FromArgb(100, 200, 100);
        txtTask.BackColor = Color.FromArgb(255, 255, 220);
        txtTask.Focus();
        txtTask.SelectAll();
    }

    private void UpdateTask()
    {
        string taskContent = txtTask.Text.Trim();
        if (string.IsNullOrEmpty(taskContent))
        {
            MessageBox.Show("任务内容不能为空！", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtTask.Focus();
            return;
        }

        if (editIndex >= 0 && editIndex < lvTasks.Items.Count)
        {
            ListViewItem item = lvTasks.Items[editIndex];
            string priority = cmbPriority.SelectedItem.ToString();
            bool isCompleted = item.SubItems[1].Text == "✓";
            string updatedAt = DateTime.Now.ToString("MM-dd HH:mm");

            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE tasks SET content = @content, priority = @priority, updated_at = @updatedAt WHERE id = @id";
                    using (var cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@content", taskContent);
                        cmd.Parameters.AddWithValue("@priority", priority);
                        cmd.Parameters.AddWithValue("@updatedAt", updatedAt);
                        cmd.Parameters.AddWithValue("@id", editTaskId);
                        cmd.ExecuteNonQuery();
                    }
                }

                item.SubItems[2].Text = taskContent;
                item.SubItems[3].Text = priority;
                item.SubItems[4].Text = updatedAt + "*";

                if (!isCompleted)
                    SetItemColor(item, priority);
            }
            catch (Exception ex)
            {
                MessageBox.Show("更新失败：" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        ExitEditMode();
    }

    private void ExitEditMode()
    {
        isEditMode = false;
        editIndex = -1;
        editTaskId = -1;
        btnAdd.Text = "添加任务";
        btnAdd.BackColor = SystemColors.Control;
        txtTask.BackColor = SystemColors.Window;
        txtTask.Clear();
        txtTask.Focus();
    }

    private void BtnClear_Click(object sender, EventArgs e)
    {
        if (lvTasks.Items.Count == 0)
        {
            MessageBox.Show("列表已经是空的！", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult result = MessageBox.Show("确定要清空所有任务吗？", "确认清空",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            try
            {
                using (var conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand("DELETE FROM tasks WHERE user_id = @userId", conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", currentUserId);
                        cmd.ExecuteNonQuery();
                    }
                }

                lvTasks.Items.Clear();
                ExitEditMode();
                UpdateStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show("清空失败：" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void RefreshIndex()
    {
        for (int i = 0; i < lvTasks.Items.Count; i++)
            lvTasks.Items[i].Text = (i + 1).ToString();
    }

    private void UpdateStats()
    {
        int total = lvTasks.Items.Count;
        int completed = 0;
        for (int i = 0; i < lvTasks.Items.Count; i++)
        {
            if (lvTasks.Items[i].SubItems[1].Text == "✓")
                completed++;
        }
        lblStats.Text = string.Format("总计: {0} | 进行中: {1} | 已完成: {2}",
            total, total - completed, completed);
    }

    private void TxtTask_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == (char)Keys.Enter)
        {
            if (isEditMode) UpdateTask();
            else AddTask();
            e.Handled = true;
        }
        else if (e.KeyChar == (char)Keys.Escape && isEditMode)
        {
            ExitEditMode();
            e.Handled = true;
        }
    }

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        string dbPath = Path.Combine(Application.StartupPath, "tasks.db");
        string connectionString = "Data Source=" + dbPath + ";Version=3;";

        while (true)
        {
            // 显示登录窗口
            LoginForm loginForm = new LoginForm(connectionString);
            DialogResult loginResult = loginForm.ShowDialog();

            if (loginResult != DialogResult.OK)
            {
                // 用户关闭了登录窗口
                break;
            }

            // 登录成功，显示主窗口
            MainForm mainForm = new MainForm(loginForm.LoggedInUser, connectionString);
            DialogResult mainResult = mainForm.ShowDialog();

            if (mainResult != DialogResult.Retry)
            {
                // 用户关闭了主窗口（不是退出登录）
                break;
            }
            // 如果是 Retry，则循环回到登录界面
        }
    }
}

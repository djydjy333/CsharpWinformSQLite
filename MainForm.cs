using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;

namespace TaskManager
{
    public partial class MainForm : Form
    {
        private bool isEditMode = false;
        private int editIndex = -1;
        private long editTaskId = -1;
        private string currentUser;
        private long currentUserId;

        public MainForm(string username)
        {
            InitializeComponent();

            currentUser = username;
            this.Text = "任务管理器 - " + username;
            lblUser.Text = "当前用户: " + username;
            cmbPriority.SelectedIndex = 1;

            GetUserId();
            InitializeTasksTable();
            LoadTasksFromDatabase();
        }

        private void GetUserId()
        {
            try
            {
                using (var conn = new SQLiteConnection(Program.ConnectionString))
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
                using (var conn = new SQLiteConnection(Program.ConnectionString))
                {
                    conn.Open();

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

                    if (tableExists && !hasUserIdColumn)
                    {
                        using (var cmd = new SQLiteCommand("DROP TABLE tasks", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

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

                using (var conn = new SQLiteConnection(Program.ConnectionString))
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

        private void RefreshIndex()
        {
            for (int i = 0; i < lvTasks.Items.Count; i++)
                lvTasks.Items[i].Text = (i + 1).ToString();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry;
            this.Close();
        }

        private void lvTasks_SelectedIndexChanged(object sender, EventArgs e)
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

        private void lvTasks_DoubleClick(object sender, EventArgs e)
        {
            if (lvTasks.SelectedItems.Count > 0)
                EnterEditMode();
        }

        private void btnAdd_Click(object sender, EventArgs e)
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
                using (var conn = new SQLiteConnection(Program.ConnectionString))
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

        private void btnEdit_Click(object sender, EventArgs e)
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
                    using (var conn = new SQLiteConnection(Program.ConnectionString))
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

        private void btnDelete_Click(object sender, EventArgs e)
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
                    using (var conn = new SQLiteConnection(Program.ConnectionString))
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

        private void btnComplete_Click(object sender, EventArgs e)
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
                using (var conn = new SQLiteConnection(Program.ConnectionString))
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
                using (var conn = new SQLiteConnection(Program.ConnectionString))
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

        private void btnClear_Click(object sender, EventArgs e)
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
                    using (var conn = new SQLiteConnection(Program.ConnectionString))
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

        private void txtTask_KeyPress(object sender, KeyPressEventArgs e)
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
    }
}

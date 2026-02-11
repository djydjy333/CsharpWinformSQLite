namespace TaskManager
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblUser = new System.Windows.Forms.Label();
            this.btnLogout = new System.Windows.Forms.Button();
            this.lblTask = new System.Windows.Forms.Label();
            this.txtTask = new System.Windows.Forms.TextBox();
            this.lblPriority = new System.Windows.Forms.Label();
            this.cmbPriority = new System.Windows.Forms.ComboBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lvTasks = new System.Windows.Forms.ListView();
            this.colIndex = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.colContent = new System.Windows.Forms.ColumnHeader();
            this.colPriority = new System.Windows.Forms.ColumnHeader();
            this.colCreatedAt = new System.Windows.Forms.ColumnHeader();
            this.colCompletedAt = new System.Windows.Forms.ColumnHeader();
            this.btnComplete = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.lblStats = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // lblUser
            //
            this.lblUser.AutoSize = true;
            this.lblUser.ForeColor = System.Drawing.Color.FromArgb(51, 122, 183);
            this.lblUser.Location = new System.Drawing.Point(400, 15);
            this.lblUser.Name = "lblUser";
            this.lblUser.Size = new System.Drawing.Size(80, 17);
            this.lblUser.TabIndex = 0;
            this.lblUser.Text = "当前用户: ";
            //
            // btnLogout
            //
            this.btnLogout.ForeColor = System.Drawing.Color.Gray;
            this.btnLogout.Location = new System.Drawing.Point(520, 10);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(70, 25);
            this.btnLogout.TabIndex = 1;
            this.btnLogout.Text = "退出登录";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            //
            // lblTask
            //
            this.lblTask.AutoSize = true;
            this.lblTask.Location = new System.Drawing.Point(20, 20);
            this.lblTask.Name = "lblTask";
            this.lblTask.Size = new System.Drawing.Size(68, 17);
            this.lblTask.TabIndex = 2;
            this.lblTask.Text = "任务内容：";
            //
            // txtTask
            //
            this.txtTask.Location = new System.Drawing.Point(20, 45);
            this.txtTask.Name = "txtTask";
            this.txtTask.Size = new System.Drawing.Size(300, 23);
            this.txtTask.TabIndex = 3;
            this.txtTask.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtTask_KeyPress);
            //
            // lblPriority
            //
            this.lblPriority.AutoSize = true;
            this.lblPriority.Location = new System.Drawing.Point(330, 20);
            this.lblPriority.Name = "lblPriority";
            this.lblPriority.Size = new System.Drawing.Size(56, 17);
            this.lblPriority.TabIndex = 4;
            this.lblPriority.Text = "优先级：";
            //
            // cmbPriority
            //
            this.cmbPriority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPriority.FormattingEnabled = true;
            this.cmbPriority.Items.AddRange(new object[] { "高", "中", "低" });
            this.cmbPriority.Location = new System.Drawing.Point(330, 45);
            this.cmbPriority.Name = "cmbPriority";
            this.cmbPriority.Size = new System.Drawing.Size(80, 25);
            this.cmbPriority.TabIndex = 5;
            //
            // btnAdd
            //
            this.btnAdd.Location = new System.Drawing.Point(420, 43);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(80, 28);
            this.btnAdd.TabIndex = 6;
            this.btnAdd.Text = "添加任务";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            //
            // lvTasks
            //
            this.lvTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.colIndex,
                this.colStatus,
                this.colContent,
                this.colPriority,
                this.colCreatedAt,
                this.colCompletedAt });
            this.lvTasks.FullRowSelect = true;
            this.lvTasks.GridLines = true;
            this.lvTasks.HideSelection = false;
            this.lvTasks.Location = new System.Drawing.Point(20, 85);
            this.lvTasks.MultiSelect = false;
            this.lvTasks.Name = "lvTasks";
            this.lvTasks.Size = new System.Drawing.Size(570, 300);
            this.lvTasks.TabIndex = 7;
            this.lvTasks.UseCompatibleStateImageBehavior = false;
            this.lvTasks.View = System.Windows.Forms.View.Details;
            this.lvTasks.SelectedIndexChanged += new System.EventHandler(this.lvTasks_SelectedIndexChanged);
            this.lvTasks.DoubleClick += new System.EventHandler(this.lvTasks_DoubleClick);
            //
            // colIndex
            //
            this.colIndex.Text = "序号";
            this.colIndex.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colIndex.Width = 45;
            //
            // colStatus
            //
            this.colStatus.Text = "状态";
            this.colStatus.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colStatus.Width = 55;
            //
            // colContent
            //
            this.colContent.Text = "任务内容";
            this.colContent.Width = 250;
            //
            // colPriority
            //
            this.colPriority.Text = "优先级";
            this.colPriority.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colPriority.Width = 60;
            //
            // colCreatedAt
            //
            this.colCreatedAt.Text = "创建时间";
            this.colCreatedAt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colCreatedAt.Width = 95;
            //
            // colCompletedAt
            //
            this.colCompletedAt.Text = "完成时间";
            this.colCompletedAt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.colCompletedAt.Width = 95;
            //
            // btnComplete
            //
            this.btnComplete.Enabled = false;
            this.btnComplete.Location = new System.Drawing.Point(20, 400);
            this.btnComplete.Name = "btnComplete";
            this.btnComplete.Size = new System.Drawing.Size(90, 30);
            this.btnComplete.TabIndex = 8;
            this.btnComplete.Text = "标记完成";
            this.btnComplete.UseVisualStyleBackColor = true;
            this.btnComplete.Click += new System.EventHandler(this.btnComplete_Click);
            //
            // btnEdit
            //
            this.btnEdit.Enabled = false;
            this.btnEdit.Location = new System.Drawing.Point(120, 400);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(90, 30);
            this.btnEdit.TabIndex = 9;
            this.btnEdit.Text = "修改选中";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            //
            // btnDelete
            //
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(220, 400);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(90, 30);
            this.btnDelete.TabIndex = 10;
            this.btnDelete.Text = "删除选中";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            //
            // btnClear
            //
            this.btnClear.Location = new System.Drawing.Point(320, 400);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(90, 30);
            this.btnClear.TabIndex = 11;
            this.btnClear.Text = "清空全部";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            //
            // lblStats
            //
            this.lblStats.AutoSize = true;
            this.lblStats.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblStats.Location = new System.Drawing.Point(20, 445);
            this.lblStats.Name = "lblStats";
            this.lblStats.Size = new System.Drawing.Size(200, 17);
            this.lblStats.TabIndex = 12;
            this.lblStats.Text = "总计: 0 | 进行中: 0 | 已完成: 0";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 481);
            this.Controls.Add(this.lblStats);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnComplete);
            this.Controls.Add(this.lvTasks);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.cmbPriority);
            this.Controls.Add(this.lblPriority);
            this.Controls.Add(this.txtTask);
            this.Controls.Add(this.lblTask);
            this.Controls.Add(this.btnLogout);
            this.Controls.Add(this.lblUser);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 9F);
            this.MinimumSize = new System.Drawing.Size(620, 520);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "任务管理器";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblUser;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Label lblTask;
        private System.Windows.Forms.TextBox txtTask;
        private System.Windows.Forms.Label lblPriority;
        private System.Windows.Forms.ComboBox cmbPriority;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ListView lvTasks;
        private System.Windows.Forms.ColumnHeader colIndex;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.ColumnHeader colContent;
        private System.Windows.Forms.ColumnHeader colPriority;
        private System.Windows.Forms.ColumnHeader colCreatedAt;
        private System.Windows.Forms.ColumnHeader colCompletedAt;
        private System.Windows.Forms.Button btnComplete;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Label lblStats;
    }
}

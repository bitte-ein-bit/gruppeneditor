namespace Gruppeneditor
{
    partial class FormGuppeneditor
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.comboBoxGruppe = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxMember = new System.Windows.Forms.GroupBox();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxMember = new System.Windows.Forms.ComboBox();
            this.buttonRemove = new System.Windows.Forms.Button();
            this.listViewMember = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonSave = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.groupBoxMember.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBoxGruppe
            // 
            this.comboBoxGruppe.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.comboBoxGruppe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxGruppe.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxGruppe.FormattingEnabled = true;
            this.comboBoxGruppe.Items.AddRange(new object[] {
            "Gruppe 1",
            "Gruppe 2"});
            this.comboBoxGruppe.Location = new System.Drawing.Point(144, 6);
            this.comboBoxGruppe.Name = "comboBoxGruppe";
            this.comboBoxGruppe.Size = new System.Drawing.Size(464, 21);
            this.comboBoxGruppe.Sorted = true;
            this.comboBoxGruppe.TabIndex = 2;
            this.toolTip1.SetToolTip(this.comboBoxGruppe, "Wählen Sie die Gruppe, die Sie verwalten möchten.");
            this.comboBoxGruppe.SelectedIndexChanged += new System.EventHandler(this.comboBoxGruppe_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Zu bearbeitende Gruppe:";
            // 
            // groupBoxMember
            // 
            this.groupBoxMember.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxMember.Controls.Add(this.buttonAdd);
            this.groupBoxMember.Controls.Add(this.label2);
            this.groupBoxMember.Controls.Add(this.comboBoxMember);
            this.groupBoxMember.Controls.Add(this.buttonRemove);
            this.groupBoxMember.Controls.Add(this.listViewMember);
            this.groupBoxMember.Enabled = false;
            this.groupBoxMember.Location = new System.Drawing.Point(15, 33);
            this.groupBoxMember.Name = "groupBoxMember";
            this.groupBoxMember.Size = new System.Drawing.Size(593, 436);
            this.groupBoxMember.TabIndex = 4;
            this.groupBoxMember.TabStop = false;
            this.groupBoxMember.Text = "Gruppenmitglieder";
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.Location = new System.Drawing.Point(431, 17);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(75, 23);
            this.buttonAdd.TabIndex = 6;
            this.buttonAdd.Text = "hinzufügen";
            this.toolTip1.SetToolTip(this.buttonAdd, "Fügt den gesuchten Nutzer hinzu.");
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Suchen";
            // 
            // comboBoxMember
            // 
            this.comboBoxMember.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMember.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxMember.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.comboBoxMember.FormattingEnabled = true;
            this.comboBoxMember.Location = new System.Drawing.Point(56, 19);
            this.comboBoxMember.Name = "comboBoxMember";
            this.comboBoxMember.Size = new System.Drawing.Size(369, 21);
            this.comboBoxMember.TabIndex = 4;
            this.toolTip1.SetToolTip(this.comboBoxMember, "Suchen Sie hier nach neuen Mitglieder für die ausgewählte Gruppe.");
            this.comboBoxMember.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBoxMember_KeyDown);
            // 
            // buttonRemove
            // 
            this.buttonRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRemove.Location = new System.Drawing.Point(512, 17);
            this.buttonRemove.Name = "buttonRemove";
            this.buttonRemove.Size = new System.Drawing.Size(75, 23);
            this.buttonRemove.TabIndex = 3;
            this.buttonRemove.Text = "löschen";
            this.toolTip1.SetToolTip(this.buttonRemove, "Löscht die markierten Nutzer aus der Gruppe.");
            this.buttonRemove.UseVisualStyleBackColor = true;
            this.buttonRemove.Click += new System.EventHandler(this.buttonRemove_Click);
            // 
            // listViewMember
            // 
            this.listViewMember.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewMember.CheckBoxes = true;
            this.listViewMember.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewMember.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listViewMember.Location = new System.Drawing.Point(6, 46);
            this.listViewMember.Name = "listViewMember";
            this.listViewMember.ShowGroups = false;
            this.listViewMember.Size = new System.Drawing.Size(581, 373);
            this.listViewMember.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewMember.TabIndex = 2;
            this.listViewMember.UseCompatibleStateImageBehavior = false;
            this.listViewMember.View = System.Windows.Forms.View.Details;
            this.listViewMember.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listViewMember_ItemChecked);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 195;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Email";
            this.columnHeader2.Width = 338;
            // 
            // buttonSave
            // 
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Enabled = false;
            this.buttonSave.Location = new System.Drawing.Point(533, 480);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 5;
            this.buttonSave.Text = "Speichern";
            this.toolTip1.SetToolTip(this.buttonSave, "Speichert alle vorgenommen Änderungen an den Mitglieder dieser Gruppe.");
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FormGuppeneditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(620, 515);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.groupBoxMember);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxGruppe);
            this.Name = "FormGuppeneditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AD-Gruppeneditor";
            this.Shown += new System.EventHandler(this.FormGuppeneditor_Shown);
            this.groupBoxMember.ResumeLayout(false);
            this.groupBoxMember.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxGruppe;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBoxMember;
        private System.Windows.Forms.ListView listViewMember;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxMember;
        private System.Windows.Forms.Button buttonRemove;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonAdd;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer timer1;

    }
}


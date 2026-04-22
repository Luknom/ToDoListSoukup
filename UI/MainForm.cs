using TodoApp.Data;
using TodoApp.Models;
using TodoApp.UI.Controls;
using Microsoft.EntityFrameworkCore;

namespace TodoApp.UI;

public partial class MainForm : Form
{
    private ListBox _lstLists = null!;
    private FlowLayoutPanel _flpTasks = null!;
    private Label _lblStatus = null!;
    private TextBox _txtSearch = null!;
    
    private int? _selectedListId = null;

    public MainForm()
    {
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        Color darkBg = Color.FromArgb(18, 18, 18);
        Color sidebarBg = Color.FromArgb(30, 30, 30);
        Color textColor = Color.White;

        this.Text = "Todo Manager";
        this.Size = new Size(1000, 700);
        this.BackColor = darkBg;
        this.Font = new Font("Segoe UI", 10);
        this.StartPosition = FormStartPosition.CenterScreen;

        ToolStrip ts = new ToolStrip();
        ts.BackColor = sidebarBg;
        ts.ForeColor = textColor;
        ts.GripStyle = ToolStripGripStyle.Hidden;
        ts.RenderMode = ToolStripRenderMode.System;
        ts.Padding = new Padding(10, 5, 10, 5);
        ts.AutoSize = false;
        ts.Height = 50;

        ToolStripButton btnAddTask = new ToolStripButton("  + New Task  ", null, (s, e) => AddTask());
        btnAddTask.Font = new Font("Segoe UI Semibold", 9.5f);
        btnAddTask.ForeColor = Color.FromArgb(0, 150, 255);
        
        ToolStripButton btnAddList = new ToolStripButton("  Create List  ", null, (s, e) => AddList());
        btnAddList.ForeColor = textColor;

        ToolStripButton btnDeleteList = new ToolStripButton("  Delete List  ", null, (s, e) => DeleteList());
        btnDeleteList.ForeColor = Color.FromArgb(200, 80, 80);

        ToolStripButton btnManageTags = new ToolStripButton("  Tags  ", null, (s, e) => ManageTags());
        btnManageTags.ForeColor = textColor;

        ts.Items.Add(btnAddTask);
        ts.Items.Add(new ToolStripSeparator());
        ts.Items.Add(btnAddList);
        ts.Items.Add(btnDeleteList);
        ts.Items.Add(new ToolStripSeparator());
        ts.Items.Add(btnManageTags);

        ts.Items.Add(new ToolStripSeparator());
        ts.Items.Add(new ToolStripLabel("  🔍 "));
        
        var tstbSearch = new ToolStripTextBox("tstbSearch");
        tstbSearch.TextBox.BackColor = Color.FromArgb(45, 45, 45);
        tstbSearch.TextBox.ForeColor = textColor;
        tstbSearch.TextBox.BorderStyle = BorderStyle.FixedSingle;
        tstbSearch.TextBox.Width = 250;
        tstbSearch.TextBox.Font = new Font("Segoe UI", 10);
        tstbSearch.TextChanged += (s, e) => { _txtSearch.Text = tstbSearch.Text; LoadTasks(); };
        ts.Items.Add(tstbSearch);
        _txtSearch = tstbSearch.TextBox;

        this.Controls.Add(ts);

        SplitContainer split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel1,
            SplitterDistance = 180,
            BackColor = Color.FromArgb(40, 40, 40),
            Panel1MinSize = 120,
            BorderStyle = BorderStyle.None
        };
        split.Panel1.BackColor = sidebarBg;
        split.Panel2.BackColor = darkBg;

        Label lblCats = new Label
        {
            Text = "LISTS",
            Dock = DockStyle.Top,
            ForeColor = Color.FromArgb(120, 120, 120),
            Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            Padding = new Padding(15, 20, 0, 5),
            Height = 45
        };
        split.Panel1.Controls.Add(lblCats);

        _lstLists = new ListBox { 
            Dock = DockStyle.Fill, 
            BackColor = sidebarBg, 
            ForeColor = textColor, 
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10),
            ItemHeight = 40,
            DrawMode = DrawMode.OwnerDrawFixed
        };
        _lstLists.DrawItem += (s, e) => {
            if (e.Index < 0) return;
            e.DrawBackground();
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color backColor = isSelected ? Color.FromArgb(50, 50, 50) : sidebarBg;
            Color textCol = isSelected ? Color.FromArgb(0, 150, 255) : textColor;
            
            using (var b = new SolidBrush(backColor)) e.Graphics.FillRectangle(b, e.Bounds);
            TextRenderer.DrawText(e.Graphics, _lstLists.Items[e.Index].ToString(), e.Font, e.Bounds, textCol, TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.LeftAndRightPadding);
        };

        _lstLists.SelectedIndexChanged += (s, e) => {
            if (_lstLists.SelectedItem is TodoList list)
            {
                _selectedListId = list.Id;
                LoadTasks();
            }
        };
        split.Panel1.Controls.Add(_lstLists);
        _lstLists.BringToFront();

        _flpTasks = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(10),
            BackColor = darkBg
        };
        split.Panel2.Controls.Add(_flpTasks);

        _lblStatus = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            ForeColor = Color.FromArgb(100, 100, 100),
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0),
            BackColor = sidebarBg,
            Font = new Font("Segoe UI", 8.5f)
        };

        this.Controls.Add(split);
        this.Controls.Add(_lblStatus);
        
        ts.SendToBack(); 
        split.BringToFront();
    }

    private void LoadData()
    {
        using (var db = new AppDbContext())
        {
            db.Database.EnsureCreated();

            if (!db.TodoLists.Any())
            {
                db.TodoLists.Add(new TodoList { Name = "General" });
                db.SaveChanges();
            }

            var lists = db.TodoLists.OrderBy(l => l.Name).ToList();
            
            _lstLists.Items.Clear();
            _lstLists.Items.Add(new TodoList { Id = -1, Name = "--- All Tasks ---" });
            foreach (var l in lists) _lstLists.Items.Add(l);

            _lstLists.DisplayMember = "Name";
            _lstLists.SelectedIndex = 0;
            _selectedListId = -1;
        }
        LoadTasks();
    }

    private void LoadTasks()
    {
        _flpTasks.SuspendLayout();
        
        while (_flpTasks.Controls.Count > 0)
        {
            var c = _flpTasks.Controls[0];
            _flpTasks.Controls.RemoveAt(0);
            c.Dispose();
        }

        string searchText = _txtSearch.Text.Trim();

        try
        {
            using (var db = new AppDbContext())
            {
                var query = db.TodoItems
                    .Include(i => i.Tags)
                    .AsQueryable();

                if (_selectedListId != -1)
                {
                    query = query.Where(i => i.TodoListId == _selectedListId);
                }

                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    string pattern = $"%{searchText}%";
                    query = query.Where(i => EF.Functions.Like(i.Title, pattern) || 
                                             EF.Functions.Like(i.Description, pattern) ||
                                             i.Tags.Any(t => EF.Functions.Like(t.Name, pattern)));
                }

                var tasks = query.OrderBy(i => i.IsCompleted).ThenByDescending(i => i.CreatedAt).ToList();

                foreach (var task in tasks)
                {
                    var control = new TodoItemControl(task);
                    control.Width = _flpTasks.ClientSize.Width - 15;
                    control.StatusChanged += UpdateTaskStatus;
                    control.DeleteRequested += DeleteTask;
                    control.EditRequested += EditTask;
                    _flpTasks.Controls.Add(control);
                }

                _lblStatus.Text = $"Showing {tasks.Count} tasks";
            }
        }
        catch (Exception ex)
        {
            _lblStatus.Text = "Error loading tasks";
            MessageBox.Show($"Could not load tasks: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _flpTasks.ResumeLayout();
        }
    }

    private void UpdateTaskStatus(TodoItem item)
    {
        try
        {
            using (var db = new AppDbContext())
            {
                db.Entry(item).State = EntityState.Modified;
                db.SaveChanges();
            }
            _lblStatus.Text = "Task updated";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating task: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteTask(TodoItem item)
    {
        if (MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            using (var db = new AppDbContext())
            {
                db.TodoItems.Remove(item);
                db.SaveChanges();
            }
            LoadTasks();
        }
    }

    private void AddTask()
    {
        if (_selectedListId == null)
        {
            MessageBox.Show("Please select or create a list first.");
            return;
        }
        
        using (var dlg = new AddEditTaskForm(_selectedListId.Value == -1 ? 1 : _selectedListId.Value))
        {
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadTasks();
            }
        }
    }

    private void EditTask(TodoItem item)
    {
        using (var dlg = new AddEditTaskForm(item))
        {
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                LoadTasks();
            }
        }
    }

    private void AddList()
    {
        using (var dlg = new Form())
        {
            dlg.Text = "New List";
            dlg.Size = new Size(300, 150);
            dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
            dlg.StartPosition = FormStartPosition.CenterParent;

            Label lbl = new Label { Text = "List Name:", Location = new Point(20, 20), Width = 100 };
            TextBox txt = new TextBox { Location = new Point(20, 45), Width = 240 };
            Button btnOk = new Button { Text = "OK", Location = new Point(110, 80), DialogResult = DialogResult.OK };
            Button btnCancel = new Button { Text = "Cancel", Location = new Point(190, 80), DialogResult = DialogResult.Cancel };

            dlg.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
            dlg.AcceptButton = btnOk;

            if (dlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txt.Text))
            {
                using (var db = new AppDbContext())
                {
                    db.TodoLists.Add(new TodoList { Name = txt.Text });
                    db.SaveChanges();
                    LoadData();
                }
            }
        }
    }

    private void DeleteList()
    {
        if (_selectedListId == null || _selectedListId == -1)
        {
            MessageBox.Show("Please select a specific list to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show("Are you sure you want to delete this list and ALL its tasks?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (result == DialogResult.Yes)
        {
            try
            {
                using (var db = new AppDbContext())
                {
                    var list = db.TodoLists.Find(_selectedListId);
                    if (list != null)
                    {
                        db.TodoLists.Remove(list);
                        db.SaveChanges();
                    }
                }
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting list: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void ManageTags()
    {
        using (var dlg = new ManageTagsForm())
        {
            dlg.ShowDialog();
            LoadTasks();
        }
    }
}

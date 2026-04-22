using TodoApp.Data;
using TodoApp.Models;
using Microsoft.EntityFrameworkCore;

namespace TodoApp.UI;

public partial class AddEditTaskForm : Form
{
    private readonly int? _listId;
    private readonly TodoItem? _item;
    
    private TextBox _txtTitle = null!;
    private TextBox _txtDesc = null!;
    private ComboBox _cboPriority = null!;
    private DateTimePicker _dtpDue = null!;
    private CheckedListBox _clbTags = null!;

    public AddEditTaskForm(int listId)
    {
        _listId = listId;
        InitializeComponent();
        LoadTags();
    }

    public AddEditTaskForm(TodoItem item)
    {
        _item = item;
        InitializeComponent();
        LoadTags();
        PopulateFields();
    }

    private void InitializeComponent()
    {
        this.Text = _item == null ? "Add Task" : "Edit Task";
        this.Size = new Size(420, 520);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(35, 35, 35);
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 10);

        Color fieldBg = Color.FromArgb(50, 50, 50);
        Color textColor = Color.White;

        Label lblTitle = new Label { Text = "Title:", Location = new Point(20, 20), Width = 100, ForeColor = textColor };
        _txtTitle = new TextBox { 
            Location = new Point(120, 20), 
            Width = 250, 
            BackColor = fieldBg, 
            ForeColor = textColor, 
            BorderStyle = BorderStyle.FixedSingle,
            MaxLength = 200
        };

        Label lblDesc = new Label { Text = "Description:", Location = new Point(20, 60), Width = 100, ForeColor = textColor };
        _txtDesc = new TextBox { 
            Location = new Point(120, 60), 
            Width = 250, 
            Multiline = true, 
            Height = 80, 
            BackColor = fieldBg, 
            ForeColor = textColor, 
            BorderStyle = BorderStyle.FixedSingle,
            MaxLength = 2000
        };

        Label lblPriority = new Label { Text = "Priority:", Location = new Point(20, 160), Width = 100, ForeColor = textColor };
        _cboPriority = new ComboBox { Location = new Point(120, 160), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = fieldBg, ForeColor = textColor, FlatStyle = FlatStyle.Flat };
        _cboPriority.Items.AddRange(Enum.GetNames(typeof(TaskPriority)));
        _cboPriority.SelectedIndex = 1;

        Label lblDue = new Label { Text = "Due Date:", Location = new Point(20, 200), Width = 100, ForeColor = textColor };
        _dtpDue = new DateTimePicker { Location = new Point(120, 200), Width = 250, ShowCheckBox = true, Checked = false };

        Label lblTags = new Label { Text = "Tags:", Location = new Point(20, 240), Width = 100, ForeColor = textColor };
        _clbTags = new CheckedListBox { Location = new Point(120, 240), Width = 250, Height = 150, BackColor = fieldBg, ForeColor = textColor, BorderStyle = BorderStyle.None, CheckOnClick = true };

        Button btnSave = new Button { 
            Text = "Save", 
            Location = new Point(210, 420), 
            Width = 80, 
            Height = 35, 
            DialogResult = DialogResult.OK,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White
        };
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.Click += (s, e) => Save();
        
        Button btnCancel = new Button { 
            Text = "Cancel", 
            Location = new Point(300, 420), 
            Width = 80, 
            Height = 35, 
            DialogResult = DialogResult.Cancel,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(70, 70, 70),
            ForeColor = Color.White
        };
        btnCancel.FlatAppearance.BorderSize = 0;

        this.Controls.AddRange(new Control[] { lblTitle, _txtTitle, lblDesc, _txtDesc, lblPriority, _cboPriority, lblDue, _dtpDue, lblTags, _clbTags, btnSave, btnCancel });
    }

    private void LoadTags()
    {
        using (var db = new AppDbContext())
        {
            var tags = db.Tags.ToList();
            foreach (var tag in tags)
            {
                bool isChecked = _item != null && _item.Tags.Any(t => t.Id == tag.Id);
                _clbTags.Items.Add(tag, isChecked);
            }
        }
    }

    private void PopulateFields()
    {
        if (_item != null)
        {
            _txtTitle.Text = _item.Title;
            _txtDesc.Text = _item.Description;
            _cboPriority.SelectedItem = _item.Priority.ToString();
            if (_item.DueDate.HasValue)
            {
                _dtpDue.Value = _item.DueDate.Value;
                _dtpDue.Checked = true;
            }
        }
    }

    private void Save()
    {
        if (string.IsNullOrWhiteSpace(_txtTitle.Text))
        {
            MessageBox.Show("Title is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            this.DialogResult = DialogResult.None;
            return;
        }

        try
        {
            using (var db = new AppDbContext())
            {
                TodoItem itemToSave;
                if (_item == null)
                {
                    itemToSave = new TodoItem { TodoListId = _listId!.Value };
                    db.TodoItems.Add(itemToSave);
                }
                else
                {
                    var existing = db.TodoItems.Include(i => i.Tags).FirstOrDefault(i => i.Id == _item.Id);
                    if (existing == null) throw new Exception("Item not found in database.");
                    itemToSave = existing;
                }

                itemToSave.Title = _txtTitle.Text;
                itemToSave.Description = _txtDesc.Text;
                
                string? priorityStr = _cboPriority.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(priorityStr))
                {
                    itemToSave.Priority = (TaskPriority)Enum.Parse(typeof(TaskPriority), priorityStr);
                }
                
                itemToSave.DueDate = _dtpDue.Checked ? _dtpDue.Value : null;

                itemToSave.Tags.Clear();
                foreach (var checkedItem in _clbTags.CheckedItems)
                {
                    var tag = (Tag)checkedItem;
                    var dbTag = db.Tags.Find(tag.Id);
                    if (dbTag != null) itemToSave.Tags.Add(dbTag);
                }

                db.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving task: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.DialogResult = DialogResult.None;
        }
    }
}

using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.UI;

public partial class ManageTagsForm : Form
{
    private ListBox _lstTags = null!;
    private TextBox _txtName = null!;

    public ManageTagsForm()
    {
        InitializeComponent();
        LoadTags();
    }

    private void InitializeComponent()
    {
        this.Text = "Manage Tags";
        this.Size = new Size(320, 420);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(35, 35, 35);
        this.ForeColor = Color.White;
        this.Font = new Font("Segoe UI", 10);

        Color fieldBg = Color.FromArgb(50, 50, 50);
        Color textColor = Color.White;

        _lstTags = new ListBox { 
            Location = new Point(20, 20), 
            Width = 260, 
            Height = 200, 
            BackColor = fieldBg, 
            ForeColor = textColor, 
            BorderStyle = BorderStyle.None 
        };
        
        _txtName = new TextBox { 
            Location = new Point(20, 240), 
            Width = 180, 
            BackColor = fieldBg, 
            ForeColor = textColor, 
            BorderStyle = BorderStyle.FixedSingle 
        };
        
        Button btnAdd = new Button { 
            Text = "Add", 
            Location = new Point(210, 240), 
            Width = 70, 
            Height = 26,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White
        };
        btnAdd.FlatAppearance.BorderSize = 0;
        btnAdd.Click += (s, e) => AddTag();

        Button btnDelete = new Button { 
            Text = "Delete Selected", 
            Location = new Point(20, 280), 
            Width = 260, 
            Height = 35,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(100, 40, 40),
            ForeColor = Color.White
        };
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.Click += (s, e) => DeleteTag();

        Button btnClose = new Button { 
            Text = "Close", 
            Location = new Point(210, 330), 
            Width = 70, 
            Height = 30, 
            DialogResult = DialogResult.OK,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(70, 70, 70),
            ForeColor = Color.White
        };
        btnClose.FlatAppearance.BorderSize = 0;

        this.Controls.AddRange(new Control[] { _lstTags, _txtName, btnAdd, btnDelete, btnClose });
    }

    private void LoadTags()
    {
        using (var db = new AppDbContext())
        {
            _lstTags.DataSource = db.Tags.ToList();
            _lstTags.DisplayMember = "Name";
        }
    }

    private void AddTag()
    {
        string name = _txtName.Text.Trim();
        if (!string.IsNullOrWhiteSpace(name))
        {
            using (var db = new AppDbContext())
            {
                db.Tags.Add(new Tag { Name = name });
                db.SaveChanges();
                _txtName.Clear();
                LoadTags();
            }
        }
    }

    private void DeleteTag()
    {
        if (_lstTags.SelectedItem is Tag tag)
        {
            using (var db = new AppDbContext())
            {
                db.Tags.Remove(tag);
                db.SaveChanges();
                LoadTags();
            }
        }
    }
}

using TodoApp.Models;

namespace TodoApp.UI.Controls;

public partial class TodoItemControl : UserControl
{
    private readonly TodoItem _item;
    public event Action<TodoItem>? StatusChanged;
    public event Action<TodoItem>? EditRequested;
    public event Action<TodoItem>? DeleteRequested;

    private CheckBox _chkCompleted = null!;
    private Label _lblTitle = null!;
    private Label _lblDetails = null!;
    private Button _btnEdit = null!;
    private Button _btnDelete = null!;

    public TodoItemControl(TodoItem item)
    {
        _item = item;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Height = 85;
        this.MinimumSize = new Size(300, 85);
        this.Padding = new Padding(0);
        this.BackColor = Color.FromArgb(32, 32, 32);
        this.Margin = new Padding(0, 0, 0, 8);

        Panel pnlPriority = new Panel
        {
            Dock = DockStyle.Left,
            Width = 4,
            BackColor = GetPriorityColor()
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(10, 5, 10, 5)
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 35));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));

        _chkCompleted = new CheckBox
        {
            Checked = _item.IsCompleted,
            Anchor = AnchorStyles.None,
            CheckAlign = ContentAlignment.MiddleCenter,
            Cursor = Cursors.Hand,
            FlatStyle = FlatStyle.Flat,
            Size = new Size(25, 25)
        };
        _chkCompleted.FlatAppearance.BorderSize = 0;
        _chkCompleted.CheckedChanged += (s, e) => {
            _item.IsCompleted = _chkCompleted.Checked;
            UpdateStyle();
            StatusChanged?.Invoke(_item);
        };

        var textPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        _lblTitle = new Label
        {
            Text = _item.Title,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 10.5f),
            ForeColor = Color.White,
            Margin = new Padding(0, 3, 0, 0)
        };

        string tags = _item.Tags.Count > 0 ? $" • {string.Join(", ", _item.Tags.Select(t => t.Name))}" : "";
        string dueDate = _item.DueDate.HasValue ? $" • Due: {_item.DueDate.Value.ToShortDateString()}" : "";
        
        _lblDetails = new Label
        {
            Text = $"{_item.Priority}{dueDate}{tags}",
            AutoSize = true,
            Font = new Font("Segoe UI", 8.5f),
            ForeColor = Color.FromArgb(150, 150, 150),
            Margin = new Padding(0, 1, 0, 0)
        };

        textPanel.Controls.Add(_lblTitle);
        textPanel.Controls.Add(_lblDetails);

        _btnEdit = CreateActionButton("Edit", Color.FromArgb(60, 60, 60));
        _btnEdit.Click += (s, e) => EditRequested?.Invoke(_item);

        _btnDelete = CreateActionButton("Delete", Color.FromArgb(180, 45, 45));
        _btnDelete.Click += (s, e) => DeleteRequested?.Invoke(_item);

        table.Controls.Add(_chkCompleted, 0, 0);
        table.Controls.Add(textPanel, 1, 0);
        table.Controls.Add(_btnEdit, 2, 0);
        table.Controls.Add(_btnDelete, 3, 0);

        this.Controls.Add(table);
        this.Controls.Add(pnlPriority);
        
        UpdateStyle();
    }

    private Color GetPriorityColor()
    {
        return _item.Priority switch
        {
            TaskPriority.High => Color.FromArgb(232, 17, 35),
            TaskPriority.Medium => Color.FromArgb(255, 185, 0),
            _ => Color.FromArgb(16, 124, 16)
        };
    }

    private Button CreateActionButton(string text, Color backColor)
    {
        var btn = new Button
        {
            Text = text,
            Size = new Size(60, 28),
            Anchor = AnchorStyles.None,
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 8.5f),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private void UpdateStyle()
    {
        if (_item.IsCompleted)
        {
            _lblTitle.Font = new Font(_lblTitle.Font, FontStyle.Strikeout);
            _lblTitle.ForeColor = Color.FromArgb(100, 100, 100);
        }
        else
        {
            _lblTitle.Font = new Font(_lblTitle.Font, FontStyle.Regular);
            _lblTitle.ForeColor = Color.White;
        }
    }

    protected override void OnResize(EventArgs e) { base.OnResize(e); }
}

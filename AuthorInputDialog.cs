namespace SvnExporter;

/// <summary>
/// 用于批量输入 SVN 账号的对话框。
/// </summary>
public sealed class AuthorInputDialog : Form
{
    private readonly TextBox _authorTextBox;

    public AuthorInputDialog()
    {
        SuspendLayout();

        Text = "添加账号";
        Icon = AppIcon.Create() ?? Icon;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(680, 500);
        MinimumSize = new Size(600, 440);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;

        var rootLayout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            RowCount = 3
        };
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var promptLabel = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 12),
            Text = "请输入要添加的账号，每行一个账号："
        };

        _authorTextBox = new TextBox
        {
            AcceptsReturn = true,
            AcceptsTab = false,
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
        };

        var confirmButton = new Button
        {
            AutoSize = true,
            Margin = new Padding(8, 0, 0, 0),
            MinimumSize = new Size(110, 42),
            Text = "确定",
            UseVisualStyleBackColor = true
        };
        confirmButton.Click += ConfirmButton_Click;

        var cancelButton = new Button
        {
            AutoSize = true,
            DialogResult = DialogResult.Cancel,
            Margin = Padding.Empty,
            MinimumSize = new Size(110, 42),
            Text = "取消",
            UseVisualStyleBackColor = true
        };

        var buttonLayout = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Margin = new Padding(0, 14, 0, 0),
            WrapContents = false
        };
        buttonLayout.Controls.Add(cancelButton);
        buttonLayout.Controls.Add(confirmButton);

        AcceptButton = confirmButton;
        CancelButton = cancelButton;

        rootLayout.Controls.Add(promptLabel, 0, 0);
        rootLayout.Controls.Add(_authorTextBox, 0, 1);
        rootLayout.Controls.Add(buttonLayout, 0, 2);
        Controls.Add(rootLayout);

        ResumeLayout(performLayout: true);
    }

    /// <summary>
    /// 用户点击“确定”后得到的非空、不重复账号列表。
    /// </summary>
    public IReadOnlyList<string> Authors { get; private set; } = Array.Empty<string>();

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _authorTextBox.Focus();
    }

    private void ConfirmButton_Click(object? sender, EventArgs e)
    {
        string[] authors = _authorTextBox.Lines
            .Select(author => author.Trim())
            .Where(author => author.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (authors.Length == 0)
        {
            MessageBox.Show(
                this,
                "请至少输入一个账号。",
                "提示",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            _authorTextBox.Focus();
            return;
        }

        Authors = authors;
        DialogResult = DialogResult.OK;
        Close();
    }
}

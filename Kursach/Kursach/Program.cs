using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

public class CircleNumberGame : Form
{
    private const int GridSize = 4;
    private const int ButtonRadius = 30;
    private const int ButtonSpacing = 75;

    private List<RoundButton> numberButtons = new List<RoundButton>();
    private Label targetLabel;
    private Label timerLabel;
    private Label scoreLabel;
    private System.Windows.Forms.Timer gameTimer;

    private int timeLeft = 45;
    private int score = 0;
    private int targetSum;

    private List<Point> pathPoints = new List<Point>();
    private List<RoundButton> selectedButtons = new List<RoundButton>();
    private int currentSum = 0;
    private bool isDrawing = false;

    private Random random = new Random();
    private Font buttonFont = new Font("Arial", 14, FontStyle.Bold);

    public CircleNumberGame()
    {
        InitializeForm();
        InitializeControls();
        StartNewRound();
    }

    private void InitializeForm()
    {
        this.Text = "Числовая Головоломка - Соединяй Числа";
        this.ClientSize = new Size(450, 550);
        this.BackColor = Color.FromArgb(240, 248, 255); // AliceBlue
        this.DoubleBuffered = true;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
    }

    private void InitializeControls()
    {
        // Header panel
        Panel headerPanel = new Panel
        {
            BackColor = Color.FromArgb(70, 130, 180), // SteelBlue
            Size = new Size(this.ClientSize.Width, 80),
            Dock = DockStyle.Top
        };
        this.Controls.Add(headerPanel);

        // Score label
        scoreLabel = new Label
        {
            Text = $"Очки: {score}",
            Location = new Point(20, 20),
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };
        headerPanel.Controls.Add(scoreLabel);

        // Timer label
        timerLabel = new Label
        {
            Text = $"⏱ {timeLeft} сек",
            Location = new Point(350, 20),
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };
        headerPanel.Controls.Add(timerLabel);

        // Target label
        targetLabel = new Label
        {
            Text = "Найдите числа, сумма которых:",
            Location = new Point(20, 90),
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 130, 180),
            AutoSize = true
        };
        this.Controls.Add(targetLabel);

        // Create circular number buttons
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                RoundButton button = new RoundButton
                {
                    Size = new Size(ButtonRadius * 2, ButtonRadius * 2),
                    Location = new Point(40 + col * ButtonSpacing, 140 + row * ButtonSpacing),
                    Font = buttonFont,
                    BackColor = Color.FromArgb(100, 149, 237), // CornflowerBlue
                    ForeColor = Color.White,
                    Tag = random.Next(1, 10), // Random number 1-9
                    FlatStyle = FlatStyle.Flat
                };

                button.FlatAppearance.BorderSize = 0;
                button.MouseDown += NumberButton_MouseDown;
                button.MouseEnter += NumberButton_MouseEnter;

                numberButtons.Add(button);
                this.Controls.Add(button);
            }
        }

        // Game timer
        gameTimer = new System.Windows.Forms.Timer
        {
            Interval = 1000
        };
        gameTimer.Tick += GameTimer_Tick;
        gameTimer.Start();
    }

    private void StartNewRound()
    {
        // Generate new random numbers (1-9)
        foreach (RoundButton button in numberButtons)
        {
            button.Tag = random.Next(1, 10);
            button.Text = button.Tag.ToString();
            button.BackColor = Color.FromArgb(100, 149, 237);
            button.Enabled = true;
        }

        // Set random target sum between 10 and 30
        targetSum = random.Next(10, 31);
        targetLabel.Text = $"Найдите числа, сумма которых: {targetSum}";

        // Reset selection
        currentSum = 0;
        pathPoints.Clear();
        selectedButtons.Clear();
        isDrawing = false;

        this.Invalidate();
    }

    private void NumberButton_MouseDown(object sender, MouseEventArgs e)
    {
        if (!isDrawing)
        {
            // Start new connection
            isDrawing = true;
            pathPoints.Clear();
            selectedButtons.Clear();
            currentSum = 0;
            AddButtonToPath((RoundButton)sender);
        }
    }

    private void NumberButton_MouseEnter(object sender, EventArgs e)
    {
        if (isDrawing && sender is RoundButton button && !selectedButtons.Contains(button))
        {
            AddButtonToPath(button);

            // Check if we have a solution
            if (currentSum == targetSum)
            {
                ProcessCorrectAnswer();
            }
            else if (currentSum > targetSum)
            {
                ProcessWrongAnswer();
            }
        }
    }

    private void AddButtonToPath(RoundButton button)
    {
        button.BackColor = Color.FromArgb(70, 130, 180); // Darker blue
        selectedButtons.Add(button);
        currentSum += (int)button.Tag;

        // Add center of button to path
        pathPoints.Add(new Point(
            button.Left + button.Width / 2,
            button.Top + button.Height / 2));

        this.Invalidate();
    }

    private void ProcessCorrectAnswer()
    {
        isDrawing = false;
        score += targetSum;
        scoreLabel.Text = $"Очки: {score}";

        // Flash the selected buttons
        foreach (RoundButton button in selectedButtons)
        {
            button.BackColor = Color.LimeGreen;
        }

        this.Refresh();
        System.Threading.Thread.Sleep(300);

        StartNewRound();
    }

    private void ProcessWrongAnswer()
    {
        isDrawing = false;

        // Flash red for wrong answer
        foreach (RoundButton button in selectedButtons)
        {
            button.BackColor = Color.IndianRed;
        }

        this.Refresh();
        System.Threading.Thread.Sleep(300);

        // Reset selection
        foreach (RoundButton button in selectedButtons)
        {
            button.BackColor = Color.FromArgb(100, 149, 237);
        }

        pathPoints.Clear();
        selectedButtons.Clear();
        currentSum = 0;

        this.Invalidate();
    }

    private void GameTimer_Tick(object sender, EventArgs e)
    {
        timeLeft--;
        timerLabel.Text = $"⏱ {timeLeft} сек";

        if (timeLeft <= 10)
        {
            timerLabel.ForeColor = Color.OrangeRed;
        }

        if (timeLeft <= 0)
        {
            gameTimer.Stop();
            MessageBox.Show($"Время вышло! Ваш счет: {score}", "Игра окончена");
            timeLeft = 45;
            score = 0;
            scoreLabel.Text = $"Очки: {score}";
            timerLabel.ForeColor = Color.White;
            StartNewRound();
            gameTimer.Start();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (pathPoints.Count > 1)
        {
            using (Pen pen = new Pen(Color.FromArgb(70, 130, 180), 4))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                e.Graphics.DrawLines(pen, pathPoints.ToArray());

                // Draw connection points
                foreach (Point p in pathPoints)
                {
                    e.Graphics.FillEllipse(Brushes.White, p.X - 5, p.Y - 5, 10, 10);
                    e.Graphics.DrawEllipse(new Pen(Color.FromArgb(70, 130, 180), 2),
                                         p.X - 5, p.Y - 5, 10, 10);
                }
            }
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new CircleNumberGame());
    }
}

public class RoundButton : Button
{
    protected override void OnPaint(PaintEventArgs e)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddEllipse(0, 0, ClientSize.Width, ClientSize.Height);
        this.Region = new Region(path);

        using (Brush brush = new SolidBrush(this.BackColor))
        {
            e.Graphics.FillEllipse(brush, 0, 0, ClientSize.Width, ClientSize.Height);
        }

        TextRenderer.DrawText(e.Graphics, Text, this.Font, this.ClientRectangle,
                            this.ForeColor,
                            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}


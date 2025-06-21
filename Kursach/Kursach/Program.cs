using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

public class NumberGame : Form
{
    private Button[,] buttons = new Button[4, 4];
    private Label taskLabel;
    private Label timerLabel;
    private Label levelLabel;
    private System.Windows.Forms.Timer timer;
    private int timeLeft = 30;
    private int level = 1;
    private int targetSum;
    private Point[] points = new Point[12];
    private int pointIndex = 0;
    private int currentSum = 0;
    private int[] selectedNumbers = new int[12];
    private int selectedCount = 0;

    public NumberGame()
    {
        InitializeComponents();
        GenerateRandomTask();
    }

    private void InitializeComponents()
    {
        this.Text = "Number Puzzle";
        this.Size = new Size(450, 450);
        this.BackColor = Color.FromArgb(240, 245, 255);
        this.DoubleBuffered = true;

        // Header panel
        Panel headerPanel = new Panel()
        {
            BackColor = Color.FromArgb(30, 60, 120),
            Size = new Size(this.Width, 70),
            Location = new Point(0, 0)
        };
        this.Controls.Add(headerPanel);

        // Game info labels
        levelLabel = new Label()
        {
            Text = $"Уровень: {level}",
            Location = new Point(20, 20),
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };

        timerLabel = new Label()
        {
            Text = $"⏱ {timeLeft} сек",
            Location = new Point(350, 20),
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };

        taskLabel = new Label()
        {
            Text = "Найдите числа, сумма которых равна:",
            Location = new Point(20, 80),
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 60, 120),
            AutoSize = true
        };

        headerPanel.Controls.Add(levelLabel);
        headerPanel.Controls.Add(timerLabel);
        this.Controls.Add(taskLabel);

        // Create buttons grid with modern style
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                buttons[i, j] = new Button()
                {
                    Text = (i * 4 + j + 1).ToString(),
                    Location = new Point(30 + j * 90, 120 + i * 70),
                    Size = new Size(80, 60),
                    Font = new Font("Arial", 14, FontStyle.Bold),
                    Tag = i * 4 + j + 1,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(200, 220, 255),
                    ForeColor = Color.FromArgb(30, 60, 120)
                };

                buttons[i, j].FlatAppearance.BorderSize = 0;
                buttons[i, j].FlatAppearance.MouseOverBackColor = Color.FromArgb(180, 200, 255);
                buttons[i, j].FlatAppearance.MouseDownBackColor = Color.FromArgb(150, 180, 255);

                buttons[i, j].Click += Button_Click;
                buttons[i, j].MouseEnter += Button_MouseEnter;
                buttons[i, j].MouseLeave += Button_MouseLeave;

                this.Controls.Add(buttons[i, j]);
            }
        }

        // Initialize timer
        timer = new System.Windows.Forms.Timer();
        timer.Interval = 1000;
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void GenerateRandomTask()
    {
        Random random = new Random();

        // Generate 3 unique random numbers between 1 and 9
        var numbers = Enumerable.Range(1, 9).OrderBy(x => random.Next()).Take(3).ToList();

        targetSum = numbers.Sum();
        taskLabel.Text = $"Найдите числа, сумма которых равна: {numbers[0]} + {numbers[1]} + {numbers[2]} = {targetSum}";

        // Reset selection
        currentSum = 0;
        selectedCount = 0;
        pointIndex = 0;

        // Reset buttons
        foreach (Button btn in buttons)
        {
            btn.BackColor = Color.FromArgb(200, 220, 255);
            btn.ForeColor = Color.FromArgb(30, 60, 120);
            btn.Enabled = true;
        }

        this.Invalidate();
    }

    private void Button_Click(object sender, EventArgs e)
    {
        Button clickedButton = sender as Button;
        int number = (int)clickedButton.Tag;

        // Check if button wasn't already selected
        if (clickedButton.BackColor != Color.FromArgb(30, 60, 120))
        {
            clickedButton.BackColor = Color.FromArgb(30, 60, 120);
            clickedButton.ForeColor = Color.White;
            currentSum += number;
            selectedNumbers[selectedCount] = number;
            selectedCount++;

            // Save center of button for drawing
            points[pointIndex] = new Point(
                clickedButton.Location.X + clickedButton.Width / 2,
                clickedButton.Location.Y + clickedButton.Height / 2);
            pointIndex++;

            this.Invalidate();

            // Check solution if 3 numbers selected
            if (selectedCount == 3)
            {
                if (currentSum == targetSum)
                {
                    timer.Stop();
                    MessageBox.Show($"Правильно! Переход на уровень {level + 1}", "Успех!",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    level++;
                    levelLabel.Text = $"Уровень: {level}";
                    timeLeft = 30;
                    timerLabel.Text = $"⏱ {timeLeft} сек";
                    GenerateRandomTask();
                    timer.Start();
                }
                else
                {
                    MessageBox.Show("Неправильная комбинация! Попробуйте снова.", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    GenerateRandomTask();
                }
            }
        }
    }

    private void Button_MouseEnter(object sender, EventArgs e)
    {
        Button button = sender as Button;
        if (button.BackColor != Color.FromArgb(30, 60, 120))
        {
            button.BackColor = Color.FromArgb(180, 200, 255);
        }
    }

    private void Button_MouseLeave(object sender, EventArgs e)
    {
        Button button = sender as Button;
        if (button.BackColor != Color.FromArgb(30, 60, 120))
        {
            button.BackColor = Color.FromArgb(200, 220, 255);
        }
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        timeLeft--;
        timerLabel.Text = $"⏱ {timeLeft} сек";

        if (timeLeft <= 5)
        {
            timerLabel.ForeColor = Color.FromArgb(255, 100, 100);
        }

        if (timeLeft <= 0)
        {
            timer.Stop();
            MessageBox.Show("Время вышло! Уровень будет перезапущен.", "Время",
                          MessageBoxButtons.OK, MessageBoxIcon.Warning);
            GenerateRandomTask();
            timeLeft = 30;
            timerLabel.Text = $"⏱ {timeLeft} сек";
            timerLabel.ForeColor = Color.White;
            timer.Start();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (pointIndex > 1)
        {
            using (Pen pen = new Pen(Color.FromArgb(30, 60, 120), 4))
            {
                pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

                Point[] drawingPoints = new Point[pointIndex];
                Array.Copy(points, drawingPoints, pointIndex);
                e.Graphics.DrawLines(pen, drawingPoints);

                // Draw circles at connection points
                foreach (Point p in drawingPoints)
                {
                    e.Graphics.FillEllipse(Brushes.White, p.X - 6, p.Y - 6, 12, 12);
                    e.Graphics.DrawEllipse(new Pen(Color.FromArgb(30, 60, 120), 2), p.X - 6, p.Y - 6, 12, 12);
                }
            }
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new NumberGame());
    }
}


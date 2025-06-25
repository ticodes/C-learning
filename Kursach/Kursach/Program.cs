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
    private Label sumLabel;
    private Label modeLabel;
    private System.Windows.Forms.Timer gameTimer;

    private int timeLeft = 45;
    private int score = 0;
    private int targetSum;
    private bool trainingMode = false;

    private List<Point> pathPoints = new List<Point>();
    private List<RoundButton> selectedButtons = new List<RoundButton>();
    private List<RoundButton> solutionButtons = new List<RoundButton>();
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
        this.Text = "Числовая Головоломка";
        this.ClientSize = new Size(450, 500);
        this.BackColor = Color.FromArgb(240, 248, 255);
        this.DoubleBuffered = true;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
    }

    private void InitializeControls()
    {
        // Header panel
        Panel headerPanel = new Panel
        {
            BackColor = Color.FromArgb(70, 130, 180),
            Size = new Size(this.ClientSize.Width, 100),
            Dock = DockStyle.Top
        };
        this.Controls.Add(headerPanel);

        // Score label
        scoreLabel = new Label
        {
            Text = $"Очки: {score}",
            Location = new Point(20, 15),
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };
        headerPanel.Controls.Add(scoreLabel);

        // Timer label
        timerLabel = new Label
        {
            Text = $"⏱ {timeLeft} сек",
            Location = new Point(350, 15),
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };
        headerPanel.Controls.Add(timerLabel);

        // Mode switch button
        Button modeButton = new Button
        {
            Text = "Переключить режим",
            Size = new Size(150, 30),
            Location = new Point((headerPanel.Width - 150) / 2, 15),
            Font = new Font("Arial", 10, FontStyle.Bold),
            BackColor = Color.LightGoldenrodYellow
        };
        modeButton.Click += ModeButton_Click;
        headerPanel.Controls.Add(modeButton);

        // Mode label
        modeLabel = new Label
        {
            Text = "Обычный режим",
            Location = new Point((headerPanel.Width - 100) / 2, 50),
            Font = new Font("Arial", 10, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };
        headerPanel.Controls.Add(modeLabel);

        // Target label
        targetLabel = new Label
        {
            Text = "Найдите числа, сумма которых:",
            Location = new Point(20, 110),
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 130, 180),
            AutoSize = true
        };
        this.Controls.Add(targetLabel);

        // Sum label
        sumLabel = new Label
        {
            Text = "Сумма: 0",
            Location = new Point(20, 140),
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(70, 130, 180),
            AutoSize = true
        };
        this.Controls.Add(sumLabel);

        // Create number buttons in grid
        for (int row = 0; row < GridSize; row++)
        {
            for (int col = 0; col < GridSize; col++)
            {
                RoundButton button = new RoundButton
                {
                    Size = new Size(ButtonRadius * 2, ButtonRadius * 2),
                    Location = new Point(40 + col * ButtonSpacing, 180 + row * ButtonSpacing),
                    Font = buttonFont,
                    BackColor = Color.FromArgb(100, 149, 237),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Tag = 0
                };

                button.FlatAppearance.BorderSize = 0;
                button.MouseDown += NumberButton_MouseDown;
                button.MouseEnter += NumberButton_MouseEnter;

                numberButtons.Add(button);
                this.Controls.Add(button);
            }
        }

        // Game timer
        gameTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        gameTimer.Tick += GameTimer_Tick;
        gameTimer.Start();
    }

    private void ModeButton_Click(object sender, EventArgs e)
    {
        trainingMode = !trainingMode;
        modeLabel.Text = trainingMode ? "Обучающий режим" : "Обычный режим";
        StartNewRound();
    }

    private void StartNewRound()
    {
        // Reset all buttons
        foreach (var button in numberButtons)
        {
            button.BackColor = Color.FromArgb(100, 149, 237);
            button.Enabled = true;
        }

        // Generate adjacent numbers that sum to target
        GenerateAdjacentNumbers();

        // Reset selection
        currentSum = 0;
        pathPoints.Clear();
        selectedButtons.Clear();
        sumLabel.Text = "Сумма: 0";
        isDrawing = false;

        this.Invalidate();
    }

    private void GenerateAdjacentNumbers()
    {
        // Generate random positions for 2-4 adjacent buttons
        solutionButtons.Clear();
        List<RoundButton> adjacentButtons = FindAdjacentButtons();

        if (adjacentButtons.Count < 2)
        {
            MessageBox.Show("Не удалось создать задание. Попробуйте еще раз.");
            return;
        }

        // Randomly select 2-4 adjacent buttons
        int count = random.Next(2, Math.Min(5, adjacentButtons.Count + 1));

        // Calculate target sum from these buttons
        targetSum = 0;
        foreach (var btn in adjacentButtons.GetRange(0, count))
        {
            int num = random.Next(1, 10);
            btn.Tag = num;
            btn.Text = num.ToString();
            solutionButtons.Add(btn);
            targetSum += num;
        }

        // Fill remaining buttons with random numbers
        foreach (var btn in numberButtons)
        {
            if (!solutionButtons.Contains(btn))
            {
                btn.Tag = random.Next(1, 10);
                btn.Text = btn.Tag.ToString();
            }
        }

        // Update target label
        if (trainingMode)
        {
            string example = "Пример: " + string.Join(" + ", solutionButtons.ConvertAll(b => b.Tag.ToString())) +
                           " = " + targetSum;
            targetLabel.Text = example;
        }
        else
        {
            targetLabel.Text = $"Найдите числа, сумма которых: {targetSum}";
        }
    }

    private List<RoundButton> FindAdjacentButtons()
    {
        List<RoundButton> adjacentButtons = new List<RoundButton>();
        RoundButton startButton = numberButtons[random.Next(numberButtons.Count)];
        adjacentButtons.Add(startButton);

        // Find random adjacent buttons (horizontally, vertically or diagonally)
        while (adjacentButtons.Count < 4)
        {
            RoundButton last = adjacentButtons[adjacentButtons.Count - 1];
            int lastIndex = numberButtons.IndexOf(last);
            int row = lastIndex / GridSize;
            int col = lastIndex % GridSize;

            List<RoundButton> possibleAdjacent = new List<RoundButton>();

            // Check all 8 possible directions
            for (int r = -1; r <= 1; r++)
            {
                for (int c = -1; c <= 1; c++)
                {
                    if (r == 0 && c == 0) continue;

                    int newRow = row + r;
                    int newCol = col + c;

                    if (newRow >= 0 && newRow < GridSize && newCol >= 0 && newCol < GridSize)
                    {
                        RoundButton adjButton = numberButtons[newRow * GridSize + newCol];
                        if (!adjacentButtons.Contains(adjButton))
                            possibleAdjacent.Add(adjButton);
                    }
                }
            }

            if (possibleAdjacent.Count == 0) break;

            adjacentButtons.Add(possibleAdjacent[random.Next(possibleAdjacent.Count)]);
        }

        return adjacentButtons;
    }

    private void NumberButton_MouseDown(object sender, MouseEventArgs e)
    {
        if (!isDrawing)
        {
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
            // Check if the new button is adjacent to the last selected one
            if (selectedButtons.Count > 0)
            {
                RoundButton last = selectedButtons[selectedButtons.Count - 1];
                if (!AreButtonsAdjacent(last, button))
                    return;
            }

            AddButtonToPath(button);

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

    private bool AreButtonsAdjacent(RoundButton btn1, RoundButton btn2)
    {
        int index1 = numberButtons.IndexOf(btn1);
        int index2 = numberButtons.IndexOf(btn2);

        int row1 = index1 / GridSize;
        int col1 = index1 % GridSize;
        int row2 = index2 / GridSize;
        int col2 = index2 % GridSize;

        return Math.Abs(row1 - row2) <= 1 && Math.Abs(col1 - col2) <= 1;
    }

    private void AddButtonToPath(RoundButton button)
    {
        button.BackColor = trainingMode ? Color.Gold : Color.FromArgb(70, 130, 180);
        selectedButtons.Add(button);
        currentSum += (int)button.Tag;

        UpdateSumLabel();

        pathPoints.Add(new Point(
            button.Left + button.Width / 2,
            button.Top + button.Height / 2));

        this.Invalidate();
    }

    private void UpdateSumLabel()
    {
        string sumText = string.Join(" + ", selectedButtons.ConvertAll(b => b.Tag.ToString()));
        sumLabel.Text = $"Сумма: {sumText} = {currentSum}";
    }

    private void ProcessCorrectAnswer()
    {
        isDrawing = false;

        if (trainingMode)
        {
            if (selectedButtons.Count == solutionButtons.Count)
            {
                foreach (var btn in selectedButtons)
                {
                    btn.BackColor = Color.LimeGreen;
                }
                MessageBox.Show("Правильно! Этот пример решен верно.", "Поздравляю!");
            }
            else
            {
                MessageBox.Show("Числа выбраны правильно, но не в том порядке.", "Подсказка");
            }
        }
        else
        {
            score += targetSum;
            scoreLabel.Text = $"Очки: {score}";

            foreach (var btn in selectedButtons)
            {
                btn.BackColor = Color.LimeGreen;
            }
        }

        this.Refresh();
        System.Threading.Thread.Sleep(300);
        StartNewRound();
    }

    private void ProcessWrongAnswer()
    {
        isDrawing = false;

        foreach (var btn in selectedButtons)
        {
            btn.BackColor = Color.IndianRed;
        }

        this.Refresh();
        System.Threading.Thread.Sleep(300);

        // Reset selection
        foreach (var btn in selectedButtons)
        {
            btn.BackColor = trainingMode ? Color.Gold : Color.FromArgb(100, 149, 237);
        }

        pathPoints.Clear();
        selectedButtons.Clear();
        currentSum = 0;
        sumLabel.Text = "Сумма: 0";

        this.Invalidate();
    }

    private void GameTimer_Tick(object sender, EventArgs e)
    {
        if (!trainingMode)
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
                gameTimer.Start();
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (pathPoints.Count > 1)
        {
            using (Pen pen = new Pen(trainingMode ? Color.Gold : Color.FromArgb(70, 130, 180), 4))
            {
                if (trainingMode)
                {
                    pen.DashStyle = DashStyle.Dash;
                    pen.DashPattern = new float[] { 5, 5 };
                }

                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                pen.LineJoin = LineJoin.Round;

                e.Graphics.DrawLines(pen, pathPoints.ToArray());

                // Draw connection points
                foreach (Point p in pathPoints)
                {
                    e.Graphics.FillEllipse(Brushes.White, p.X - 5, p.Y - 5, 10, 10);
                    e.Graphics.DrawEllipse(new Pen(pen.Color, 2), p.X - 5, p.Y - 5, 10, 10);
                }
            }
        }

        // Highlight solution in training mode
        if (trainingMode && solutionButtons.Count > 0 && selectedButtons.Count == 0)
        {
            using (Pen pen = new Pen(Color.Gold, 4) { DashStyle = DashStyle.Dash })
            {
                List<Point> solutionPoints = new List<Point>();
                foreach (var btn in solutionButtons)
                {
                    solutionPoints.Add(new Point(
                        btn.Left + btn.Width / 2,
                        btn.Top + btn.Height / 2));
                }

                if (solutionPoints.Count > 1)
                {
                    e.Graphics.DrawLines(pen, solutionPoints.ToArray());
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


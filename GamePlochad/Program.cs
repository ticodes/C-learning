using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AreaGame
{
    public partial class MainForm : Form
    {
        // Игровые переменные
        private int score = 0;
        private int timeLeft = 30;
        private int difficulty = 1; // 1-легкий, 2-средний, 3-сложный
        private Timer gameTimer;
        private Rectangle outerRect;
        private Rectangle innerRect;
        private bool gameRunning = false;
        private Random random = new Random();
        private List<int> answersHistory = new List<int>();

        // Для хранения рекордов
        private Dictionary<int, List<int>> records = new Dictionary<int, List<int>>();
        private const int CellSize = 20; // Одна клетка = 20 на 20 пикселей

        public MainForm()
        {
            InitializeComponent();
            InitializeGame();
            LoadRecords();

            // Настройка таймера
            gameTimer = new Timer();
            gameTimer.Interval = 1000;
            gameTimer.Tick += GameTimer_Tick;
        }

        private void InitializeGame()
        {
            // Настройка размеров формы
            this.ClientSize = new Size(800, 600);
            this.Text = "Игра 'Площадь'";
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.Paint += MainForm_Paint;

            // Создание элементов управления
            SetupUI();
        }

        private void SetupUI()
        {
            // Очистка контролов
            this.Controls.Clear();

            // Главное меню
            if (!gameRunning)
            {
                var title = new Label
                {
                    Text = "Игра 'Площадь'",
                    Font = new Font("Arial", 24, FontStyle.Bold),
                    Location = new Point(250, 50),
                    AutoSize = true
                };

                var easyBtn = new Button
                {
                    Text = "Легкий уровень",
                    Location = new Point(300, 150),
                    Size = new Size(200, 50)
                };
                easyBtn.Click += (s, e) => StartGame(1);

                var mediumBtn = new Button
                {
                    Text = "Средний уровень",
                    Location = new Point(300, 220),
                    Size = new Size(200, 50)
                };
                mediumBtn.Click += (s, e) => StartGame(2);

                var hardBtn = new Button
                {
                    Text = "Сложный уровень",
                    Location = new Point(300, 290),
                    Size = new Size(200, 50)
                };
                hardBtn.Click += (s, e) => StartGame(3);

                var recordsBtn = new Button
                {
                    Text = "Рекорды",
                    Location = new Point(300, 360),
                    Size = new Size(200, 50)
                };
                recordsBtn.Click += ShowRecords;

                this.Controls.AddRange(new Control[] { title, easyBtn, mediumBtn, hardBtn, recordsBtn });
            }
        }

        private void StartGame(int difficultyLevel)
        {
            difficulty = difficultyLevel;
            score = 0;
            timeLeft = 30;
            answersHistory.Clear();
            gameRunning = true;
            gameTimer.Start();

            // Очистка контролов
            this.Controls.Clear();

            // Генерация прямоугольников
            GenerateRectangles();

            // Создание элементов игрового интерфейса
            CreateGameUI();

            // Обновление экрана
            this.Invalidate();
        }

        private void CreateGameUI()
        {


            // Панель информации
            var infoPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(780, 40),
                BackColor = Color.LightGray
            };

            var scoreLabel = new Label
            {
                Text = $"Очки: {score}",
                Location = new Point(10, 10),
                AutoSize = true
            };

            var timeLabel = new Label
            {
                Text = $"Время: {timeLeft}",
                Location = new Point(200, 10),
                AutoSize = true
            };

            var difficultyLabel = new Label
            {
                Text = $"Уровень: {GetDifficultyName()}",
                Location = new Point(400, 10),
                AutoSize = true
            };

            infoPanel.Controls.AddRange(new Control[] { scoreLabel, timeLabel, difficultyLabel });

            // Поле для ввода ответа
            var answerBox = new TextBox
            {
                Location = new Point(300, 500),
                Size = new Size(100, 30)
            };

            var submitBtn = new Button
            {
                Text = "Ответить",
                Location = new Point(420, 500),
                Size = new Size(100, 30)
            };
            submitBtn.Click += (s, e) => CheckAnswer(answerBox.Text);

            // Кнопка выхода в меню
            var menuBtn = new Button
            {
                Text = "В меню",
                Location = new Point(700, 550),
                Size = new Size(80, 30)
            };
            menuBtn.Click += (s, e) => ReturnToMenu();


            // В методе CreateGameUI стилизация:
            submitBtn.BackColor = Color.LimeGreen;
            submitBtn.ForeColor = Color.White;
            submitBtn.FlatStyle = FlatStyle.Flat;
            submitBtn.FlatAppearance.BorderSize = 0;
            menuBtn.BackColor = Color.Orange;
            menuBtn.ForeColor = Color.White;


            this.Controls.AddRange(new Control[] { infoPanel, answerBox, submitBtn, menuBtn });
        }

        private void GenerateRectangles()
        {
            int widthCells, heightCells; // Размеры в клетках!

            // Генерация размеров В КЛЕТКАХ
            switch (difficulty)
            {
                case 1: // Легкий (5-15 клеток)
                    widthCells = random.Next(5, 16);
                    heightCells = random.Next(5, 16);
                    break;
                case 2: // Средний (3-20 клеток)
                    widthCells = random.Next(3, 21);
                    heightCells = random.Next(3, 21);
                    break;
                case 3: // Сложный (2-25 клеток)
                    widthCells = random.Next(2, 26);
                    heightCells = random.Next(2, 26);
                    break;
                default:
                    widthCells = 10;
                    heightCells = 10;
                    break;
            }

            // Внешний прямоугольник (в пикселях)
            int outerX = random.Next(10, this.ClientSize.Width - widthCells * CellSize - 10);
            int outerY = random.Next(10, this.ClientSize.Height - heightCells * CellSize - 10);
            outerRect = new Rectangle(
                outerX,
                outerY,
                widthCells * CellSize,  // Конвертируем клетки в пиксели
                heightCells * CellSize
            );

            // Внутренний прямоугольник (отступы в клетках!)
            int marginCells = random.Next(1, Math.Min(widthCells, heightCells) / 2 + 1); // Отступ 1-3 клетки
            innerRect = new Rectangle(
                outerX + marginCells * CellSize,
                outerY + marginCells * CellSize,
                (widthCells - 2 * marginCells) * CellSize,  // Ширина в клетках → пикселях
                (heightCells - 2 * marginCells) * CellSize  // Высота в клетках → пикселях
            );
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            if (!gameRunning) return;
            Graphics g = e.Graphics;

            // Отрисовка сетки (уже привязана к CellSize)
            DrawGrid(g);

            // Отрисовка прямоугольников
            g.FillRectangle(Brushes.LightBlue, outerRect);
            g.FillRectangle(Brushes.Blue, innerRect);

            // Подсказка (теперь показывает клетки)
            //string sizeText = $"Клеток: {outerRect.Width / CellSize}×{outerRect.Height / CellSize}";
            //g.DrawString(sizeText, new Font("Arial", 12), Brushes.Black, 10, 60);
        }

        private void DrawGrid(Graphics g)
        {
            // Рисуем сетку 20x20 пикселей
            Pen gridPen = new Pen(Color.LightGray, 1);

            for (int x = 0; x < this.ClientSize.Width; x += CellSize)
            {
                g.DrawLine(gridPen, x, 0, x, this.ClientSize.Height);
            }

            for (int y = 0; y < this.ClientSize.Height; y += CellSize)
            {
                g.DrawLine(gridPen, 0, y, this.ClientSize.Width, y);
            }
        }

        private void CheckAnswer(string answerText)
        {
            try
            {
                // Проверка, является ли ввод целым числом
                if (!int.TryParse(answerText, out int userAnswer))
                {
                    MessageBox.Show("Пожалуйста, введите целое число.");
                    return;
                }

                // Проверка наличия элементов управления
                if (this.Controls.Count > 0 && this.Controls[0].Controls.Count > 0)
                {
                    var scoreLabel = (Label)this.Controls[0].Controls[0];
                    scoreLabel.Text = $"Очки: {score}";
                }

                // Вычисляем правильный ответ как площадь внутреннего прямоугольника в клетках
                int correctAnswer = (innerRect.Width / CellSize) * (innerRect.Height / CellSize);
                answersHistory.Add(correctAnswer); // Сохраняем правильный ответ в истории

                // Отладочный вывод для проверки значений
                Console.WriteLine($"Внутренний прямоугольник: {innerRect.Width / CellSize}x{innerRect.Height / CellSize} клеток");
                Console.WriteLine($"Правильный ответ: {correctAnswer}, Ваш ответ: {userAnswer}");

                // Проверка, совпадает ли ответ пользователя с правильным ответом
                if (userAnswer == correctAnswer)
                {
                    // Начисляем очки в зависимости от сложности
                    score += 10 * difficulty;

                    // Обновляем отображение очков
                    if (this.Controls.Count > 0 && this.Controls[0].Controls.Count > 0)
                    {
                        var scoreLabel = (Label)this.Controls[0].Controls[0];
                        scoreLabel.Text = $"Очки: {score}";
                    }

                    // Генерируем новые прямоугольники
                    GenerateRectangles();
                    this.Invalidate(); // Обновляем экран

                    // Анимация успеха
                    AnimateSuccess();
                }
                else
                {
                    // Анимация ошибки
                    AnimateError();
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку в консоль
                Console.WriteLine($"Ошибка: {ex.Message}");
                // Можно также добавить обработку, чтобы не показывать окно с ошибкой
            }
        }



        private void AnimateSuccess()
        {
            var animPanel = new Panel
            {
                BackColor = Color.FromArgb(100, Color.Green),
                Size = this.ClientSize,
                Location = Point.Empty
            };

            var label = new Label
            {
                Text = "✓ Правильно!",
                Font = new Font("Arial", 36, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(
                    this.ClientSize.Width / 2 - 120,
                    this.ClientSize.Height / 2 - 30
                )
            };

            animPanel.Controls.Add(label);
            this.Controls.Add(animPanel);
            animPanel.BringToFront();

            Timer animTimer = new Timer { Interval = 500 };
            animTimer.Tick += (s, e) =>
            {
                this.Controls.Remove(animPanel);
                animTimer.Stop();
            };
            animTimer.Start();
        }

        private void AnimateError()
        {
            var animPanel = new Panel
            {
                BackColor = Color.FromArgb(100, Color.Red),
                Size = this.ClientSize,
                Location = Point.Empty
            };

            var label = new Label
            {
                Text = "X Ошибка",
                Font = new Font("Arial", 36, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(
                    this.ClientSize.Width / 2 - 80,
                    this.ClientSize.Height / 2 - 30
                )
            };

            animPanel.Controls.Add(label);
            this.Controls.Add(animPanel);
            animPanel.BringToFront();

            Timer animTimer = new Timer { Interval = 500 };
            animTimer.Tick += (s, e) =>
            {
                this.Controls.Remove(animPanel);
                animTimer.Stop();
            };
            animTimer.Start();
        }
        //------------------------------------------------------------

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                timeLeft--;

                // Проверка наличия элементов управления
                if (this.Controls.Count > 0 && this.Controls[0].Controls.Count > 1)
                {
                    var timeLabel = (Label)this.Controls[0].Controls[1];
                    timeLabel.Text = $"Время: {timeLeft}";
                }

                if (timeLeft <= 0)
                {
                    EndGame(); //если истекло время, то вызываем метод
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку в консоль
                Console.WriteLine($"Ошибка в GameTimer_Tick: {ex.Message}");
                
            }
        }



        private void EndGame()
        {
            gameTimer.Stop();
            gameRunning = false;

            // Вычисляем правильный ответ для последнего прямоугольника
            int lastCorrectAnswer = (innerRect.Width / CellSize) * (innerRect.Height / CellSize);

            // Сохраняем результат
            SaveRecord();

            // Показываем результаты + правильный ответ
            var result = MessageBox.Show(
                "Время вышло!\n" +
                $"Ваш счет: {score}\n" +
                $"Правильный ответ: {lastCorrectAnswer} клеток\n\n" +
                "Хотите посмотреть рекорды?",
                "Игра завершена",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                ShowRecords(this, EventArgs.Empty);
            }
            else
            {
                ShowGameResults();
            }
        }

        private void SaveRecord()
        {
            if (!records.ContainsKey(difficulty))
            {
                records[difficulty] = new List<int>();
            }

            records[difficulty].Add(score);

            // Сохраняем в файл
            try
            {
                using (StreamWriter writer = new StreamWriter("records.txt"))
                {
                    foreach (var level in records.Keys)
                    {
                        writer.WriteLine($"Level{level}:{string.Join(",", records[level])}");
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки сохранения
            }
        }

        private void LoadRecords()
        {
            records = new Dictionary<int, List<int>>();

            try
            {
                if (File.Exists("records.txt"))
                {
                    foreach (string line in File.ReadAllLines("records.txt"))
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 2 && parts[0].StartsWith("Level"))
                        {
                            int level = int.Parse(parts[0].Substring(5));
                            var scores = parts[1].Split(',').Where(s => !string.IsNullOrEmpty(s)).Select(int.Parse).ToList();
                            records[level] = scores;
                        }
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки загрузки
            }
        }

        private void ShowGameResults()
        {
            this.Controls.Clear();


            var title = new Label
            {
                Text = "Игра завершена!",
                Font = new Font("Arial", 24, FontStyle.Bold),
                Location = new Point(250, 50),
                AutoSize = true
            };

            var scoreLabel = new Label
            {
                Text = $"Ваш счет: {score}",
                Font = new Font("Arial", 18),
                Location = new Point(300, 120),
                AutoSize = true
            };

            var avgLabel = new Label
            {
                Text = $"Средний ответ: {(answersHistory.Count > 0 ? answersHistory.Average() : 0):F2}",
                Location = new Point(300, 170),
                AutoSize = true
            };

            var recordsBtn = new Button
            {
                Text = "Рекорды",
                Location = new Point(300, 220),
                Size = new Size(200, 50)
            };
            recordsBtn.Click += ShowRecords;

            var menuBtn = new Button
            {
                Text = "В меню",
                Location = new Point(300, 290),
                Size = new Size(200, 50)
            };
            menuBtn.Click += (s, e) => ReturnToMenu();

            this.Controls.AddRange(new Control[] { title, scoreLabel, avgLabel, recordsBtn, menuBtn });
        }

        private void ShowRecords(object sender, EventArgs e)
        {
            this.Controls.Clear();

            var title = new Label
            {
                Text = "Рекорды",
                Font = new Font("Arial", 24, FontStyle.Bold),
                Location = new Point(300, 20),
                AutoSize = true
            };

            // Показываем рекорды для каждого уровня
            int yPos = 80;
            foreach (var level in records.Keys.OrderBy(k => k))
            {
                var levelLabel = new Label
                {
                    Text = $"{GetDifficultyName(level)} уровень:",
                    Font = new Font("Arial", 14, FontStyle.Bold),
                    Location = new Point(50, yPos),
                    AutoSize = true
                };

                var topRecords = records[level].OrderByDescending(s => s).Take(5).ToList();

                var recordsLabel = new Label
                {
                    Text = topRecords.Any() ? string.Join(", ", topRecords) : "Нет результатов",
                    Location = new Point(250, yPos),
                    AutoSize = true
                };

                yPos += 30;

                this.Controls.Add(levelLabel);
                this.Controls.Add(recordsLabel);
            }

            var menuBtn = new Button
            {
                Text = "В меню",
                Location = new Point(300, yPos + 30),
                Size = new Size(200, 50)
            };
            menuBtn.Click += ReturnToMenu;

            this.Controls.Add(title);
            this.Controls.Add(menuBtn);
        }

        private void ReturnToMenu(object sender = null, EventArgs e = null)
        {
            gameRunning = false;
            gameTimer.Stop();
            SetupUI();
        }

        private string GetDifficultyName(int? level = null)
        {
            int lvl = level ?? difficulty;
            switch (lvl)
            {
                case 1: return "Легкий";
                case 2: return "Средний";
                case 3: return "Сложный";
                default: return "Неизвестный";
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}


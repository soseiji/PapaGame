using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace BlockBreaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum BlockType 
        {
            // モンスターボール的な
            NormalBlock = 0,
            SuperBlock,
            HyperBlock,
            MasterBlock,
        }

        public class Block
        {
            public int Number { get; set; }
            public Border Control { get; set; }
            public Rect Rect { get; set; }
        }
        public class BlockParam 
        {
            public BlockType BlockType { get; set; }
            public CornerRadius CornerRadius { get; set; }
            public LinearGradientBrush Color { get; set; }
            public LinearGradientBrush BorderColor { get; set; }
            public Thickness BorderThickness { get; set; }
            public int Score { get; set; }

            public BlockParam()
            {
                BlockType = BlockType.NormalBlock;
                var value = new Random().Next(0, 200);
                if (1 <= value && value <= 50) BlockType = BlockType.SuperBlock;
                if (51 <= value && value <= 60) BlockType = BlockType.HyperBlock;
                if (100 <= value && value <= 101) BlockType = BlockType.MasterBlock;

                BorderColor = new LinearGradientBrush(Colors.Silver, Colors.DimGray, new Point(0.5, 1), new Point(0.5, 0));
                BorderThickness = new Thickness(2);

                switch (BlockType)
                {
                    case BlockType.NormalBlock:
                        CornerRadius = new CornerRadius(20);
                        Color = new LinearGradientBrush(Colors.LightGreen, Colors.GreenYellow, new Point(0.5, 1), new Point(0.5, 0));
                        Score = 1;
                        break;
                    case BlockType.SuperBlock:
                        CornerRadius = new CornerRadius(40);
                        Color = new LinearGradientBrush(Colors.DeepPink, Colors.Pink, new Point(0.5, 1), new Point(0.5, 0));
                        Score = 3;
                        break;
                    case BlockType.HyperBlock:
                        CornerRadius = new CornerRadius(100);
                        Color = new LinearGradientBrush(Colors.LightGray, Colors.AntiqueWhite, new Point(0.5, 1), new Point(0.5, 0));
                        Score = 10;
                        break;
                    case BlockType.MasterBlock:
                        CornerRadius = new CornerRadius(100);
                        Color = new LinearGradientBrush(Colors.Orange, Colors.Yellow, new Point(0.5, 1), new Point(0.5, 0));
                        Score = 100;
                        break;
                    default:
                        CornerRadius = new CornerRadius(0);
                        Color = new LinearGradientBrush(Colors.DarkRed, Colors.Red, new Point(0.5, 1), new Point(0.5, 0));
                        break;
                }
            }
        }

        private List<Block> blockList = new List<Block>();
        private Vector barPosition = new Vector(-100, -100);
        private Vector ballPosition = new Vector(-100, -100);
        private Vector ballSpeed = new Vector(0, 0);
        private double speedMax;
        private double speedRate;
        private int score = 0;
        private int level = 1;
        private int dropCount = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Window->ContentRenderedイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // 背景セット
            SetImage();

            // ブロック生成
            CreateBlockList();

            // ゲーム開始
            var timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 1) };
            timer.Tick += (senderTick, eTick) =>
            {
                // 領域サイズの取得
                var areaWidth = this.CanvasArea.ActualWidth;
                var areaHeight = this.CanvasArea.ActualHeight;

                // 領域判定（左）
                if (ballPosition.X < 0)
                {
                    MoveBall(0, ballPosition.Y);
                    ballSpeed.X *= -1;
                }

                // 領域判定（右）
                if (areaWidth < ballPosition.X + this.EllipseBall.ActualWidth)
                {
                    MoveBall(areaWidth - this.EllipseBall.ActualWidth, ballPosition.Y);
                    ballSpeed.X *= -1;
                }

                // 領域判定（上）
                if (ballPosition.Y < 0)
                {
                    MoveBall(ballPosition.X, 0);
                    ballSpeed.Y *= -1;
                }

                // 領域判定（下）
                if (areaHeight < ballPosition.Y + this.EllipseBall.ActualHeight)
                {                   
                    if (this.CheckBoxEndless.IsChecked.Value)
                    {
                        // エンドレスモード
                        MoveBall(ballPosition.X, areaHeight - this.EllipseBall.ActualHeight);
                        ballSpeed.Y *= -1;

                        SetDropCount(dropCount + 1);
                    }
                    else
                    {
                        // おわり：点数表示
                        if ((ballSpeed.X == 0) && (ballSpeed.Y == 0)) return;
                        ballSpeed.X = 0;
                        ballSpeed.Y = 0;
                        MessageBox.Show($"てんすう：{score}　{GetScoreMessage(score)}", "ホッケー", MessageBoxButton.OK, MessageBoxImage.Information);
                        ButtonBall.IsEnabled = true;
                    }
                }

                // バー衝突判定
                const double barPositionOffsetX = -20;  // 左端にあたりづらいので
                var barRect = level < 13 ?   // レベルが高いと、早くて当たらないので範囲をひろげておく
                    new Rect(barPosition.X + barPositionOffsetX, barPosition.Y, this.RectangleBar.ActualWidth, this.RectangleBar.ActualHeight) :
                    new Rect(barPosition.X + barPositionOffsetX, barPosition.Y, this.RectangleBar.ActualWidth + 200, this.RectangleBar.ActualHeight + 200);  
                if (barRect.Contains(ballPosition.X, ballPosition.Y + this.EllipseBall.ActualHeight))
                {
                    if (0 < ballSpeed.Y) ballSpeed.Y *= -1;
                }

                // ブロック衝突判定
                foreach (var block in blockList)
                {
                    if (block.Control.Visibility != Visibility.Visible) continue;
                    var rect = level < 13 ?     // レベルが高いと、早くて当たらないので範囲をひろげておく
                        block.Rect :
                        new Rect(block.Rect.X, block.Rect.Y, block.Rect.Width + 150, block.Rect.Height + 150);

                    if (rect.Contains(ballPosition.X, ballPosition.Y)) HitBlock(block.Control);
                }

                // ボール移動
                var movePosition = ballPosition + ballSpeed;
                MoveBall(movePosition.X, movePosition.Y);

                // ブロック再生成（すべて非表示の場合）
                if (!blockList.Any(x => x.Control.Visibility == Visibility.Visible))
                {
                    // タイマー停止
                    timer.Stop();

                    // ボール初期化
                    InitializeBall();

                    // 最大スピードアップ
                    speedMax += 3.0;
                    speedRate += 0.1;

                    // レベルアップ
                    SetLevel(level + 1);

                    // 再表示
                    CreateBlockList();
                    //foreach (var block in blockList)
                    //{
                    //    block.Control.Visibility = Visibility.Visible;
                    //    //block.Image.Source = GetBitmap(block.Number, level);
                    //}

                    // タイマー開始
                    timer.Start();
                }
            };
            timer.Start();
        }

        /// <summary>
        /// ブロックを生成する
        /// </summary>
        /// <param name="count">生成数</param>
        private void CreateBlockList(int count = 48)
        {
            blockList.Clear();

            var row = 1;
            var column = 1;
            foreach (var number in Enumerable.Range(1, count))
            {
                var blockParam = new BlockParam();

                // 画像生成
                var control = new Border()
                {
                    Name = $"Image{number}",
                    Width = 100,
                    Height = 100,
                    Tag = blockParam.Score,
                    Background = blockParam.Color,
                    CornerRadius = blockParam.CornerRadius,
                    BorderBrush = blockParam.BorderColor,
                    BorderThickness = blockParam.BorderThickness,
                };
                this.CanvasArea.Children.Add(control);

                // 座標設定
                if (IsOverArea(blockList.LastOrDefault()))
                {
                    column = 1;
                    row++;
                }
                var positionX = column * control.Width;
                var positionY = row * control.Height;
                column++;

                // 画像配置
                Canvas.SetLeft(control, positionX);
                Canvas.SetTop(control, positionY);

                // ブロック情報の保持
                var rect = new Rect(positionX, positionY, control.Width, control.Height);
                blockList.Add(new Block()
                {
                    Number = number,
                    Control = control,
                    Rect = rect,
                });
            }
        }

        /// <summary>
        /// ブロックが表示領域を超えているか判定する
        /// </summary>
        /// <param name="block">ブロック</param>
        /// <returns>true:超えた、false:超えていない</returns>
        private bool IsOverArea(Block block)
        {
            if (block == null) return false;
            var margin = 100;
            var areaWidth = this.CanvasArea.ActualWidth;
            return (areaWidth < block.Rect.X + block.Rect.Width + margin);
        }

        /// <summary>
        /// ブロックを衝突処理を行う
        /// </summary>
        /// <param name="control">イメージ</param>
        private void HitBlock(Border control)
        {
            // ブロック非表示
            if (control.Visibility != Visibility.Visible) return;
            control.Visibility = Visibility.Collapsed;

            // スコアアップ
            SetScore(control);

            // スピードアップ
            if (speedMax < Math.Abs(ballSpeed.X)) return;
            if (speedMax < Math.Abs(ballSpeed.Y)) return;
            ballSpeed *= speedRate;     // Vector に乗算できるのが処理のポイント。
        }

        /// <summary>
        /// スコアを設定する
        /// </summary>
        /// <param name="value">スコア</param>
        private void SetScore(Border control)
        {
            if (control == null)
            {
                score = 0;
            }
            else 
            {
                score += (int)control.Tag * level;
            }            
            this.RunScore.Text = score.ToString();
        }

        /// <summary>
        /// レベルを設定する
        /// </summary>
        /// <param name="value">レベル</param>
        private void SetLevel(int value)
        {
            level = value;
            this.RunLevel.Text = value.ToString();

            SetImage();
        }

        private void SetDropCount(int value)
        {
            dropCount = value;
            this.RunDrop.Text = dropCount.ToString();        
        }

        private void SetImage() 
        {
            //var imageName = (level == 1) ? "00" : new Random().Next(1, 11).ToString("D2");

            //var bmpImage = new BitmapImage();
            //using (FileStream stream = File.OpenRead($@"Images/{imageName}.jpg"))
            //{
            //    bmpImage.BeginInit();
            //    bmpImage.StreamSource = stream;
            //    bmpImage.DecodePixelWidth = 500;
            //    bmpImage.CacheOption = BitmapCacheOption.OnLoad;
            //    bmpImage.CreateOptions = BitmapCreateOptions.None;
            //    bmpImage.EndInit();
            //    bmpImage.Freeze();
            //}
            //ImageBack.Source = bmpImage;
        }

        /// <summary>
        /// スコアメッセージを取得する
        /// </summary>
        /// <param name="score">スコア</param>
        /// <returns>メッセージ</returns>
        private string GetScoreMessage(double score)
        {
            if (100000 <= score) return "うんち もれた (;´Д｀)";
            if (10000 <= score) return "ひ、ひぃぃぃぃぃ～！　ごめん！！！ごめんなさい。もう許してください。(;´Д｀)";
            if (5000 <= score) return "ごめんなさい。もう許してください。(;´Д｀)";
            if (7000 <= score) return "まいりました。ごめんなさい。(;´Д｀)";
            if (3000 <= score) return "すごい！すごいよ！！！ ( ﾟДﾟ)";
            if (1000 <= score) return "すごい、じょうずだね。( ｀ー´)ノ";
            if (500 <= score) return "じょうずだね。(^^♪";
            if (300 <= score) return "ちょっと、がんばったね。(; ･`д･´)";
            return "がんばりましょう。('ω')ノ";
        }

        /// <summary>
        /// バーを移動する
        /// </summary>
        /// <param name="positionX">X座標</param>
        /// <param name="positionY">Y座標</param>
        private void MoveBar(double positionX, double positionY)
        {
            const double positionMinY = 100;
            if (positionY < positionMinY) positionY = positionMinY;
            Canvas.SetLeft(this.RectangleBar, positionX);
            Canvas.SetTop(this.RectangleBar, positionY);
            barPosition.X = positionX;
            barPosition.Y = positionY;
        }

        /// <summary>
        /// ボールを移動する
        /// </summary>
        /// <param name="positionX">X座標</param>
        /// <param name="positionY">Y座標</param>
        private void MoveBall(double positionX, double positionY)
        {
            Canvas.SetLeft(this.EllipseBall, positionX);
            Canvas.SetTop(this.EllipseBall, positionY);
            ballPosition.X = positionX;
            ballPosition.Y = positionY;
        }

        /// <summary>
        /// ボールを初期化する
        /// </summary>
        private void InitializeBall()
        {
            // ボール速度設定
            ballSpeed.X = 1;
            ballSpeed.Y = 2;

            // ボール移動
            MoveBall(0, 0);
        }

        /// <summary>
        /// Window->MouseMoveイベント
        /// </summary>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            const double offsetX = -150.0;
            const double offsetY = -50.0;
            var position = e.GetPosition(this);
            MoveBar(position.X + offsetX, position.Y + offsetY);
        }

        /// <summary>
        /// Window->KeyDownイベント
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Right:
                    if (ballSpeed.X < 0) ballSpeed.X *= -1;
                    break;
                case Key.Left:
                    if (0 < ballSpeed.X) ballSpeed.X *= -1;
                    break;
                case Key.Down:
                    if (ballSpeed.Y < 0) ballSpeed.Y *= -1;
                    break;
                case Key.Up:
                    if (0 < ballSpeed.Y) ballSpeed.Y *= -1;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// ButtonBall->Clickイベント
        /// </summary>
        private void ButtonBall_Click(object sender, RoutedEventArgs e)
        {
            // はじめる操作を無効化
            ButtonBall.IsEnabled = false;

            // パラメータ初期化
            speedMax = 10.0;
            speedRate = 1.08;
            SetScore(null);
            SetLevel(1);
            SetDropCount(0);


            // ブロック表示
            foreach (var block in blockList) block.Control.Visibility = Visibility.Visible;

            // ボール初期化
            InitializeBall();
        }
    }
}

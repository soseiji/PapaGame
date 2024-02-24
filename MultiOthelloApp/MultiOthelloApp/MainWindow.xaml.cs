using System;
using System.Collections.Generic;
using System.Linq;
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
using static MultiOthelloApp.OthelloAiService;
using static MultiOthelloApp.Stone;

namespace MultiOthelloApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            UpdateBoard();

            ComboBoxPlayerCount.ItemsSource = othelloService.SupportPlayerCount;
            ComboBoxPlayerCount.SelectedValue = othelloService.DefaultPlayerCount;

            ComboBoxBoardSize.ItemsSource = othelloService.SupportBoardSize;
            ComboBoxBoardSize.SelectedValue = othelloService.DefaultBoardSize;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            // 指定サイズの盤に初期化
            if (ItemsControl1.ItemsPanel.LoadContent() is not System.Windows.Controls.Primitives.UniformGrid uniformGrid) return;
            var playerCount = (int)ComboBoxPlayerCount.SelectedValue;
            var boardSize = (int)ComboBoxBoardSize.SelectedValue;
            uniformGrid.Rows = boardSize;
            uniformGrid.Columns = boardSize;
            othelloService.Start(boardSize, playerCount);
            UpdateBoard();

            // 無効化
            CheckBoxPapa.IsEnabled = false;
            ComboBoxPlayerCount.IsEnabled = false;
            ComboBoxBoardSize.IsEnabled = false;
            ButtonStart.Visibility = Visibility.Collapsed;
            ButtonEnd.Visibility = Visibility.Visible;
            ButtonPass.Visibility = Visibility.Visible;

        }

        private void ButtonEnd_Click(object sender, RoutedEventArgs e)
        {
            // 有効化
            CheckBoxPapa.IsEnabled = true;
            ComboBoxPlayerCount.IsEnabled = true;
            ComboBoxBoardSize.IsEnabled = true;
            ComboBoxBoardSize.IsEnabled = true;
            ButtonStart.Visibility = Visibility.Visible;
            ButtonEnd.Visibility = Visibility.Collapsed;
            ButtonPass.Visibility = Visibility.Collapsed;

            // 初期化
            var playerCount = 0;
            var boardSize = 0;
            othelloService.Start(boardSize, playerCount);
            UpdateBoard();
        }

        private async void ButtonPass_Click(object sender, RoutedEventArgs e)
        {
            // 連続操作の抑止
            ButtonPass.IsEnabled = false;

            // 交代
            othelloService.Pass();
            UpdateBoard();

            // 自動反転モード
            await RunAutoReverse(null);

            ButtonPass.IsEnabled = true;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.DataContext is not Stone putStone) return;

            // 反転できる石を取得
            var reverseStoneList = othelloService.GetReverseStoneList(putStone);
            if (!reverseStoneList.Any()) return;

            // 反転実施
            othelloService.Reverse(putStone, reverseStoneList);
            UpdateBoard();

            // 自動反転モード
            await RunAutoReverse(putStone);
        }

        private async Task RunAutoReverse(Stone? putStone)
        {
            var isAuto = CheckBoxPapa.IsChecked ?? false;
            if (!isAuto) return;

            if (getPapaColorType() != othelloService.MyColor) return;

            if (!othelloService.StoneList.Any()) return;
            if (othelloService.NoneCount == 0) return;

            ItemsControl1.IsEnabled = false;
            await Task.Delay(1500);
            var isPutted = false;
            await Task.Run(() =>
            {
                if (!othelloService.BoardSizeRange.Any()) return;
                var boardSize = othelloService.BoardSizeRange.Max();

                // 空きマス情報の生成
                var stoneDictionary = new Dictionary<Stone, ReverseStoneInfo>();
                foreach (var stone in othelloService.StoneList)
                {
                    var reverseStoneList = othelloService.GetReverseStoneList(stone);
                    if (!reverseStoneList.Any()) continue;
                    stoneDictionary[stone] = new ReverseStoneInfo(stone, reverseStoneList, boardSize);
                }

                // 最適マスを反転
                var stoneInfo = OthelloAiService.GetReverseStoneInfo(
                    stoneDictionary,
                    boardSize,
                    isPuttedLeftTop: othelloService.IsPuttedSelf(1, 1),
                    isPuttedLeftBottom: othelloService.IsPuttedSelf(boardSize, 1),
                    isPuttedRightTop: othelloService.IsPuttedSelf(1, boardSize),
                    isPuttedRightBottom: othelloService.IsPuttedSelf(boardSize, boardSize));
                if (stoneInfo == null) return;

                // ここで試し打ちをしたい。リトライ処理を入れる。
                // 打つことで、自分がすべて取られる場合、リトライ処理を入れる。



                othelloService.Reverse(stoneInfo.PutStone, stoneInfo.ReverseStoneList);
                isPutted = true;
            });

            if (!isPutted)
            {
                othelloService.Pass();
                MessageBox.Show("ぱすです ( ﾟДﾟ)");
            }
            UpdateBoard();
            ItemsControl1.IsEnabled = true;

            ColorType getPapaColorType() {
                return othelloService.PlayerCount switch
                {
                    3 => ColorType.Red,
                    4 => ColorType.Blue,
                    _ => ColorType.White,
                };
            }
        }

        private void UpdateBoard()
        {
            ItemsControl1.ItemsSource = null;
            ItemsControl1.ItemsSource = othelloService.StoneList;

            LabelBlack.Content = othelloService.BlackCount;
            LabelWhite.Content = othelloService.WhiteCount;
            LabelRed.Content = othelloService.RedCount;
            LabelBlue.Content = othelloService.BlueCount;

            LabelBlackTurn.Visibility = othelloService.MyColor == Stone.ColorType.Black ? Visibility.Visible : Visibility.Collapsed;
            LabelWhiteTurn.Visibility = othelloService.MyColor == Stone.ColorType.White ? Visibility.Visible : Visibility.Collapsed;
            LabelRedTurn.Visibility = othelloService.MyColor == Stone.ColorType.Red ? Visibility.Visible : Visibility.Collapsed;
            LabelBlueTurn.Visibility = othelloService.MyColor == Stone.ColorType.Blue ? Visibility.Visible : Visibility.Collapsed;

            ItemsControl1.BorderBrush = getColor(othelloService.MyColor);
            if (othelloService.PlayerCount == 0) ItemsControl1.BorderBrush = getColor(ColorType.None);

            othelloService.SetCanReverse();

            if (othelloService.Winners.Any())
            {
                var winners = othelloService.Winners.Select(x => x.ToString()).ToList();
                var name = string.Join(", ", winners);
                MessageBox.Show($"{name} の勝ち！ (^_^)/ ");
            }

            LinearGradientBrush getColor(Stone.ColorType colorType)
            {
                if (colorType == Stone.ColorType.Black) return new LinearGradientBrush(Colors.Black, Colors.Black, new Point(0.5, 1), new Point(0.5, 0));
                if (colorType == Stone.ColorType.White) return new LinearGradientBrush(Colors.WhiteSmoke, Colors.WhiteSmoke, new Point(0.5, 1), new Point(0.5, 0));
                if (colorType == Stone.ColorType.Red) return new LinearGradientBrush(Colors.DarkRed, Colors.DarkRed, new Point(0.5, 1), new Point(0.5, 0));
                if (colorType == Stone.ColorType.Blue) return new LinearGradientBrush(Colors.DarkBlue, Colors.DarkBlue, new Point(0.5, 1), new Point(0.5, 0));
                return new LinearGradientBrush(Colors.Transparent, Colors.Transparent, new Point(0.5, 1), new Point(0.5, 0));
            }

        }

        private readonly OthelloService othelloService = new();

    }
}

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
using static OthelloApp.OthelloAiService;

namespace OthelloApp
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
            ComboBoxBoardSize.ItemsSource = othelloService.SupportBoardSize;
            ComboBoxBoardSize.SelectedValue = othelloService.DefaultBoardSize;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            // 指定サイズの盤に初期化
            if (ItemsControl1.ItemsPanel.LoadContent() is not System.Windows.Controls.Primitives.UniformGrid uniformGrid) return;
            var boardSize = (int)ComboBoxBoardSize.SelectedValue;
            uniformGrid.Rows = boardSize;
            uniformGrid.Columns = boardSize;
            othelloService.Start(boardSize);
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

            var stoneList = othelloService.StoneList.Where(x => x.Color == Stone.ColorType.None).ToList();
            if (!stoneList.Any()) return;

            ItemsControl1.IsEnabled = false;
            await Task.Delay(1500);
            var isPutted = false;
            await Task.Run(() =>
            {
                var boardSize = othelloService.BoardSizeRange.Max();

                // 空きマス情報の生成
                var stoneDictionary = new Dictionary<Stone, ReverseStoneInfo>();
                foreach (var stone in stoneList)
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
        }

        private void UpdateBoard()
        {
            ItemsControl1.ItemsSource = null;
            ItemsControl1.ItemsSource = othelloService.StoneList;

            LabelBlack.Content = othelloService.BlackCount;
            LabelWhite.Content = othelloService.WhiteCount;
            LabelBlackTurn.Visibility = othelloService.MyColor == Stone.ColorType.Black ? Visibility.Visible : Visibility.Collapsed;
            LabelWhiteTurn.Visibility = othelloService.MyColor == Stone.ColorType.White ? Visibility.Visible : Visibility.Collapsed;

            if (othelloService.Winner.HasValue)
            {
                var name = othelloService.Winner.Value == Stone.ColorType.Black ? "くろ" : "しろ";
                MessageBox.Show($"{othelloService.Winner} のかち！  (^_^)/ ");
            }
        }

        private readonly OthelloService othelloService = new();
    }
}

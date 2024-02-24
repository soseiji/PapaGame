using GomokuApp.Dto;
using GomokuApp.Service;
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

namespace GomokuApp
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

            ComboBoxThreshold.ItemsSource = gomokuService.SupportThreshold;
            ComboBoxThreshold.SelectedValue = gomokuService.DefaultThreshold;

            ComboBoxBoardSize.ItemsSource = gomokuService.SupportBoardSize;
            ComboBoxBoardSize.SelectedValue = gomokuService.DefaultBoardSize;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            DebugLog("---------- Start ----------");

            UniformGridColumn.Visibility = Visibility.Visible;
            ButtonStart.Visibility = Visibility.Collapsed;
            ButtonEnd.Visibility = Visibility.Visible;
            ButtonSet.Visibility = Visibility.Visible;
            CheckBoxPapa.IsEnabled = false;
            ComboBoxThreshold.IsEnabled = false;
            ComboBoxBoardSize.IsEnabled = false;

            // 指定サイズの盤に初期化
            if (ItemsControl1.ItemsPanel.LoadContent() is not System.Windows.Controls.Primitives.UniformGrid uniformGrid) return;
            var threshold = (int)ComboBoxThreshold.SelectedValue;
            var boardSize = (int)ComboBoxBoardSize.SelectedValue;
            uniformGrid.Rows = boardSize;
            uniformGrid.Columns = boardSize;

            // 盤の上に選択用ボタンを配置
            UniformGridColumn.Children.Clear();
            foreach (var colNum in Enumerable.Range(1, boardSize))
            {
                var radioButton = new RadioButton()
                {
                    Tag = colNum
                };
                radioButton.Click += (sender, e) =>
                {
                    if (sender is not RadioButton clickedRadioButton) return;
                    var colNum = (int)clickedRadioButton.Tag;
                    gomokuService.SelectColumn(colNum);
                    ItemsControl1.ItemsSource = null;
                    ItemsControl1.ItemsSource = gomokuService.StoneList;
                };
                UniformGridColumn.Children.Add(radioButton);
            }

            // 五目サービス開始
            gomokuService.Start(threshold, boardSize);

            UpdateBoard();
        }

        private void ButtonEnd_Click(object sender, RoutedEventArgs e)
        {
            UniformGridColumn.Visibility = Visibility.Collapsed;
            ButtonStart.Visibility = Visibility.Visible;
            ButtonEnd.Visibility = Visibility.Collapsed;
            ButtonSet.Visibility = Visibility.Collapsed;
            CheckBoxPapa.IsEnabled = true;
            ComboBoxThreshold.IsEnabled = true;
            ComboBoxBoardSize.IsEnabled = true;

            gomokuService.Start(threshold: 0, boardSize: 0);

            UpdateBoard();
        }

        private async void ButtonSet_Click(object sender, RoutedEventArgs e)
        {
            // 連続操作の抑止
            ButtonSet.IsEnabled = false;
            UniformGridColumn.IsEnabled = false;
            try
            {
                // 選択した石をおとす
                var colNum = selectedColNum();
                if (!colNum.HasValue) return;
                if (!gomokuService.CanSet(colNum.Value)) 
                {
                    MessageBox.Show($"おけません  ( ﾟДﾟ)");
                    return;
                }
                gomokuService.Set(colNum.Value);
                UpdateBoard();

                // パパモード
                var isAuto = CheckBoxPapa.IsChecked ?? false;
                if (!isAuto) return;
                if (gomokuService.Winner.HasValue) return;  // 勝敗がついてたら終わる
                await Task.Delay(500);
                await Task.Run(() =>
                {
                    gomokuService.Set(new GomokuPapaService().GetGoodColumnNum(
                        gomokuService.StoneList,
                        gomokuService.Threshold,
                        gomokuService.BoardSizeRange.Max()));
                });
                UpdateBoard();
            }
            finally
            {
                ButtonSet.IsEnabled = true;
                UniformGridColumn.IsEnabled = true;
                if (gomokuService.Winner.HasValue)
                {
                    ButtonSet.Visibility = Visibility.Collapsed;   // 勝利した場合、「おとす」ボタンは非表示。「はじめる」でゲーム再開したら表示
                }
            }

            int? selectedColNum()
            {
                foreach (var child in UniformGridColumn.Children)
                {
                    if (child is not RadioButton radioButton) return null;
                    if (radioButton.IsChecked.Value) return radioButton.Tag as int?;
                }
                return null;
            }
        }

        private void UpdateBoard()
        {
            TextBoxLog.Text = log;

            ItemsControl1.ItemsSource = null;
            ItemsControl1.ItemsSource = gomokuService.StoneList;

            foreach (var child in UniformGridColumn.Children)
            {
                if (child is not RadioButton radioButton) return;
                radioButton.Background = gomokuService.MyColor == Stone.ColorType.Black ? 
                    new LinearGradientBrush(Colors.Black, Colors.Gray, new Point(0.5, 1), new Point(0.5, 0)) :
                    new LinearGradientBrush(Colors.LightGray, Colors.White, new Point(0.5, 1), new Point(0.5, 0));
                radioButton.IsChecked = false;
            }
            
            if (gomokuService.Winner.HasValue)
            {
                var name = gomokuService.Winner.Value == Stone.ColorType.Black ? "くろ" : "しろ";
                MessageBox.Show($"{gomokuService.Winner} のかち！  (^_^)/ ");
            }
        }

        private readonly GomokuService gomokuService = new();

        private static string log = "";
        public static void DebugLog(string newLog)
        {
            // ログが必要ならコメントアウト
            //log = string.IsNullOrEmpty(log) ? newLog : log + Environment.NewLine + newLog;
        }
    }
}

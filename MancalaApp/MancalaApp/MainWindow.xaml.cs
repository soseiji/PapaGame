using MancalaApp.Dto;
using MancalaApp.Service;
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
using static MancalaApp.Service.MancalaService;

namespace MancalaApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ComboBoxSize.ItemsSource = mancalaService.SupportSizeList;
            ComboBoxSize.SelectedValue = mancalaService.DefaultSize;

            ComboBoxStoneCount.ItemsSource = mancalaService.SupportDefaultStoneList;
            ComboBoxStoneCount.SelectedValue = mancalaService.DefaultStoneCount;

            ComboBoxFinishRule.ItemsSource = mancalaService.SupportFinishRuleList;
            ComboBoxFinishRule.SelectedValue = mancalaService.DefaultFinishRule;

            mancalaService.Initialize();
            Update();
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            ButtonStart.Visibility = Visibility.Collapsed;
            ButtonEnd.Visibility = Visibility.Visible;
            StackPanelOption.IsEnabled = false;

            mancalaService.Initialize(
                size: (int)ComboBoxSize.SelectedValue, 
                defaultStoneCount: (int)ComboBoxStoneCount.SelectedValue,
                finishRule: (FinishRule)ComboBoxFinishRule.SelectedValue);
            Update();
        }

        private void ButtonEnd_Click(object sender, RoutedEventArgs e)
        {
            ButtonStart.Visibility = Visibility.Visible;
            ButtonEnd.Visibility = Visibility.Collapsed;
            StackPanelOption.IsEnabled = true;
        }

        private async void ButtonPlayer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (button.DataContext is not Pocket pocket) return;
            if (pocket.StoneCount == 0) return;

            var isOnceAgain = mancalaService.Move(pocket.Player, pocket.Position);
            if (Update()) return;
            if (isOnceAgain) return;

            // パパモード
            var isAuto = CheckBoxPapa.IsChecked ?? false;
            if (!isAuto) return;
            var isOnceAgain_Papa = false;
            foreach (var retry in Enumerable.Range(0, 10))
            {
                await Task.Delay(1500);
                await Task.Run(() =>
                {
                    var goodPosition = new MancalaPapaService().GetGoodPocketPosition(mancalaService.PocketList);
                    isOnceAgain_Papa = mancalaService.Move(Pocket.PlayerType.Player2, goodPosition);
                });
                if (Update()) return;

                if (!isOnceAgain_Papa) break;
                await Task.Delay(1000); // 打つの早いとわけわからんので、遅らせる
            }

        }


        private bool Update()
        {
            ItemsControlPlayer1.ItemsSource = mancalaService.Player1AreaList;
            ItemsControlPlayer2.ItemsSource = mancalaService.Player2AreaList;

            PocketUserControlPlayer1Goal.Pocket = null; // 描画更新のため
            PocketUserControlPlayer2Goal.Pocket = null; // 描画更新のため
            PocketUserControlPlayer1Goal.Pocket = mancalaService.Player1Goal;
            PocketUserControlPlayer2Goal.Pocket = mancalaService.Player2Goal;

            // 誤操作防止用
            GridPlayer1Cover.Visibility = Visibility.Visible;
            GridPlayer2Cover.Visibility = Visibility.Visible;
            if (mancalaService.NowPlayer == Pocket.PlayerType.Player1) GridPlayer1Cover.Visibility = Visibility.Collapsed;
            if (mancalaService.NowPlayer == Pocket.PlayerType.Player2) GridPlayer2Cover.Visibility = Visibility.Collapsed;
            if (CheckBoxPapa.IsChecked ?? false) GridPlayer2Cover.Visibility = Visibility.Visible;  // パパモードの場合、ユーザー操作を無効化しとく

            // パパモード演出用
            GridPlayer2CoverPapa.Visibility = ((CheckBoxPapa.IsChecked ?? false) && (mancalaService.NowPlayer == Pocket.PlayerType.Player2)) ?
                Visibility.Visible :
                Visibility.Collapsed;
            GridPlayer1CoverPapa.Visibility = ((CheckBoxPapa.IsChecked ?? false) && (mancalaService.NowPlayer == Pocket.PlayerType.Player1)) ?
                Visibility.Visible :
                Visibility.Collapsed;

            var isFinish = mancalaService.IsFinish(out var winPlayer);
            if (isFinish)
            {
                if (winPlayer == Pocket.PlayerType.Player1) MessageBox.Show("あか色 のかち (^^)/");
                if (winPlayer == Pocket.PlayerType.Player2) MessageBox.Show("みどり色 のかち (^^)/");

                GridPlayer1Cover.Visibility = Visibility.Visible;
                GridPlayer2Cover.Visibility = Visibility.Visible;
                GridPlayer1CoverPapa.Visibility = Visibility.Collapsed;
                GridPlayer2CoverPapa.Visibility = Visibility.Collapsed;
            }

            return isFinish;
        }


        private readonly MancalaService mancalaService = new();


    }
}

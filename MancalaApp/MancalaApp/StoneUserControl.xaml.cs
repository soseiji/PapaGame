using MancalaApp.Dto;
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

namespace MancalaApp
{
    /// <summary>
    /// PocketUserControl.xaml の相互作用ロジック
    /// </summary>
    public partial class PocketUserControl : UserControl
    {
        public PocketUserControl()
        {
            InitializeComponent();
        }

        public Pocket Pocket
        {
            get { return (Pocket)GetValue(PocketProperty); }
            set { SetValue(PocketProperty, value); }
        }

        public static readonly DependencyProperty PocketProperty = DependencyProperty.Register(
            "Pocket",
            typeof(Pocket),
            typeof(PocketUserControl),
            new PropertyMetadata(null, OnPocketChanged));

        private static void OnPocketChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not PocketUserControl control) return;
            if (control.Pocket == null) return;

            control.LabelCount.Content = control.Pocket.StoneCount;
            control.UniformGridPocket.Children.Clear();
            foreach (int index in Enumerable.Range(0, control.Pocket.StoneCount))
            {
                var stone = new Ellipse();
                if (index < control.Pocket.AddedStoneCount) 
                {
                    // 特別色。配置したことがわかるように。
                    stone.Fill = new LinearGradientBrush(Colors.OrangeRed, Colors.Yellow, new Point(0.5,1), new Point(0.5, 0));
                }
                control.UniformGridPocket.Children.Add(stone);
            }
         }
    }
}

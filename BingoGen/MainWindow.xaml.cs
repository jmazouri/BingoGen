using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using BingoGen.Annotations;
using Microsoft.Win32;

namespace BingoGen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private float _boxSize;
        public float BoxSize
        {
            get { return _boxSize; }
            set
            {
                if (value.Equals(_boxSize)) return;
                _boxSize = value;
                OnPropertyChanged();
            }
        }

        public int Seed
        {
            get { return Util.RandSeed; }
            set { Util.RandSeed = value; OnPropertyChanged(); ParseAndRender(); }
        }

        private double _fontSize;
        public double BoxFontSize
        {
            get { return _fontSize; }
            set
            {
                if (value.Equals(_fontSize)) return;
                _fontSize = value;
                OnPropertyChanged();
            }
        }

        public float MaxBoxWidth = 5;
        

        public MainWindow()
        {
            BoxSize = 128;
            BoxFontSize = 16;

            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ParseAndRender();
        }

        void ParseAndRender()
        {
            string[] inputs = InputBox.Text.Split(Environment.NewLine.ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            inputs.Shuffle();

            RenderToCanvas(inputs.ToList());
        }

        void RenderToCanvas(List<string> boardItems)
        {
            if (MainCanvas == null || !MainCanvas.IsInitialized) { return; }

            MainCanvas.Children.Clear();

            float xPos = 0;
            float yPos = 0;

            for (int i =0; i < (boardItems.Count > 24 ? 24 : boardItems.Count); i++)
            {
                string line = boardItems[i];

                TextBlock l = new TextBlock();

                l.TextAlignment = TextAlignment.Center;
                l.VerticalAlignment = VerticalAlignment.Center;
                
                l.Text = line;
                
                l.TextWrapping = TextWrapping.Wrap;

                l.Padding = new Thickness(4);

                l.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                l.FontSize = BoxFontSize;

                Border bord = new Border();
                bord.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                bord.BorderThickness = new Thickness(1);

                bord.MaxHeight = BoxSize;
                bord.MaxWidth = BoxSize;
                bord.Width = BoxSize;
                bord.Height = BoxSize;

                bord.Background = new SolidColorBrush(Util.generateRandomColor(Color.FromRgb(0, 0, 0), line));


                bord.Child = l;

                Canvas.SetLeft(bord, xPos);
                Canvas.SetTop(bord, yPos);

                xPos += BoxSize;

                if (xPos / BoxSize >= MaxBoxWidth)
                {
                    xPos = 0;
                    yPos += BoxSize;
                }

                MainCanvas.Children.Add(bord);

                if (Math.Abs(xPos - (Math.Ceiling(MaxBoxWidth / 2) * BoxSize)) < 5 &&
                    Math.Abs(yPos - (Math.Floor(MaxBoxWidth / 2) * BoxSize)) < 5)
                {
                    l.Text = "FREE";
                    l.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    l.FontWeight = FontWeights.Bold;
                    l.FontSize = BoxFontSize*1.5f;
                    bord.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

                    i--;
                }
            }

            TextBlock seedWatermark = new TextBlock();

            seedWatermark.Text = "#"+Seed;

            seedWatermark.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            seedWatermark.FontSize = BoxFontSize;

            Canvas.SetLeft(seedWatermark, 4);
            Canvas.SetTop(seedWatermark, yPos - 24);

            MainCanvas.Children.Add(seedWatermark);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ParseAndRender();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ParseAndRender();
        }

        private void Export_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Make sure to resize the window to fit the entire board before clicking OK!", "HEY",
                    MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "PNG Image|*.png";

                var dialogResult = dialog.ShowDialog();

                if (dialogResult.HasValue && dialogResult.Value)
                {
                    RenderTargetBitmap rtb = new RenderTargetBitmap((int)MainCanvas.RenderSize.Width, (int)MainCanvas.RenderSize.Height + 100, 96d, 96d, PixelFormats.Default);

                    VisualBrush sourceBrush = new VisualBrush(MainCanvas);
                    DrawingVisual drawingVisual = new DrawingVisual();
                    DrawingContext drawingContext = drawingVisual.RenderOpen();
                    using (drawingContext)
                    {
                        drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0),
                              new Point((int)MainCanvas.RenderSize.Width, (int)MainCanvas.RenderSize.Height + 100)));
                    }

                    rtb.Render(drawingVisual);

                    FileStream fs = new FileStream(dialog.FileName, FileMode.Create);

                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(rtb));

                    encoder.Save(fs);
                    fs.Close();
                }
            }
        }

        private void RandomSeed_OnClick(object sender, RoutedEventArgs e)
        {
            Random r = new Random();
            Seed = r.Next(100000);
        }
    }
}

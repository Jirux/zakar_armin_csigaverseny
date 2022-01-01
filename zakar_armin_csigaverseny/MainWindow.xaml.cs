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
using System.Windows.Threading;

namespace zakar_armin_csigaverseny
{
    public partial class MainWindow : Window
    {
        private List<Csiga> participants;
        private DispatcherTimer timer = new DispatcherTimer();
        private int currentPlacement = 1;
        private double celVonalX;

        public MainWindow()
        {
            InitializeComponent();
            ujBajnoksagGomb.Click += new RoutedEventHandler(visszaGomb_Click);
            participants = new List<Csiga>();
            Random random = new Random();
            participants.Add(new Csiga("csiga", 0, random, new DisplayContents(foAblak, csiga1, helyezes1, sav1)));
            participants.Add(new Csiga("rammus", -17, random, new DisplayContents(foAblak, csiga2, helyezes2, sav2)));
            participants.Add(new Csiga("Armino", -8, random, new DisplayContents(foAblak, csiga3, helyezes3, sav3)));
            timer.Interval = TimeSpan.FromSeconds(0.04);
            timer.Tick += new EventHandler(timer_Tick);
            visszaGomb.IsEnabled = false;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            celVonalX = celVonal.TransformToAncestor(foAblak).Transform(new Point(0.0, 0.0)).X;
            participants.ForEach(v => v.tick(celVonalX, onFinish));
            if (currentPlacement != 4)
                return;
            timer.Stop();
            participants.ForEach(v => v.reset());
            bajnoksagAllasa.Content = CreateResults();
            currentPlacement = 1;
            visszaGomb.IsEnabled = true;
            ujBajnoksagGomb.IsEnabled = true;
        }

        private int onFinish(Csiga csiga)
        {
            return currentPlacement++;
        }

        private string CreateResults()
        {
            string str = "Hely\tNév\t\t1.\t2.\t3.\tPont\n";
            List<Csiga> list = participants.OrderByDescending(x => x.Pont).ThenByDescending(x => x.Helyezes[1]).ThenByDescending(x => x.Helyezes[2]).ToList();
            for (int index = 0; index < participants.Count; ++index)
                str += string.Format("{0}.\t{1}\t\t{2}\t{3}\t{4}\t{5} p\n", index + 1, list[index].Nev, list[index].Helyezes[1], list[index].Helyezes[2], list[index].Helyezes[3], list[index].Pont);
            return str;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Start();
            startButton.IsEnabled = false;
            ujBajnoksagGomb.IsEnabled = false;
        }

        private void visszaGomb_Click(object sender, RoutedEventArgs e)
        {
            csiga1.Margin = new Thickness(36.0, 50.0, 0.0, 0.0);
            csiga2.Margin = new Thickness(20.0, 250.0, 0.0, 0.0);
            csiga3.Margin = new Thickness(50.0, 420.0, 0.0, 0.0);
            helyezes1.Content = "";
            helyezes2.Content = "";
            helyezes3.Content = "";
            startButton.IsEnabled = true;
            visszaGomb.IsEnabled = false;
            sav1.Fill = null;
            sav2.Fill = null;
            sav3.Fill = null;
        }

        private void ujBajnoksagGomb_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(CreateResults(), "A bajnokság végeredménye");
            foreach (Csiga csiga in participants)
            {
                for (int index = 0; index < csiga.Helyezes.Count<int>(); index++)
                    csiga.Helyezes[index] = 0;
            }
            bajnoksagAllasa.Content = "";
        }

        private class Csiga
        {
            private SolidColorBrush[] medals = new SolidColorBrush[3]
            {
                new SolidColorBrush(Color.FromRgb(212,175,55)),
                new SolidColorBrush(Color.FromRgb(170,169,173)),
                new SolidColorBrush(Color.FromRgb(143, 92, 60))
            };
            public string Nev { get; }
            public int[] Helyezes { get; set; }
            private int[] pontok = new int[4] { 0, 3, 2, 1 };
            int imageOffset;
            private int speed;
            private Random random;
            private DisplayContents data;

            public Csiga(string csigaNev, int offset, Random random, DisplayContents display)
            {
                this.Nev = csigaNev;
                this.imageOffset = offset;
                this.random = random;
                this.data = display;
                this.Helyezes = new int[4];
                this.speed = random.Next(2, 10);
            }

            public void tick(double finishLine, Func<Csiga, int> onFinish)
            {
                double x = data.image.TransformToAncestor(data.ancestor).Transform(new Point(0.0, 0.0)).X;
                double width = data.image.Width + imageOffset;
                if (x + width < finishLine)
                {
                    double left = x + speed;
                    if (left + width > finishLine)
                        left = finishLine - width;
                    var margin = data.image.Margin;
                    data.image.Margin = new Thickness(left, margin.Top, margin.Right, margin.Bottom);
                } else
                {
                    if (data.placeLabel.Content == "")
                    {
                        int placement = onFinish(this);
                        ++Helyezes[placement];
                        data.placeLabel.Content = placement;
                        data.lineRectangle.Fill = medals[placement - 1];
                    }
                }
            }

            public void reset()
            {
                speed = random.Next(2, 10);
            }

            public int Pont => Helyezes[1] * pontok[1] + Helyezes[2] * pontok[2] + Helyezes[3] * pontok[3];
        }

        private class DisplayContents
        {
            public Visual ancestor { get; }
            public Image image { get; }
            public Label placeLabel { get; }
            public Rectangle lineRectangle { get; }

            public DisplayContents(Visual ancestor, Image image, Label placeLabel, Rectangle lineRectangle)
            {
                this.ancestor = ancestor;
                this.image = image;
                this.placeLabel = placeLabel;
                this.lineRectangle = lineRectangle;
            }

        }
    }
}

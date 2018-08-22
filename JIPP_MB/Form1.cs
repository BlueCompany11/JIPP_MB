using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JIPP_MB
{
    public partial class Form1 : Form
    {
        public delegate void SaveToDb();
        public event SaveToDb DbUpdateFinished;
        delegate void TimerStop();
        event TimerStop TimerStopHandler;
        int ticks;
        System.Timers.Timer timer;
        bool result;
        int dx = 5;
        int dy = 50;
        public Form1()
        {
            InitializeComponent();
            //dodaje do eventu metode do wykonania
            DbUpdateFinished += Form1_DbUpdateFinished;
            //to samo co wyzej tylko zamiast deklarowac ta metode korzystam z wyrazenia lambda
            //to () =>{} oznacza, ze dopisuje do TimerStopHandler metode ktora przyjmuje 0 zmiennych i nic nie zwraca czyli zgodnie
            //z delegata TimerStop ktora jest void i nic nie przyjmuje
            TimerStopHandler += (() =>
            {   //metoda ta resetuje ustawienia 
                ticks = 0;
                timer.Stop();
                dx = 5;
                dy = 50;
            });
            //ustawiam timer
            timer = new System.Timers.Timer(200);
            timer.AutoReset = true;
            //dodaje do timera metode ktora sie bedzie wykonywac za kazdym ticknieciem
            timer.Elapsed += Timer_Elapsed;
            //poki co timer jeszcze nie ejst uruchominoy
        }

        private void Form1_DbUpdateFinished()
        {
            //metoda ktora jest przypisana do eventu DbUpdateFinished
            //gdy sie powiedzie modyfikwacja bazy danych to kolor kontrolki na dole zmieni sie na odpowiedni
            if (result == true)
                pictureBoxAnimation.BackColor = Color.AliceBlue;
            else
                pictureBoxAnimation.BackColor = Color.Bisque;
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //zadaniem metody dopisanej do timera jest wywolywanie metody ktora rysuje
            DrawAnimation();
            ticks++;
            if (ticks > 5)
            {
                //wywoluje zdarzenie TimerStopHandler gdy ilosc tickniec jest wieksza od 5
                //czyli wywola sie ta lambda ktora jest w konstuktorze Form1
                //czyli zresetuje wartosci i zatrzyma timer
                TimerStopHandler();
            }
            
        }
        private void DrawAnimation()
        {
            //rysowanie animacji - nie ma co tu tlumaczyc kod sztampowy z oficjalnej strony microsoft
            //podrasowany by spelnial moje zalozenia
            Bitmap bmp1 = new Bitmap(pictureBoxAnimation.Width, pictureBoxAnimation.Height);
            Graphics graphics = Graphics.FromImage(bmp1);
            string textToShow = "Zmiana poprawna";
            if (result == false)
            {
                textToShow = "Zmiana niepoprawna";
            }
            //przesuwanie napisu o dx poziomo i dy w pionie
            graphics.TranslateTransform(dx, dy);
            //dzialanie modulo bo moglo wyjsc poza ekran i by nie bylo widac zmian, a tak to zacznie od poczatku jak wyjdzie poza kontrolke
            dx = (dx + 70) % pictureBoxAnimation.Width;
            //raz do gory raz do dolu
            dy *= (-1);
            System.Drawing.Font drawFont = new System.Drawing.Font("Arial", 16);
            System.Drawing.SolidBrush drawBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            System.Drawing.StringFormat drawFormat = new System.Drawing.StringFormat();
            graphics.DrawString(textToShow, drawFont, drawBrush, 10, 100, drawFormat);
            pictureBoxAnimation.BackgroundImage = bmp1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'studentsDbDataSet.Students' table. You can move, or remove it, as needed.
            this.studentsTableAdapter.Fill(this.studentsDbDataSet.Students);
        }

        private void dataGridViewStudents_KeyDown(object sender, KeyEventArgs e)
        {
            //jesli przyciskiem ktory spowodowal zdarzenie keydown jest enter to:
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    //update bazy - zapisze sie to co sie znajduje w tej kontrolce w trakcie dzialania programu
                    studentsTableAdapter.Update(studentsDbDataSet.Students);
                    result = true;
                    //wywolanie zdarzenia 
                    DbUpdateFinished();
                    
                    //uruchamiamy timera
                    timer.Enabled = true;
                }
                catch (Exception)
                {
                    //w to praktycznie nigdy nie wejdziemy bo kontrolka sama zabezpiecza nas przed wprowadzeniem zlych danych
                    result = false;
                    DbUpdateFinished();
                    timer.Enabled = true;
                }
                
            }
        }

    }
}

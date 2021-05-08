using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EdytorZdjec_v1
{
    public partial class Form1 : Form
    {
        private int szer=0, wys=0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pictureBox1.Load(openFileDialog1.FileName);
                szer = pictureBox1.Image.Width;
                wys = pictureBox1.Image.Height;
                pictureBox2.Image = new Bitmap(szer, wys);
            }
        }

//Wyrównanie histogramu
        private void button3_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            Bitmap b1 = (Bitmap)pictureBox1.Image;
            Bitmap b2 = (Bitmap)pictureBox2.Image;
            Color p1, p2;
            int[] red = new int[256];
            int[] green = new int[256];
            int[] blue = new int[256];

            for (int x = 0; x < szer; x++)
            {
                for (int y = 0; y < wys; y++)
                {
                    Color p = ((Bitmap)pictureBox1.Image).GetPixel(x, y);
                    red[p.R]++;
                    green[p.G]++;
                    blue[p.B]++;
                }
            }

            //Tablice LUT dla składowych
            int[] LUTred = calcLUT(red, szer * wys);
            int[] LUTgreen = calcLUT(green, szer * wys);
            int[] LUTblue = calcLUT(blue, szer * wys);

            //Przetwórz obraz
            for (int x = 0; x < szer; x++)
            {
                for (int y = 0; y < wys; y++)
                {
                    p1 = b1.GetPixel(x, y);
                    p2 = Color.FromArgb(LUTred[p1.R], LUTgreen[p1.G], LUTblue[p1.B]);
                    b2.SetPixel(x, y, p2);
                }
            }
            pictureBox2.Image = b2;

            pictureBox2.Invalidate();

            Cursor = Cursors.Default;
        }

        private int[] calcLUT(int[] values, int size)
        {
            //(poszukaj wartości minimalnej - czyli pierwszej niezerowej wartosci dystrybuanty)
            double minValue = 0;
            int[] result = new int[256];
            double sum = 0;

            for (int i = 0; i < 256; i++)
            {
                if (values[i] != 0)
                {
                    minValue = values[i];
                    break;
                }
            }

            for (int i = 0; i < 256; i++)
            {
                sum += values[i];
                result[i] = (int)(((sum - minValue) / (size - minValue)) * 255.0);
            }

            return result;
        }

        //Filtr Uśredniający
        private void button4_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            Bitmap b1 = (Bitmap)pictureBox1.Image;
            Bitmap b2 = (Bitmap)pictureBox2.Image;

            int[,] maska = new int[3, 3];
            maska[0, 0] = 1;
            maska[0, 1] = 1;
            maska[0, 2] = 1;
            maska[1, 0] = 1;
            maska[1, 1] = 1;
            maska[1, 2] = 1;
            maska[2, 0] = 1;
            maska[2, 1] = 1;
            maska[2, 2] = 1;

            int norm = 0;
            for (int i = 0; i < 3; i++)    //normowanie maski(czyli dzielenie przez sumę elementów)
                for (int j = 0; j < 3; j++)
                    norm += maska[i, j];

            int R, G, B;
            Color k;

            for (int i = 1; i < szer - 1; i++) //(od 1 bo piksele brzegowe wyjdą poza obraz)
            {
                for (int j = 1; j < wys - 1; j++)
                {
                    R = B = G = 0;

                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            k = b1.GetPixel(i + x - 1, j + y - 1); //piksel lewy
                            R += k.R * maska[x, y];
                            G += k.G * maska[x, y];
                            B += k.B * maska[x, y];
                        }
                    }

                    if (norm != 0)
                    {
                        R /= norm;
                        G /= norm;
                        B /= norm;
                    }

                    if (R < 0) R = 0;
                    if (R > 255) R = 255;

                    if (G < 0) G = 0;
                    if (G > 255) G = 255;

                    if (B < 0) B = 0;
                    if (B > 255) B = 255;


                    b2.SetPixel(i, j, Color.FromArgb(R, G, B));

                }
            }

            pictureBox2.Invalidate();

            Cursor = Cursors.Default;
        }

//Filtr Gaussa
        private void button5_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            Bitmap b1 = (Bitmap)pictureBox1.Image;
            Bitmap b2 = (Bitmap)pictureBox2.Image;

            int[,] maska = new int[3, 3];
            maska[0, 0] = 1;
            maska[0, 1] = 2;
            maska[0, 2] = 1;
            maska[1, 0] = 2;
            maska[1, 1] = 4;
            maska[1, 2] = 2;
            maska[2, 0] = 1;
            maska[2, 1] = 2;
            maska[2, 2] = 1;

            int norm = 0;
            for (int i = 0; i < 3; i++)    //normowanie maski(czyli dzielenie przez sumę elementów)
                for (int j = 0; j < 3; j++)
                    norm += maska[i, j];

            int R, G, B;
            Color k;

            for (int i = 1; i < szer - 1; i++) //(od 1 bo piksele brzegowe wyjdą poza obraz)
            {
                for (int j = 1; j < wys - 1; j++)
                {
                    R = B = G = 0;

                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < 3; y++)
                        {
                            k = b1.GetPixel(i + x - 1, j + y - 1); //piksel lewy
                            R += k.R * maska[x, y];
                            G += k.G * maska[x, y];
                            B += k.B * maska[x, y];
                        }
                    }

                    if (norm != 0)
                    {
                        R /= norm;
                        G /= norm;
                        B /= norm;
                    }

                    if (R < 0) R = 0;
                    if (R > 255) R = 255;

                    if (G < 0) G = 0;
                    if (G > 255) G = 255;

                    if (B < 0) B = 0;
                    if (B > 255) B = 255;


                    b2.SetPixel(i, j, Color.FromArgb(R, G, B));

                }
            }

            pictureBox2.Invalidate();

            Cursor = Cursors.Default;
        }

//Wyjście z aplikacji
        private void button6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //Zapisywanie otrzymanego obrazu o nazwie wpisanej w textBox1
        private void button12_Click(object sender, EventArgs e)
        {
            if(pictureBox2.Image != null)
            {
                pictureBox2.Image.Save(textBox1.Text + "(zmienione).jpg", ImageFormat.Jpeg);
            }
            else
            {
                string message = "Nie ma zmienionego obrazu. Anulować operację?";
                string caption = "Error Detected";
                MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                DialogResult result;

                //Wyświetla MassageBox
                result = MessageBox.Show(message, caption, buttons);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    this.Close();
                }

            }
        }



    }
}

Program napisany w języku C# w środowisku Visual Studio. Pozwala na wczytanie dowolnego zdjęcia, modyfikację obrazu w oparciu o algorytm wyrównywania histogramu oraz w oparciu o filtry rozmywające
(filtr uśredniający oraz filtr Gaussa).

Aby uruchomić program wystarczy otworzyć plik WindowsFormsApplication12.exe z folderu WindowsFormsApplication12/bin/Debug/

Plik: Edytor.Designer.cs tworzy interfejs GUI za pomocą Projektanta formularzy systemu Windows. Plik: Edytor.cs zawiera kod opisujący funkcje wywoływane dla poszczególnych komponentów składających się na GUI.

W chwili otwarcia programu ukazuje nam się okno interfejsu. Zawiera ono dwa PictureBox'y (pierwszy to obraz wejściowy a drugi wyjściowy). PictureBoxy automatycznie dopasowują swój rozmiar do paneli w których się znajdują po wczytaniu zdjęcia.(Dzięki właściwości SizeMode = AutoSize).
Wczytujemy dowolny plik png. lub jpg.(klikając przycisk "Wczytaj" otwiera się openFileDialog). Następnie możemy modyfikować obraz klikając odpowiedni Button. 
Przycisk zapisz pozwala zapisać plik wynikowy o nazwie wpisanej w textBox (plik zapisze się w folderze: bin/Debug ).
Trzy zaimplementowane w programie algorytmy bazują na Bitmapach zdjęć PictureBox'ów . Wczytują wartość koloru czerwonego/zielonego/niebieskiego każdego z pikseli zdjęcia/zdjęć wejściowych, zmieniając w odpowiedni dla danego algorytmu sposób ich wartość i przypisując ją dla pikseli obrazu wyjściowego.
W momencie wykonywania algorytmu(po kliknięciu któregoś z przycisków) dokonuje się zmiana kursora na kursor czekania.

Cursor = Cursors.WaitCursor;

Wyrównywanie histogramu to przeksztalcenie obrazu przy pomocy odpowiednio przygotowanej tablicy LUT. Operacja wyrównywania histogramu pozwala na uwypuklenie tych szczegółów w obrazie, które z uwagi na niewielki kontrast sa mało widoczne.
W funkcji calcLut obliczam dystrybuantę rozkładu prawdopodobieństwa, na której podstawie otrzymujemy tablice LUT:
   
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
        
W funkcji wywoływanej po kliknięciu przycisku "Wyrównanie Histogramu" tworzę tablice dla składowych(wcześniej zliczam do tablic ilość pikseli o natężeniu danej barwy(czerwonej, zielonej lub niebieskiej)):  

 ...
 
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
            
...

Następnie przetwarzając obraz wejściowy ustalalam kolor dla pikseli obrazu wyjściowego i go odświeżam:

...

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
            
...

Algorytmy Filrów Rozmywających bazują na maskach o wymiarach 3x3. 
Filtr Uśredniający ma maskę:
1 1 1
1 1 1
1 1 1
Filtr Gaussa:
1 2 1 
2 4 2
1 2 1
Zaimplementowane jest w nich normalizowanie maski(czyli dzielimy się wynik przez sumę współczynników maski) - stosujemy ją aby uniknąć wyjścia z zakresu intensywności obrazu:

...

int norm = 0;
            for (int i = 0; i < 3; i++)    
                for (int j = 0; j < 3; j++)
                    norm += maska[i, j];
                    
...

                  if (norm != 0)
                    {
                        R /= norm;
                        G /= norm;
                        B /= norm;
                    }
                    
...

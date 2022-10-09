using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;

namespace WFsqlApp1
{
    public partial class Form1 : Form           //далее я буду называть опорный элемент базисом
    {

        private string Fname = @"input.txt";            //имя файла для сохранения / открытия    
        private int X, Y;                               //объявление переменных для размерности

        private double[] Mass0 = new double[20];        //основная функция
        private double[,] Matr0 = new double[20, 20];   //матрица ограничений / а также матрица действий

        private string[] MassX = new string[20];        //строка Иксов
        private string[] MassY = new string[20];        //столбец Игреков / вспомогательные переменные

        private int MinY = 0;                           //индекс минимального базиса в столбце
        private double MinY_P;                          //значение минимального базиса в столбце

        private string BufStr = "";                     //вспомогательная переменная для перестановки
        private int Try = 0;                            //номер действия
        private int TryPr = 0;                          //отображаемый номер действия

        private bool Check_2 = false;                   //переменные для проверки создания финальной таблицы
        private bool BackCh = false;                    //переменная для проверки возможности шага в перед

        private string Ans = "";                        //строка ответа
        private double[] Ans_X = new double[20];        //массив отсортированых иксов из последней таблицы для ответа
        private bool ChAns = false;                     //преверка есть ли ответ
        private bool ChErr = false;                     //преверка есть ли Error

        private double[,,] Matr0Save = new double[20, 20, 20];   //массивы для сохранения вывода и возвращения к нему
        private string[,] MassYSave = new string[20, 20];
        private string[,] MassXSave = new string[20, 20];
        private int[] TrySave = new int[20];
        private int[] ColSaveY = new int[20];
        private int[] ColSaveX = new int[20];
        private int TL = 0;                             //переменная чтобы не перегнать действия при возврате вперед

        private bool[] MX0 = new bool[20];              //массив показывающий вабранные переменные или нет
        private int Che;                                //переменная для подсчета кол-ва выбранных элементов для гауса

        private bool ChOpen = false;                    //был ли открыт файл

        private int ChoiceX_Auto;                       //переменная бля автоматического выбора столбца



        public Form1()
        {
            InitializeComponent();
        }

        private void C_In()         //ввод
        {


            try                                            //создание таблиц
            {
                string TBY = textBoxY.Text;
                string TBX = textBoxX.Text;

                if (Che == 0)
                {     
                    Cleaner();  

                    Y = int.Parse(TBY);
                    X = int.Parse(TBX);

                    dataGridView1.RowCount = Y + 2;
                    dataGridView1.ColumnCount = X + 2;

                    dataGridView2.RowCount = Y + 4;
                    dataGridView2.ColumnCount = X + 2;

                    for (int y = 0; y < Y + 2; y++)                 //покраска и запрещение редактирования таблицы вывода
                        for (int x = 0; x < X + 2; x++)
                        {
                            dataGridView1.Rows[y].Cells[x].ReadOnly = true;
                            dataGridView1.Rows[y].Cells[x].Style.BackColor = Color.Gray;
                        }

                    for (int i = 0; i < 20; i++) { MassX[i] = "X " + (i + 1); }          //создание массивов и столбцов иксов и игреков
                    for (int i = 0; i < 20; i++) { MassY[i] = "Y " + (i + 1); }

                    for (int y = 0; y < Y + 4; y++)                 //покраска и запрещение редактирования таблицы ввода с последующими изменениями
                        for (int x = 0; x < X + 2; x++)
                        {
                            dataGridView2.Rows[y].Cells[x].ReadOnly = true;
                            dataGridView2.Rows[y].Cells[x].Style.BackColor = Color.Gray;
                        }


                    for (int y = 3; y < Y + 3; y++)
                        for (int x = 1; x < X + 2; x++)
                        {
                            dataGridView2.Rows[y].Cells[x].ReadOnly = false;
                            dataGridView2.Rows[y].Cells[x].Style.BackColor = Color.White;
                        }


                    for (int x = 1; x < X + 1; x++)             //покраска и преобразование первых 2-х строк ввода (иксов)
                    {
                        if (x == X)
                        {
                            dataGridView2.Rows[0].Cells[x].Value = MassX[x - 1] + " ->";
                            dataGridView2.Rows[0].Cells[x + 1].Value = "Min";
                            dataGridView2.Rows[1].Cells[x].ReadOnly = false;
                            dataGridView2.Rows[1].Cells[x].Style.BackColor = Color.White;
                        }
                        else
                        {
                            dataGridView2.Rows[0].Cells[x].Value = MassX[x - 1];
                            dataGridView2.Rows[1].Cells[x].ReadOnly = false;
                            dataGridView2.Rows[1].Cells[x].Style.BackColor = Color.White;
                        }
                    }

                    for (int y = 3; y < Y + 3; y++) { dataGridView2.Rows[y].Cells[0].Value = MassY[y - 3]; }        //игреков

                }
            }
            catch (Exception ex)
            {
                textBox6.Text = "????";
                MessageBox.Show("Неверные данные для создания таблиц " + ex);
            }
        }

        private void C_Out()        //вывод
        {

            dataGridView1.Rows[0].Cells[0].Value = TryPr;           //вывод на таблицу вывода (смотри названия переменных выше)

            for (int y = 1; y < Y + 2; y++)
                for (int x = 1; x < X + 2; x++)
                    dataGridView1.Rows[y].Cells[x].Value = Matr0[y - 1, x - 1];

            for (int x = 1; x < X + 1; x++)
                dataGridView1.Rows[0].Cells[x].Value = MassX[x - 1];

            for (int y = 1; y < Y + 1; y++)
                dataGridView1.Rows[y].Cells[0].Value = MassY[y - 1];


            for (int y = 0; y < Y + 2; y++)                         //покраска нужна чтобы убрать покрашенный базис
                for (int x = 0; x < X + 2; x++)
                    dataGridView1.Rows[y].Cells[x].Style.BackColor = Color.Gray;

            bool Check = true;          //проверка на создание 2-й таблицы
            Ans = "";
            double Result = 0;
            string[] sub = new string[2];


            for (int x = 0; x < X; x++)
            {
                sub = MassX[x].Split(' ');

                if (sub[0] != "Y") 
                {
                    bool Ch = false;
                    for (int y = 0; y < Y; y++)
                        if (Matr0[y, x] > 0)
                            Ch = true;

                    if (Matr0[Y, x] < 0 && (Ch || Check_2))
                        Check = false;
                }
            }

            if (Check && Try != 0)
            {               

                if (Check_2)        //проверка на вывод решения
                {
                    for (int y = 0; y < Y; y++)
                    {
                        sub = MassY[y].Split(' ');
                        if (sub[0] == "X")
                            Ans_X[int.Parse(sub[1]) - 1] = Matr0[y, X];
                    }
                    for (int x = 0; x < X; x++)
                    {
                        Result += Ans_X[x] * Mass0[x];                          //вывод
                    }
                    Ans = "f* ( ";
                    if (checkBox1.Checked == true)
                        Drob_Ans();
                    else
                        for (int x = 0; x < X; x++)
                        {
                            Ans += Ans_X[x];
                            if (X != 1 && x != X - 1)
                                Ans += "; ";
                        }
                    Ans += " ) = " + Result;
                    textBox6.Text = Ans;
                    ChAns = true;
                }
                else
                {
                    Check_2 = true;
                    TryPr = 0;

                    for (int x = 0; x < X + 1; x++)
                    {
                        Matr0[Y, x] = 0;
                        for (int y = 0; y < Y; y++)
                        {
                            sub = MassY[y].Split(' ');
                            if (sub[0] == "X")
                                Matr0[Y, x] -= Matr0[y, x] * Mass0[int.Parse(sub[1]) - 1];          //преобразование 1-й во 2-ю таблицу

                        }
                        if (x != X)
                        {
                            sub = MassX[x].Split(' ');
                            if (sub[0] == "X")
                                Matr0[Y, x] += Mass0[int.Parse(sub[1]) - 1];
                        }
                    }

                    C_Out();

                }

            }


            for (int y = 0; y < Y + 1; y++)               //сохранение для отката
                for (int x = 0; x < X + 1; x++)
                    Matr0Save[Try, y, x] = Matr0[y, x];
            for (int y = 0; y < Y; y++)
                MassYSave[Try, y] = MassY[y];
            for (int x = 0; x < X + 1; x++)
                MassXSave[Try, x] = MassX[x];
            TrySave[Try] = TryPr;



            ChToMis(); 
            Drob();
            OppPreview();

        }

        private void ChToMis()      //проверка на неограниченность
        {
            bool MisCh = false;
            bool MisCh1 = false;
            if (dataGridView1.Rows[0].Cells[0].Value != null)
            {
                MisCh = true;

                for (int x = 0; x < X; x++)
                {
                    string[] Sub = MassX[x].Split(' ');
                    if (Matr0[Y, x] < 0 && Sub[0] == "X")
                    {
                        MisCh1 = true;
                        for (int y = 0; y < Y; y++)
                            if (Matr0[y, x] > 0)
                                MisCh = false;
                    }
                }
            }

            if (MisCh && MisCh1 && !ChAns)
            {
                textBox6.Text = "Ф-я не ограничена";
                ChErr = true;
            }
            if (!ChAns && !MisCh1)
            {
                textBox6.Text = "А как решать?";
                ChErr = true;
            }

        }

        private void Drob()
        {
            if (checkBox1.Checked == true)
            {
                double per;
                for (int y = 0; y < Y + 1; y++)
                    for (int x = 0; x < X + 1; x++)
                        if (Matr0[y, x] % 1 != 0)
                        {
                            int i = 2;
                            per = Matr0[y, x];
                            if (per < 0)
                            {
                                per *= -1;
                                while (per * i % 1 > 0.00001 && per * i % 1 < 0.99999)
                                    i++;
                                per *= -1;
                            }
                            else
                            {
                                while (per * i % 1 > 0.00001 && per * i % 1 < 0.99999)
                                    i++;
                            }
                            if (Math.Round(per * i) == 0)
                                if (per < 0)
                                    dataGridView1.Rows[y + 1].Cells[x + 1].Value = "< 0";
                                else
                                    dataGridView1.Rows[y + 1].Cells[x + 1].Value = 0;
                            else
                            {
                                if ((Math.Round(per * i)) / i % 1 != 0)
                                    dataGridView1.Rows[y + 1].Cells[x + 1].Value = Math.Round(per * i) + " / " + i;
                                else
                                    dataGridView1.Rows[y + 1].Cells[x + 1].Value = (Math.Round(per * i)) / i;
                            }
                        }

            }

        }

        private void Drob_Ans()
        {
            double per;
            for (int x = 0; x < X; x++)
            {
                if (Ans_X[x] % 1 != 0)
                {
                    int i = 2;
                    per = Ans_X[x];
                    if (per < 0)
                    {
                        per *= -1;
                        while (per * i % 1 > 0.00001 && per * i % 1 < 0.99999)
                            i++;
                        per *= -1;
                    }
                    else
                    {
                        while (per * i % 1 > 0.00001 && per * i % 1 < 0.99999)
                            i++;
                    }
                    if ((Math.Round(per * i)) / i % 1 != 0)
                        Ans += Math.Round(per * i) + " / " + i;
                    else
                        Ans += (Math.Round(per * i)) / i;
                }
                else
                {
                    Ans += Ans_X[x];
                }
                if (X != 1 && x != X - 1)
                    Ans += "; ";
            }

        }

        
        private void OppPreview()
        {
            if (checkBox2.Checked != true) 
            {
                int MY = 0;
                for (int x = 0; x < X; x++)
                {
                    bool Ch = false;
                    double MY_P = 99999;
                    for (int y = 0; y < Y; y++)
                    {
                        if (Matr0[y, x] > 0 )            //поиск минимального базиса
                        {
                            if (Matr0[y, X] / Matr0[y, x] < MY_P)
                            {
                                MY = y;
                                MY_P = Matr0[y, X] / Matr0[y, x];
                                Ch = true;
                            }                            
                        }
                    }
                    if(Ch)
                        dataGridView1.Rows[MY + 1].Cells[x + 1].Style.BackColor = Color.Green;
                }
            }

        }



        private void Gauss()
        {
            int Che0 = Che;
            
            if (Che0 < X)
            {
                bool[] ChCh = new bool[20];
                double[,] Matr = new double[20, 20];
                int[] SaveY = new int[20];
                int[] SaveX = new int[20];

                for (int y = 0; y < 20; y++) { ChCh[y] = true; }

                for (int y = 0; y < Che0; y++)
                    for (int x = 0; x < X + 1; x++)
                    {
                        Matr[y, x] = Matr0[y % Y, x];
                    }

                int I = 0;

                for (int x = 0; x < X; x++)
                {
                    if (MX0[x])
                    {
                        bool Ch = true;
                        for (int y = 0; y < Che0; y++)
                            if (Matr[y, x] != 0 && ChCh[y])
                            {
                                for (int y1 = 0; y1 < Che0; y1++)
                                {
                                    for (int x1 = 0; x1 < X + 1; x1++)
                                    {
                                        if (x1 != x && Matr[y1, x] != 0)
                                            Matr[y1, x1] /= Matr[y1, x];
                                    }
                                    Matr[y1, x] = 1;
                                }


                                for (int y1 = 0; y1 < Che0; y1++)
                                    for (int x1 = 0; x1 < X + 1; x1++)
                                    {
                                        if (y1 != y && Matr[y1, x] != 0)
                                            Matr[y1, x1] -= Matr[y, x1];
                                    }

                                SaveY[I] = y;
                                SaveX[I] = x;
                                I++;

                                ChCh[y] = false;
                                MX0[x] = false;
                                Che--;
                                dataGridView2.Rows[0].Cells[x + 1].Style.BackColor = Color.DarkGreen;
                                Ch = false;
                            }


                        if (Ch)
                            MessageBox.Show("Один из иксов равен 0");

                    }
                }


                for (int i = 0; i < I; i++)                 // корекция (округление до 1)
                {
                    for (int x1 = 0; x1 < X + 1; x1++)
                    {
                        if (x1 != SaveX[i])
                            Matr[SaveY[i], x1] /= Matr[SaveY[i], SaveX[i]];
                    }
                    Matr[SaveY[i], SaveX[i]] = 1;
                }

                for (int i = 0; i < I; i++)
                {
                    string Buf = MassY[SaveY[i]];
                    MassY[SaveY[i]] = MassX[SaveX[i]];
                    MassX[SaveX[i]] = Buf;
                    //MessageBox.Show("y", SaveY[i].ToString());
                    //MessageBox.Show("x", SaveX[i].ToString());
                    //MessageBox.Show("i", i.ToString());                    
                }

                for (int x = 0; x < X; x++) 
                {
                    Matr[Che0, x] = 0;
                    string[] SubX = MassX[x].Split(' ');
                    if (SubX[0] == "X")
                        for (int y = 0; y < Che0; y++)
                        {
                            string[] SubY = MassY[y].Split(' ');
                            Matr[Che0, x] -= Matr[y, x]*Mass0[int.Parse(SubY[1]) - 1];
                        }
                    Matr[Che0, x] += Mass0[x];
                }
                for (int y = 0; y < Che0; y++)
                {
                    string[] SubY = MassY[y].Split(' ');
                    Matr[Che0, X] -= Matr[y, X] * Mass0[int.Parse(SubY[1]) - 1];
                }
                    

                for (int y = 0; y < Che0 + 1; y++)
                    for (int x = 0; x < X + 1; x++)
                    {
                        Matr0[y, x] = Matr[y, x];
                    }


                Y = Che0;
                //X = X - Che0;                

                dataGridView1.RowCount = Y + 2;
                dataGridView1.ColumnCount = X + 2;

                for (int y = 0; y < Y + 2; y++)                 //покраска и запрещение редактирования таблицы вывода
                    for (int x = 0; x < X + 2; x++)
                    {
                        dataGridView1.Rows[y].Cells[x].ReadOnly = true;
                        dataGridView1.Rows[y].Cells[x].Style.BackColor = Color.Gray;
                    }



            }
            else 
            {
                MessageBox.Show(" уменьшите кол-во пременных ");
            }

            
        }

        private void Cleaner()
        {            
            ChOpen = false;
            Che = 0;
            Try = 0;        
            TryPr = 0;
            Ans = "";
            textBox6.Text = "";
            Check_2 = false;
            ChAns = false;
            ChErr = false;
            for (int x = 0; x < X; x++) { Ans_X[x] = 0; }
            for (int x = 0; x < X; x++) { MX0[x] = false; }
        }




        private void button1_Click(object sender, EventArgs e)          //старт и сброс
        {

            C_In();             //  инициализация таблиц

            try                                                     // операции над таблицами
            {
                BackCh = false;
                ChAns = false;

                for (int y = 3; y < Y + 3; y++)                     //запихиваем все в массивы
                    for (int x = 1; x < X + 2; x++)
                    {
                        string[] sub = (Convert.ToString(dataGridView2.Rows[y].Cells[x].Value)).Split('/');
                        if (sub.Length > 1)
                        {
                            Matr0[y - 3, x - 1] = Convert.ToDouble(sub[0]) / Convert.ToDouble(sub[1]);
                        }
                        else
                        {
                            Matr0[y - 3, x - 1] = Convert.ToDouble(dataGridView2.Rows[y].Cells[x].Value);
                        }
                    }

                for (int x = 0; x < X; x++)
                {
                    string[] sub = (Convert.ToString(dataGridView2.Rows[1].Cells[x + 1].Value)).Split('/');
                    if (sub.Length > 1)
                    {
                        Mass0[x] = Convert.ToDouble(sub[0]) / Convert.ToDouble(sub[1]);
                    }
                    else
                    {
                        Mass0[x] = Convert.ToDouble(dataGridView2.Rows[1].Cells[x + 1].Value);
                    }

                }


                for (int x = 0; x < X + 1; x++)                     //делаем 0-е преобразование
                {
                    Matr0[Y, x] = 0;
                    for (int y = 0; y < Y; y++)
                        Matr0[Y, x] -= Matr0[y, x];
                }

                if (Che > 0)                
                    Gauss();                

                C_Out();                //вывод (в данном случае 0-й таблицы)


                if (checkBox2.Checked == true)
                    while(!ChAns && !ChErr)
                    {
                        button2_Click(sender, e);
                        for (int x = 0; x < X; x++)
                        {
                            double AutoMin = 99999;
                            string[] sub = MassX[x].Split(' ');
                            if (Matr0[Y, x] < 0 && sub[0] != "Y" && Matr0[Y, x]<AutoMin)
                            {
                                AutoMin = Matr0[Y, x];
                                ChoiceX_Auto = x + 1;
                            }
                        }
                    }
            }
            catch (Exception ex)
            {

                textBox6.Text = "Ошибка!!!!";

                MessageBox.Show("" + ex);
            }
        }

        private void button2_Click(object sender, EventArgs e)          //пошаговое решение
        {
            int ChoiceX;

            try
            {               

                if (checkBox2.Checked != true)
                {
                    ChoiceX = dataGridView1.SelectedCells[0].ColumnIndex;
                    //int ChoiceY = dataGridView1.SelectedCells[0].RowIndex;
                }
                else
                {
                    ChoiceX = ChoiceX_Auto;
                }



                if (ChoiceX > 0 && ChoiceX < X + 1)         //проверка в праывильном ли поле выбирает пользователь
                    try
                    {
                        ChoiceX--;
                        //ChoiceY--;
                        string[] sub = MassX[ChoiceX].Split(' ');
                        BackCh = false;
                        bool Ch = true;


                        if (Matr0[Y, ChoiceX] < 0 && sub[0] != "Y")           //проверка на отрицательность выбраного элемента нижней строки
                        {

                            MinY_P = 99999;
                            for (int y = 0; y < Y; y++)
                            {
                                if (Matr0[y, ChoiceX] > 0 && Matr0[y, X] / Matr0[y, ChoiceX] < MinY_P)            //поиск минимального базиса
                                {
                                    MinY = y;
                                    MinY_P = Matr0[y, X] / Matr0[y, ChoiceX];
                                    Ch = false;
                                }
                            }



                            //MinY_P = 99999;
                            //for (int y = 0; y < Y; y++)
                            //{
                            //    if (Matr0[y, ChoiceX] > 0 && Matr0[y, X] / Matr0[y, ChoiceX] < MinY_P)            //поиск минимального базиса
                            //    {
                            //        MinY = y;
                            //        MinY_P = Matr0[y, X] / Matr0[y, ChoiceX];
                            //    }
                            //}

                            //if (ChoiceY > -1 && ChoiceY < Y && Matr0[ChoiceY, ChoiceX] > 0 && ChoiceY == MinY)
                            //{
                            //    Ch = false;
                            //    MinY = ChoiceY;
                            //}


                            if (Ch)         //проверка есть ли в выбранном столбце подходящие элементы
                            {
                                textBox6.Text = "не подходит";
                            }
                            else
                            {

                                for (int x = 0; x < X + 1; x++)         //действия над строкой базиса
                                {
                                    if (x != ChoiceX)
                                        Matr0[MinY, x] /= Matr0[MinY, ChoiceX];
                                }


                                for (int y = 0; y < Y + 1; y++)         //действия над элементами не имеющих общик координать с базисом
                                {
                                    for (int x = 0; x < X + 1; x++)
                                    {
                                        if (x != ChoiceX && y != MinY)
                                        {
                                            Matr0[y, x] = Matr0[y, x] - (Matr0[y, ChoiceX] * Matr0[MinY, x]);
                                        }
                                    }
                                }

                                for (int y = 0; y < Y + 1; y++)         //действия над столбцом базиса
                                {
                                    if (y != MinY)
                                        Matr0[y, ChoiceX] /= -Matr0[MinY, ChoiceX];
                                }

                                Matr0[MinY, ChoiceX] = 1 / Matr0[MinY, ChoiceX];      //преобразование базиса



                                BufStr = MassY[MinY];               //замена в строке Х и в столбце У
                                MassY[MinY] = MassX[ChoiceX];
                                MassX[ChoiceX] = BufStr;

                                Try++;
                                TryPr++;

                                C_Out();

                                dataGridView1.Rows[MinY + 1].Cells[ChoiceX + 1].Style.BackColor = Color.Red;// выделение цветом базиса
                                ColSaveY[Try] = MinY + 1;
                                ColSaveX[Try] = ChoiceX + 1;
                            }
                        }
                        else
                        {
                            textBox6.Text = "????";
                        }
                    }
                    catch (Exception ex)
                    {
                        textBox6.Text = "Ошибка!!!!";

                        MessageBox.Show("" + ex);
                    }
                else
                    textBox6.Text = "не правильно выделено";
            }
            catch { MessageBox.Show("Не сейчас, снвчала объявите таблицы"); }
        }

        private void открытьToolStripMenuItem_Click_1(object sender, EventArgs e)           //открытие файла
        {
            if (File.Exists(Fname))
            {
                ChOpen = true;

                string[] lines = File.ReadAllLines(Fname);
                string[] OpSub = lines[0].Split('|');

                textBoxY.Text = OpSub[0];
                textBoxX.Text = OpSub[1];

                C_In();

                string[] temp = lines[1].Split('|');        //вывод и присваивание значений из файла

                for (int x = 0; x < X; x++)
                    dataGridView2.Rows[1].Cells[x + 1].Value = temp[x];

                for (int y = 2; y < Y + 2; y++)
                {
                    temp = lines[y].Split('|');
                    for (int x = 0; x < X + 1; x++)
                        dataGridView2.Rows[y + 1].Cells[x + 1].Value = temp[x];
                }
            }
            else
                textBox6.Text = "Нет файла!";
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)       //сохранение в файл
        {
            string[] LTM = new string[Y + 2];

            LTM[0] = Y + "|" + X;       //запись размерности в массив строк

            for (int x = 1; x < X + 1; x++)         //запись основного выражение в массив строк
                LTM[1] += dataGridView2.Rows[1].Cells[x].Value + "|";

            for (int y = 3; y < Y + 3; y++)         //запись ограничений в массив строк
                for (int x = 1; x < X + 2; x++)
                    LTM[y - 1] += dataGridView2.Rows[y].Cells[x].Value + "|";

            File.Create(Fname).Close();
            File.WriteAllLines(Fname, LTM);

        }
        private void button3_Click(object sender, EventArgs e)      //возврат во времени
        {
            if (Try > 0)
            {
                textBox6.Text = "";
                Check_2 = false;
                ChAns = false;
                ChErr = false;

                for (int x = 0; x < X; x++) { Ans_X[x] = 0; }

                if (!BackCh)        //проверка были ли ли до этого возраты 
                    TL = Try;
                BackCh = true;
                Try--;

                TryPr = TrySave[Try];     //присваивание значений элементам заного
                for (int x = 0; x < X; x++)
                    MassX[x] = MassXSave[Try, x];
                for (int y = 0; y < Y; y++)
                    MassY[y] = MassYSave[Try, y];
                for (int y = 0; y < Y + 1; y++)
                    for (int x = 0; x < X + 1; x++)
                        Matr0[y, x] = Matr0Save[Try, y, x];
                C_Out();
                dataGridView1.Rows[ColSaveY[Try]].Cells[ColSaveX[Try]].Style.BackColor = Color.Red;
            }

        }

        private void button4_Click(object sender, EventArgs e)      //назад в будущее
        {
            if (TL > Try && BackCh)
            {
                Try++;

                TryPr = TrySave[Try];     //присваивание значений элементам заного
                for (int x = 0; x < X; x++)
                    MassX[x] = MassXSave[Try, x];
                for (int y = 0; y < Y; y++)
                    MassY[y] = MassYSave[Try, y];
                for (int y = 0; y < Y + 1; y++)
                    for (int x = 0; x < X + 1; x++)
                        Matr0[y, x] = Matr0Save[Try, y, x];
                C_Out();
                dataGridView1.Rows[ColSaveY[Try]].Cells[ColSaveX[Try]].Style.BackColor = Color.Red;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            int ChoiceX = dataGridView2.SelectedCells[0].ColumnIndex;
            ChoiceX--;
            if (ChoiceX >= 0 && X > ChoiceX ) 
            {
                if (MX0[ChoiceX])
                {
                    MX0[ChoiceX] = false;
                    dataGridView2.Rows[0].Cells[ChoiceX + 1].Style.BackColor = Color.Gray;
                    Che--;
                }
                else
                {
                    MX0[ChoiceX] = true;
                    dataGridView2.Rows[0].Cells[ChoiceX + 1].Style.BackColor = Color.YellowGreen;
                    Che++;
                }                
            }
            else
            {
                MessageBox.Show("Неправильно выбранна переменная");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Программа изображает бурную деятельность с умным интерфейсои");
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WFsqlApp1
{
    public partial class Form2 : Form
    {
        public string[,,] DGV = new string[20, 20, 20];
        public int X, Y, Try, TL;
        public int[] ColSaveY = new int[20];
        public int[] ColSaveX = new int[20];
        public bool BackCh, Check_2;
        public string[,] InterimSave = new string[20, 20];
        public int[] MPersH = new int[20];

        private void обновитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Refresh_2();
        }

        public void Refresh_2()
        {
            try
            {


                if (Form1.SelfRef != null)
                {                    
                    X = Form1.SelfRef.GetX();
                    Y = Form1.SelfRef.GetY();
                    Try = Form1.SelfRef.GetTry();
                    TL = Form1.SelfRef.GetTL();
                    BackCh = Form1.SelfRef.GetBackCh();
                    Check_2 = Form1.SelfRef.GetCheck_2();
                }  


                int T;
                if (BackCh)
                    T = TL;
                else
                    T = Try;
                T++;
                if(Check_2)
                    T++;

                dataGridView1.RowCount = (Y + 3) * T + 1;
                dataGridView1.ColumnCount = X + 2;

                for (int y = 0; y < (Y + 3) * T + 1; y++)
                    for (int x = 0; x < X + 2; x++)
                    {
                        dataGridView1.Rows[y].Cells[x].Style.BackColor = Color.Gray;
                    }
                int Q = 0;
                bool Ch = true;
                for (int t = 0; t < T; t++)
                {
                    if (DGV[t, 0, 0] == "0" && t != 0 && Ch)
                    {
                        for (int y = 0; y < Y + 2; y++)
                            for (int x = 0; x < X + 2; x++)
                            {
                                dataGridView1.Rows[t * (Y + 3) + 1 + y].Cells[x].Value = InterimSave[y, x];
                                dataGridView1.Rows[t * (Y + 3) + 1 + y].Cells[x].Style.BackColor = Color.LightGoldenrodYellow;
                            }
                        t++;
                        Q = 1;
                        Ch = false;
                    }


                    for (int y = 0; y < Y + 2; y++)
                        for (int x = 0; x < X + 2; x++)
                        {
                            dataGridView1.Rows[t * (Y + 3) + 1 + y].Cells[x].Value = DGV[t - Q, y, x];
                        }
                    if (t < T - 1)
                        dataGridView1.Rows[t * (Y + 3) + 1 + ColSaveY[t - Q + 1]].Cells[ColSaveX[t - Q + 1]].Style.BackColor = Color.Red;

                }
                if(DGV[Try + 1, 0, 0] == "0" || DGV[Try, 0, 0] == Try.ToString()) { Q = 0; }
                for (int x = 0; x < X + 2; x++) { dataGridView1.Rows[(Try + Q) * (Y + 3)].Cells[x].Style.BackColor = Color.Green; }
                for (int x = 0; x < X + 2; x++) { dataGridView1.Rows[(Try + Q + 1) * (Y + 3)].Cells[x].Style.BackColor = Color.Green; }
            }
            catch { }
        }

        public static Form2 SelfRef { get; set; }
        public Form2()
        {
            SelfRef = this;
            InitializeComponent();
        }
        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Excel;

namespace onlinesoft
{
    public partial class Form1 : Form
    {
        //定义局部变量扫描人工号,当前工步名称,当前区域
        private string m_gh;
        private string m_gbmc;
        private double m_gbbh;
        private double m_gxbh;

        private string m_jhbh;
        private int sm_num = 0;
        private int sm_bfnum = 0;

        cls_batch mbatch = new cls_batch();

        public Form1()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = GetNetCardMacAddress();
            cls_batch.macaddress= GetNetCardMacAddress();
            double mqybm = mbatch.getcjdbh(cls_batch.macaddress);
            cls_batch.m_device = mqybm;
            cls_batch.m_qybm = mbatch.getcjdqybm(cls_batch.macaddress);
            this.toolStripStatusLabel1.Text = this.toolStripStatusLabel1.Text.Trim() + "  " + mbatch.getqyxinxi(cls_batch.m_qybm);            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmselgbmc mfrm = new frmselgbmc();
            try
            {
                if (mfrm.ShowDialog() == DialogResult.OK)
                {
                    DataGridViewRow mrow = mfrm.dataGridView1.CurrentRow;
                    if (mrow != null)
                    {
                        this.m_gbmc = mrow.Cells["gbmc"].Value.ToString().Trim();
                        this.m_gbbh = Convert.ToDouble(mrow.Cells["id"].Value);
                        this.m_gxbh = Convert.ToDouble(mrow.Cells["gxmc"].Value);                        
                    }
                    //上线，转运，成品入库
                    if (m_gbmc.IndexOf("上线")!=-1)
                    {
                        label3.Visible = true;
                        textBox2.Visible = true;
                        button6.Visible = true;
                    }
                    else
                    {
                        label3.Visible = false;
                        textBox2.Visible = false;
                        button6.Visible = false;
                    }
                    this.toolStripStatusLabel1.Text = this.toolStripStatusLabel1.Text.Trim() + "  " + "当前工步:" + m_gbmc.Trim();
                }
            }
            finally
            {
                mfrm.Dispose();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            frmghlr mfrm = new frmghlr();
            try
            {
                if (mfrm.ShowDialog() == DialogResult.OK)
                {
                    m_gh = mfrm.textBox1.Text.Trim();
                    if (string.IsNullOrEmpty(m_gh.Trim()))
                    {
                        MessageBox.Show("员工工号不可为空");
                        return;
                    }
                    else
                    {
                        if (!mbatch.chkgh(m_gh))
                        {
                            MessageBox.Show("此工号不存在");
                            m_gh = "";
                            return;
                        }
                    }
                }                
            }
            finally
            {
                mfrm.Dispose();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            {
                return;
            }
            Microsoft.Office.Interop.Excel.Application myexcel = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook mybook = myexcel.Workbooks.Add(true);
            Microsoft.Office.Interop.Excel.Worksheet mysheet = (Worksheet)mybook.Worksheets.Add(Type.Missing, Type.Missing, Type.Missing, Type.Missing);
            mysheet.Name = "扫描清单";
            mysheet.Cells[1, 1] = "清单";
            mysheet.get_Range(mysheet.Cells[1, 1], mysheet.Cells[1, 6]).Font.Size = 12;
            mysheet.get_Range(mysheet.Cells[1, 1], mysheet.Cells[1, 6]).Font.Bold = true;
            mysheet.get_Range(mysheet.Cells[1, 1], mysheet.Cells[1, 6]).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            mysheet.get_Range(mysheet.Cells[1, 1], mysheet.Cells[1, 6]).Merge(true);
            mysheet.get_Range(mysheet.Cells[1, 1], mysheet.Cells[1, 6]).Borders.LineStyle = 7;
            mysheet.Cells[2, 1] = "工步名称";
            mysheet.Cells[2, 2] = "计划编号";
            mysheet.Cells[2, 3] = "产品代码";
            mysheet.Cells[2, 4] = "条码";
            mysheet.Cells[2, 5] = "采集时间";
            mysheet.Cells[2, 6] = "扫描人";           
            mysheet.get_Range(mysheet.Cells[2, 1], mysheet.Cells[2, 6]).HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
            mysheet.get_Range(mysheet.Cells[2, 1], mysheet.Cells[2, 6]).Borders.LineStyle = 7;
            for (int i = 0; i < dataGridView1.RowCount - 1; i++)
            {
                Microsoft.Office.Interop.Excel.Range myrange = mysheet.get_Range(mysheet.Cells[i + 3, 1], mysheet.Cells[i + 3, 6]);
                myrange.NumberFormatLocal = "@";
                myrange.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;
                myrange.Borders.LineStyle = 7;
                mysheet.Cells[i + 3, 1] = dataGridView1.Rows[i].Cells["f_process"].Value.ToString().Trim();
                mysheet.Cells[i + 3, 2] = dataGridView1.Rows[i].Cells["f_jhbh"].Value.ToString().Trim();
                mysheet.Cells[i + 3, 3] = dataGridView1.Rows[i].Cells["f_product"].Value.ToString().Trim();
                mysheet.Cells[i + 3, 4] = dataGridView1.Rows[i].Cells["f_barcode"].Value.ToString().Trim();
                mysheet.Cells[i + 3, 5] = dataGridView1.Rows[i].Cells["f_datetime"].Value.ToString().Trim();
                mysheet.Cells[i + 3, 6] = dataGridView1.Rows[i].Cells["f_optioner"].Value.ToString().Trim();                
            }
            myexcel.Visible = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string err = ""; 
               if(string.IsNullOrEmpty(this.textBox1.Text.Trim()))
               {
                   return;
               }
               bool mark = false;
               if (string.IsNullOrEmpty(m_gh))
               {
                   err="扫描人工号不可为空";
                   mark = true;                        
               }
               if (string.IsNullOrEmpty(m_gbmc))
               {
                   err="当前工步名称没有指定";
                   mark = true;
               }
               string barcode = this.textBox1.Text.Trim();
               //如果是报废状态,不判断当前是否连续扫描
               if (!checkBox1.Checked)
               {
                   short msrec = mbatch.chkbarcode(barcode, this.m_gbbh);
                   if (msrec == 1)
                   {
                       err = "当前条码当前工步不能连续扫描2次以上";
                       mark = true;
                   }
                   //如果当前非上线状态,且整机在线状态表不存在 0
                   if (!string.IsNullOrEmpty(m_gbmc))
                   {
                       if (m_gbmc.IndexOf("上线") == -1)
                       {
                           if (msrec == 0)
                           {
                               err = "此台产品没有上线操作";
                               mark = true;
                           }
                       }
                   }
               }
               else
               {
                   if (mbatch.chkbfisexist(barcode) == 1)
                   {
                       err = "此台产品已报废,不能再报废";
                       mark = true;
                   }
               }
               short mislrjh=0;
               if (this.textBox2.Visible)
               {
                   if(string.IsNullOrEmpty(this.textBox2.Text.Trim()))
                   {
                       err="当前工位须录入计划编号";
                       mark = true;
                   }
                   mislrjh=1;
               }
               m_jhbh = this.textBox2.Text.Trim();

               if (mark)
               {
                   this.textBox1.Clear();
                   panel1.BackColor = Color.Red;            
                   bool m_bz = false;
                   while (!m_bz)
                   {
                       if (MessageBox.Show(err, "警告", MessageBoxButtons.YesNo) == DialogResult.No)
                       {
                           m_bz = true;
                       }
                   }
                   return;
               }
               else
               {
                   panel1.BackColor = Color.Lime;
               }
               short mresult=0;
               if (this.checkBox1.Checked)
               {
                   mresult = mbatch.del_barcode(barcode, m_gh, m_gbbh, m_jhbh, mislrjh);
               }
               else
               {                   
                   mresult = mbatch.insert_barcode(barcode, m_gh, m_gbbh,m_gxbh, m_jhbh, mislrjh);
               }
               if (mresult == 0)
               {                   
                   this.textBox1.Clear();
                   panel1.BackColor = Color.Red;         
               }
               else
               {
                   if (this.checkBox1.Checked)
                   {
                       sm_bfnum = sm_bfnum + 1;
                       label2.Text = "当前报废数量:" + sm_bfnum.ToString().Trim() + "台";
                   }
                   else
                   {
                       sm_num = sm_num + 1;
                       label2.Text = "当前扫描数量:" + sm_num.ToString().Trim() + "台";
                   }
                   panel1.BackColor = Color.Lime;
                   string mjhbh = mbatch.getbarjhbh(barcode);
                   DateTime mdatetime = mbatch.getserdate();
                   string product = mbatch.getproduct(barcode);
                   int mindex = this.dataGridView1.Rows.Add();
                   this.dataGridView1.Rows[mindex].Cells["f_process"].Value = m_gbmc;
                   this.dataGridView1.Rows[mindex].Cells["f_jhbh"].Value = mjhbh;
                   this.dataGridView1.Rows[mindex].Cells["f_product"].Value = product;
                   this.dataGridView1.Rows[mindex].Cells["f_barcode"].Value = barcode;
                   this.dataGridView1.Rows[mindex].Cells["f_datetime"].Value = mdatetime;            
                   this.dataGridView1.Rows[mindex].Cells["f_optioner"].Value = m_gh;
                   this.textBox1.Clear();
               }
            }
        }

        public string GetNetCardMacAddress()
        {
            ManagementClass mc;
            ManagementObjectCollection moc;
            mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            moc = mc.GetInstances();
            string str = "";
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                    str = mo["MacAddress"].ToString();

            }
            Regex regex = new Regex(":");
            string ma = "";
            string[] substrings = regex.Split(str);
            foreach (string match in substrings)
            {
                if(string.IsNullOrEmpty(ma.Trim()))
                {
                    ma=match.Trim();
                }
                else
                {
                    ma = ma.Trim() + "-" + match.Trim();
                }
            }
            return ma;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            frmseljhbh mfrm = new frmseljhbh();
            try
            {
                if (mfrm.ShowDialog() == DialogResult.OK)
                {
                    DataGridViewRow mrow = mfrm.dataGridView1.CurrentRow;
                    if (mrow != null)
                    {
                        this.textBox2.Text = mrow.Cells["jhbh"].Value.ToString().Trim();
                    } 
                }
            }
            finally
            {
                mfrm.Dispose();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            this.textBox1.Focus();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                this.checkBox1.Checked = true;
            }
            if (e.KeyCode == Keys.F12)
            {
                this.checkBox1.Checked = false;
            }
            if (e.KeyCode == Keys.F9)
            {
                button6.Enabled = false;
                button1.Enabled = false;
                button5.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
            }
            if (e.KeyCode == Keys.F10)
            {
                button6.Enabled = true;
                button1.Enabled = true;
                button5.Enabled = true;
                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace onlinesoft
{
    public partial class frmseljhbh : Form
    {
        string cxsql = "select jhbh,cpdm from lo_t_mps where status=1";
        cls_data mdata = new cls_data();
        public frmseljhbh()
        {
            InitializeComponent();
        }

        private void frmseljhbh_Load(object sender, EventArgs e)
        {
            DataSet mdataset = mdata.getdataset(cxsql);
            this.dataGridView1.DataSource = mdataset.Tables[0];
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string mjh = this.textBox1.Text.Trim();
                cxsql = "select jhbh,cpdm from lo_t_mps where status=1 and jhbh like '" + mjh + "%'";
                DataSet mdataset = mdata.getdataset(cxsql);
                this.dataGridView1.DataSource = mdataset.Tables[0];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}

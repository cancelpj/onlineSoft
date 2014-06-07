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
    public partial class frmselgbmc : Form
    {
        cls_data mdata = new cls_data();
        public frmselgbmc()
        {
            InitializeComponent();
        }

        private void frmselgbmc_Load(object sender, EventArgs e)
        {
            double mdeviceid = cls_batch.m_qybm; 
            string msql = "select a.id,a.gxmc,a.gbmc,b.mc from lo_t_gbb a inner join (select c.gxmc,d.gxmc as mc from lo_t_qybmb c inner join lo_t_gxb d on c.gxmc=d.id where c.id=" + mdeviceid + ") b on a.gxmc=b.gxmc";
            DataSet gbdataset = mdata.getdataset(msql);
            this.dataGridView1.DataSource = gbdataset.Tables[0];
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}

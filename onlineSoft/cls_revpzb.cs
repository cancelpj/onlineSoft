using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace onlinesoft
{
    class cls_revpzb
    {
        cls_data mdata = new cls_data();
        private string m_process;
        private string m_qydm;


        public string mm_process
        {
            get
            {
                return this.m_process;
            }
            set
            {
                this.m_process = value;
            }
        }

        public string mm_gydm
        {
            get
            {
                return this.m_qydm;
            }
            set
            {
                this.m_qydm = value;
            }
        }

        public void getdevice()
        {
            mdata.openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = mdata.m_sqlconnect;
            }
        }

    }
}

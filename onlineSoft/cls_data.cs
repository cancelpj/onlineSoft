using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Security.Cryptography;

namespace onlinesoft
{
    class cls_data
    {
        private string constr = "";
        private bool isopened = false;
        public SqlConnection m_sqlconnect = null;
        
  
        public cls_data()
        {
            constr = "data source=192.168.2.15;initial catalog=LiveOA_test;max pool size=512;user id=test;password=f3721";            
        }

        public void openconnect()
        {
            if (!this.isopened)
            {
                m_sqlconnect = new SqlConnection(constr);
                m_sqlconnect.Open();
                this.isopened = true;
            }
        }

        public Int16 getrevdataaddress()
        {            
            Int16 revdataaddress =Convert.ToInt16(ConfigurationManager.AppSettings["revdataaddress"]);
            return revdataaddress;
        }

        public void closeconnect()
        {
            if (this.isopened)
            {
                m_sqlconnect.Close();
                this.isopened = false;
            }
        }

        public DataSet getdataset(string cxsql)
        {
            openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = m_sqlconnect;
                mcmd.CommandText = cxsql;
                SqlDataAdapter mada = new SqlDataAdapter();
                mada.SelectCommand = mcmd;
                DataSet myset = new DataSet();
                mada.Fill(myset);
                closeconnect();
                return myset;
            }
        }

    }
}

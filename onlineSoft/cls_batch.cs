using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;

namespace onlinesoft
{
    class cls_batch
    {
        cls_data mdata = new cls_data();
        ServiceReference1.mesinterlistSoapClient weblist = new onlinesoft.ServiceReference1.mesinterlistSoapClient(); 
        public static string macaddress;
        public static double m_device;
        public static double m_qybm;     

        //获取计划编号
        public string getbarjhbh(string barcode)
        {
            string mjhbh = "-1";
            string msql = "select jhbh from lo_t_jhzj where zjtm='"+barcode+"'";
            DataSet mdataset = mdata.getdataset(msql);
            if (mdataset.Tables[0].Rows.Count > 0)
            {
                mjhbh = mdataset.Tables[0].Rows[0]["jhbh"].ToString().Trim();
            }
            return mjhbh;
        }

        public short chkjhisexist(string jhbh, double cjd, double gbbh)
        {
            short mresult =0;
            string msql = "select jhbh from lo_t_wcsl where jhbh='"+jhbh+"' and cjd="+cjd+" and gb="+gbbh;
            DataSet mdataset = mdata.getdataset(msql);
            if (mdataset.Tables[0].Rows.Count > 0)
            {
                mresult = 1;
            }
            return mresult;
        }

        public string getproduct(string barcode)
        {
            string cpdm = weblist.getcpdm(barcode);
            return cpdm;
        }
       
        public int getmaxid(string barcode)
        {
            string msql="select max(id) as xh from lo_t_xcgsjlb where zjtm='"+barcode+"'";
            DataSet mdataset=mdata.getdataset(msql);
            int mid;
            if (mdataset.Tables[0].Rows[0]["xh"]==DBNull.Value)
            {
                mid=0;
            }
            else
            {
               mid=Convert.ToInt32(mdataset.Tables[0].Rows[0]["xh"]);
            }
            return mid;
        }

        public short chkbfisexist(string barcode)
        {
            string msql = "select zjtm from lo_t_zjbfqd where zjtm='"+barcode+"'";
            DataSet mdataset = mdata.getdataset(msql);
            if (mdataset.Tables[0].Rows.Count > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public short chkjhzj(string barcode, string jhbh)
        {
            short mresult = 0;
            string msql ="select jhbh from lo_t_jhzj where zjtm='" + barcode + "' and jhbh='" + jhbh + "'";
            DataSet mdataset = mdata.getdataset(msql);
            if (mdataset.Tables[0].Rows.Count > 0)
            {
                mresult = 1;
            }
            return mresult;
        }

        public string getgbmc(double gbbh)
        {
            string gbmc = "";
            string msql = "select gbmc from lo_t_gbb where id=" + gbbh;
            DataSet mdataset = mdata.getdataset(msql);
            if (mdataset.Tables[0].Rows.Count > 0)
            {
                gbmc = mdataset.Tables[0].Rows[0]["gbmc"].ToString().Trim();
            }
            return gbmc;
        }

        public double getlatedgbmc(string barcode)
        {
            double zjgb = 0;
            string msql = "select zjgb from lo_t_zjzxztb where zjtm='"+barcode+"'";
            DataSet mdataset = mdata.getdataset(msql);
            if (mdataset.Tables[0].Rows.Count > 0)
            {
                zjgb = Convert.ToDouble(mdataset.Tables[0].Rows[0]["zjgb"]);
            }
            return zjgb;
        }

        public short del_barcode(string barcode, string gh, double gmbh, string jhbh, short jhmark)
        {
            short mrec = 0;
            string m_jhbh = getbarjhbh(barcode);
            DateTime mdatetime = getserdate();
            //获取整机在线状态表中最近工步 0表示没有 不为0表示有            
            double mlatgb = getlatedgbmc(barcode);

            short mrex = chkjhisexist(jhbh, m_device, mlatgb);         
            //获取id
            int maxid = getmaxid(barcode);

            mdata.openconnect();
            SqlTransaction mtrans = mdata.m_sqlconnect.BeginTransaction();
            try
            {
                using (SqlCommand mcmd = new SqlCommand())
                {
                    mcmd.Connection = mdata.m_sqlconnect;
                    mcmd.Transaction = mtrans;
                    mcmd.CommandText = "delete from lo_t_zjzxztb where zjtm='"+barcode+"'";
                    mcmd.ExecuteNonQuery();
                    //如果完成数量表无记录0，则添加记录，1否则修改记录
                    if (mrex == 1)
                    {
                        mcmd.Parameters.Clear();
                        mcmd.CommandText = "update lo_t_wcsl set bfsl=isnull(bfsl,0)+1 where cjd=" + m_device + " and jhbh='" + m_jhbh + "' and gb=" + gmbh;
                        mcmd.ExecuteNonQuery();
                    }
                    else
                    {
                        mcmd.Parameters.Clear();
                        mcmd.CommandText = "insert into lo_t_wcsl(jhbh,cjd,gb,bfsl,gh) values(@p1,@p2,@p3,@p4,@p5)";
                        mcmd.Parameters.Add("@p1", SqlDbType.Char, 30).Value = m_jhbh;
                        mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = m_device;
                        mcmd.Parameters.Add("@p3", SqlDbType.Float).Value = gmbh;
                        mcmd.Parameters.Add("@p4", SqlDbType.Float).Value = 1;
                        mcmd.Parameters.Add("@p5", SqlDbType.Char, 30).Value = gh;
                        mcmd.ExecuteNonQuery();
                    }
                    // 更新现场工时记录表中完成时间                 
                    mcmd.CommandText = "update lo_t_xcgsjlb set wcsj='" + mdatetime + "',gh='" + gh + "' where id=" + maxid;
                    mcmd.ExecuteNonQuery();
                    mcmd.Parameters.Clear();
                    mcmd.CommandText = "update lo_t_xcgsjlb set hs=datediff(minute,kssj,wcsj) where id=" + maxid + " and (kssj is not null) and (wcsj is not null)";
                    mcmd.ExecuteNonQuery();
                    
                    mcmd.Parameters.Clear();
                    mcmd.CommandText = "insert into lo_t_zjbfqd(zjtm,cjd,gb,cjsj,gh) values(@p1,@p2,@p3,@p4,@p5)";
                    mcmd.Parameters.Add("@p1", SqlDbType.Char, 30).Value = barcode;
                    mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = m_device;
                    mcmd.Parameters.Add("@p3", SqlDbType.Float).Value = gmbh;
                    mcmd.Parameters.Add("@p4", SqlDbType.DateTime).Value = mdatetime;
                    mcmd.Parameters.Add("@p5", SqlDbType.Float).Value = gh;
                    mcmd.ExecuteNonQuery();
                }
                mtrans.Commit();
                mrec = 1;
            }
            catch
            {
                mtrans.Rollback();
                mrec = 0;
            }
            mdata.closeconnect();
            return mrec;
        }

        public short insert_barcode(string barcode, string gh,double gmbh,double gxbh,string jhbh,short jhmark)
        {
            short sresult = 0;
            //获取产品状态,是否返工 0为返工状态 1 正常状态         
            short mfzzt=chk_fgzt(barcode);

            // 获取返工是否切换到正常状态 0 非返工状态,1,可以切换到正常状态,2,继续返工状态
            short mqhzt = chkisfgzt(barcode, gmbh);

            //获取整机在线状态表中最近工步 0表示没有 不为0表示有            
            double mlatgb = getlatedgbmc(barcode);

            //判断整机在线状态表中整机条码是否已扫描 barcodeisexist为0 代表整机在线状态表不存在,1:条码存在，当前工步已存在，2：条码存在，当前工步不存在
            short barcodeisexist = chkbarcode(barcode, gmbh);

            

            string m_jhbh; 
            if (jhmark == 0)
            {
                //获取计划编号            
                m_jhbh = getbarjhbh(barcode);
            }
            else
            {
                m_jhbh = jhbh;
            }
            //判断完成数量表是否存在 0:不存在,1:存在
            short jhwcisexist = chkjhisexist(m_jhbh, m_device, mlatgb);
            //获取当前工序在产品工艺路线是否存在 0 不存在，1 存在

            short mrec = chkgylxisexist(barcode, gxbh);
            //获取id
            int maxid=getmaxid(barcode);
            short jhzjresult = chkjhzj(barcode, jhbh);

            string dqgbmc = getgbmc(gmbh);
          
            DateTime mdatetime = getserdate();
            
            mdata.openconnect();
            SqlTransaction mtrans = mdata.m_sqlconnect.BeginTransaction();
            try
            {
                using (SqlCommand mcmd = new SqlCommand())
                {
                    mcmd.Connection = mdata.m_sqlconnect;
                    mcmd.Transaction = mtrans;
                    if (jhmark == 1)
                    {
                        if (jhzjresult == 0)
                        {
                            mcmd.Parameters.Clear();
                            mcmd.CommandText = "insert into lo_t_jhzj(jhbh,zjtm) values(@p1,@p2)";
                            mcmd.Parameters.Add("@p1", SqlDbType.Char, 30).Value = jhbh;
                            mcmd.Parameters.Add("@p2", SqlDbType.Char, 30).Value =barcode;
                            mcmd.ExecuteNonQuery();
                        }
                    }
                    mcmd.Parameters.Clear();                   
                    if (barcodeisexist == 0)
                    {
                        // 只有上线工位才可以添加记录
                        if (jhmark == 1)
                        {
                            if (mfzzt == 0)
                            {
                                mcmd.CommandText = "insert into lo_t_zjzxztb(zjtm,zt,zjcjd,zjgb,cjsj,gh,tfgb) values(@p1,@p2,@p3,@p4,@p5,@p6,@p7)";
                                mcmd.Parameters.Add("@p1", SqlDbType.Char, 30).Value = barcode;
                                mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = 1;
                                mcmd.Parameters.Add("@p3", SqlDbType.Float).Value = m_device;
                                mcmd.Parameters.Add("@p4", SqlDbType.Float).Value = gmbh;
                                mcmd.Parameters.Add("@p5", SqlDbType.DateTime).Value = mdatetime;
                                mcmd.Parameters.Add("@p6", SqlDbType.Char, 30).Value = gh;
                                mcmd.Parameters.Add("@p7", SqlDbType.Float).Value = gmbh;
                                mcmd.ExecuteNonQuery();
                            }
                            else
                            {
                                mcmd.CommandText = "insert into lo_t_zjzxztb(zjtm,zt,zjcjd,zjgb,cjsj,gh,tfgb) values(@p1,@p2,@p3,@p4,@p5,@p6,@p7)";
                                mcmd.Parameters.Add("@p1", SqlDbType.Char, 30).Value = barcode;
                                mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = 0;
                                mcmd.Parameters.Add("@p3", SqlDbType.Float).Value = m_device;
                                mcmd.Parameters.Add("@p4", SqlDbType.Float).Value = gmbh;
                                mcmd.Parameters.Add("@p5", SqlDbType.DateTime).Value = mdatetime;
                                mcmd.Parameters.Add("@p6", SqlDbType.Char, 30).Value = gh;
                                mcmd.Parameters.Add("@p7", SqlDbType.Float).Value = DBNull.Value;
                                mcmd.ExecuteNonQuery();
                            }
                        }
                    }
                    else
                    {
                        if (mfzzt == 0)
                        {
                            mcmd.CommandText = "update lo_t_zjzxztb set zt=@p2,zjcjd=@p3,zjgb=@p4,cjsj=@p5,gh=@p6,tfgb=@p7 where zjtm=@p1";
                            mcmd.Parameters.Add("@p1", SqlDbType.Char, 30).Value = barcode;
                            if (mrec == 0)
                            {
                                mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = 2;
                            }
                            else
                            {
                                if ((mqhzt == 1) || (mqhzt == 0))
                                {
                                    mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = 0; 
                                }
                                else
                                {
                                    mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = 1;
                                }
                            }
                            mcmd.Parameters.Add("@p3", SqlDbType.Float).Value = m_device;
                            mcmd.Parameters.Add("@p4", SqlDbType.Float).Value = gmbh;
                            mcmd.Parameters.Add("@p5", SqlDbType.DateTime).Value = mdatetime;
                            mcmd.Parameters.Add("@p6", SqlDbType.Char, 30).Value = gh;
                            mcmd.Parameters.Add("@p7", SqlDbType.Float).Value = gmbh;
                            mcmd.ExecuteNonQuery();
                        }
                        else
                        {
                            mcmd.CommandText = "update lo_t_zjzxztb set zt=@p2,zjcjd=@p3,zjgb=@p4,cjsj=@p5,gh=@p6,tfgb=@p7 where zjtm=@p1";
                            mcmd.Parameters.Add("@p1", SqlDbType.Char, 30).Value = barcode;
                            if (mrec == 0)
                            {
                                mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = 2; 
                            }
                            else
                            {                               
                                mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = 0;                                                                                
                            }
                            mcmd.Parameters.Add("@p3", SqlDbType.Float).Value = m_device;
                            mcmd.Parameters.Add("@p4", SqlDbType.Float).Value = gmbh;
                            mcmd.Parameters.Add("@p5", SqlDbType.DateTime).Value = mdatetime;
                            mcmd.Parameters.Add("@p6", SqlDbType.Char, 30).Value = gh;
                            mcmd.Parameters.Add("@p7", SqlDbType.Float).Value = DBNull.Value;
                            mcmd.ExecuteNonQuery();                              
                        }
                    }
                    mcmd.Parameters.Clear();
                    if (jhwcisexist == 1)
                    {
                        if (mfzzt == 0)
                        {
                            mcmd.CommandText = "update lo_t_wcsl  set fgsl=isnull(fgsl,0)+1,gh='" + gh + "' where jhbh='" + m_jhbh + "' and cjd=" + m_device + " and gb=" + gmbh;
                            mcmd.ExecuteNonQuery();
                        }
                        else
                        {
                            //正常数量 整机在线状态表最近工步的正常数量加1
                          //  mcmd.CommandText = "update lo_t_wcsl set zcsl=isnull(zcsl,0)+1,gh='" + gh + "' where jhbh='" + m_jhbh + "' and cjd=" + m_device + " and gb=" + gmbh;
                            mcmd.CommandText = "update lo_t_wcsl set zcsl=isnull(zcsl,0)+1,gh='" + gh + "' where jhbh='" + m_jhbh + "' and cjd=" + m_device + " and gb=" + mlatgb;
                            mcmd.ExecuteNonQuery();
                           
                        }
                    }
                    else
                    {
                        //如果整机在线状态表没有记录，则mlatgb为0
                        if (mlatgb != 0)
                        {
                            if (mfzzt == 0)
                            {
                                mcmd.CommandText = "insert into lo_t_wcsl(jhbh,cjd,gb,fgsl,gh) values(@p1,@p2,@p3,@p4,@p5)";
                                mcmd.Parameters.Add("@p1", SqlDbType.Char, 30).Value = m_jhbh;
                                mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = m_device;
                                mcmd.Parameters.Add("@p3", SqlDbType.Float).Value = mlatgb;
                                mcmd.Parameters.Add("@p4", SqlDbType.Float).Value = 1;
                                mcmd.Parameters.Add("@p5", SqlDbType.Char, 30).Value = gh;
                                mcmd.ExecuteNonQuery();
                            }
                            else
                            {
                                mcmd.CommandText = "insert into lo_t_wcsl(jhbh,cjd,gb,zcsl,gh) values(@p1,@p2,@p3,@p4,@p5)";
                                mcmd.Parameters.Add("@p1", SqlDbType.Char, 30).Value = m_jhbh;
                                mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = m_device;
                                mcmd.Parameters.Add("@p3", SqlDbType.Float).Value = mlatgb;
                                mcmd.Parameters.Add("@p4", SqlDbType.Float).Value = 1;
                                mcmd.Parameters.Add("@p5", SqlDbType.Char, 30).Value = gh;
                                mcmd.ExecuteNonQuery();
                            }
                        }
                    }
                    //现场工时记录表操作 2 工步不一致才执行
                    mcmd.Parameters.Clear();
                    if ((barcodeisexist == 2) || (barcodeisexist == 0))
                    {

                        mcmd.CommandText = "update lo_t_xcgsjlb set wcsj='" + mdatetime + "',gh='" + gh + "' where id=" + maxid;
                        mcmd.ExecuteNonQuery();
                        mcmd.Parameters.Clear();
                        mcmd.CommandText = "update lo_t_xcgsjlb set hs=datediff(minute,kssj,wcsj) where id="+maxid+" and (kssj is not null) and (wcsj is not null)";
                        mcmd.ExecuteNonQuery();

                        mcmd.Parameters.Clear();
                        mcmd.CommandText = "insert into lo_t_xcgsjlb(zjtm,gb,kssj,gh) values(@p1,@p2,@p3,@p4)";
                        mcmd.Parameters.Add("@p1", SqlDbType.Char, 30).Value = barcode;
                        mcmd.Parameters.Add("@p2", SqlDbType.Float).Value = gmbh;
                        mcmd.Parameters.Add("@p3", SqlDbType.DateTime).Value = mdatetime;
                        mcmd.Parameters.Add("@p4", SqlDbType.Char, 30).Value = gh;
                        mcmd.ExecuteNonQuery();
                    }
                    //判断是否成品入库
                    if (dqgbmc == "成品入库")
                    {
                        mcmd.Parameters.Clear();
                        mcmd.CommandText = "delete from lo_t_zjzxztb where zjtm='"+barcode+"' and zt=0";
                        mcmd.ExecuteNonQuery();
                    }
                }
                mtrans.Commit();
                sresult = 1;
            }
            catch
            {
                mtrans.Rollback();
                sresult = 0;
            }
            mdata.closeconnect();
            return sresult;
        }
        
        public bool chkgh(string m_gh)
        {
            mdata.openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = mdata.m_sqlconnect;
                mcmd.CommandType = CommandType.Text;
                mcmd.CommandText = "select f1 from hremployee where f1='"+m_gh+"'";
                using (SqlDataReader mreader = mcmd.ExecuteReader())
                {
                    if (mreader.Read())
                    {
                        mdata.closeconnect();
                        return true;
                    }
                    else
                    {
                        mdata.closeconnect();
                        return false;
                    }
                }
            }
            mdata.closeconnect();
            return false;
        }

        public short chkgylxisexist(string barcode, double gxbh)
        {
            string cpdm=weblist.getcpdm(barcode);
            string msql = "select cpdm from lo_t_cpgylxb where cpdm='" + cpdm + "' and gxmc=" + gxbh;
            DataSet mdataset = mdata.getdataset(msql);
            if (mdataset.Tables[0].Rows.Count > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public DateTime getserdate()
        {
            DateTime mdate = System.DateTime.Now;
            mdata.openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = mdata.m_sqlconnect;
                mcmd.CommandType = CommandType.Text;
                mcmd.CommandText = "select getdate() as nowday";
                using (SqlDataReader mreader = mcmd.ExecuteReader())
                {
                    if (mreader.Read())
                    {
                        mdate = Convert.ToDateTime(mreader["nowday"]);                        
                    }                   
                }
            }
            mdata.closeconnect();
            return mdate;
        }

        //判断整机条码是否扫描2次,0:整机在线状态表不存在，1：整机状态在线表已存在当前工步,2:整机在线表存在条码，不存在当前工步
        public short chkbarcode(string barcode,double gbbh)
        {
            short mresult =0;
            double mzjgb = -1.0;
            mdata.openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = mdata.m_sqlconnect;
                mcmd.CommandType = CommandType.Text;
                mcmd.CommandText = "select zjgb from lo_t_zjzxztb where zjtm='" + barcode + "'";
                using (SqlDataReader mreader = mcmd.ExecuteReader())
                {
                    if (mreader.Read())
                    {
                        mzjgb = Convert.ToDouble(mreader["zjgb"]);
                        mresult = -1;
                    }
                    else
                    {
                        mresult = 0;
                    }
                }
            }
            mdata.closeconnect();
            if (mresult ==-1)
            {
                if (mzjgb == gbbh)
                {
                    mresult = 1;
                }
                else
                {
                    mresult = 2;
                }
            }
            return mresult;
        }

        public double getcjdbh(string macadd)
        {
            double mbh = -1.0;
            mdata.openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = mdata.m_sqlconnect;
                mcmd.CommandType = CommandType.Text;
                mcmd.CommandText = "select ID from lo_t_cjdgl where mac='" + macadd + "'";
                using (SqlDataReader mreader = mcmd.ExecuteReader())
                {
                    if (mreader.Read())
                    {
                        mbh = Convert.ToDouble(mreader["ID"]);
                        mdata.closeconnect();
                        return mbh;
                    }
                    else
                    {
                        mdata.closeconnect();
                        return mbh;
                    }
                }
            }
            mdata.closeconnect();
        }

        public double getcjdqybm(string macadd)
        {
            double mbh = -1.0;
            mdata.openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = mdata.m_sqlconnect;
                mcmd.CommandType = CommandType.Text;
                mcmd.CommandText = "select qybm from lo_t_cjdgl where mac='" + macadd + "'";
                using (SqlDataReader mreader = mcmd.ExecuteReader())
                {
                    if (mreader.Read())
                    {
                        mbh = Convert.ToDouble(mreader["qybm"]);
                        mdata.closeconnect();
                        return mbh;
                    }
                    else
                    {
                        mdata.closeconnect();
                        return mbh;
                    }
                }
            }
            mdata.closeconnect();
        }

        public string getqyxinxi(double qydm)
        {
            string dispxx = "";
            mdata.openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = mdata.m_sqlconnect;
                mcmd.CommandType = CommandType.Text;
                mcmd.CommandText = "select b.gxmc,a.qywzms from lo_t_qybmb a inner join lo_t_gxb b on a.gxmc=b.id where a.id=" + qydm;
                using (SqlDataReader mreader = mcmd.ExecuteReader())
                {
                    if (mreader.Read())
                    {
                        dispxx = "当前工序:" + mreader["gxmc"].ToString().Trim() + "   当前区域:" + mreader["qywzms"].ToString().Trim();
                        mdata.closeconnect();
                        return dispxx;
                    }
                    else
                    {
                        mdata.closeconnect();
                        return dispxx;
                    }
                }
            }
            mdata.closeconnect();
            
        }

        // 判断条码属性 正常 返工
        public short  chk_fgzt(string barcode)
        {
            short m_result = 1;
            short m_currxh = -1;
            short m_revxh = -1;
            m_revxh = getrevgxmc(barcode);
            m_currxh = getcurrgxmc(barcode,m_qybm);
            if (m_currxh < m_revxh)
            {
                m_result = 0;
            }          
            return m_result;
        }
        //获取区域编码的工序序列号
        public short getcurrgxmc(string barcode,double gbbh)
        {
            short mxh = -1;   
            string cpdm = weblist.getcpdm(barcode);         
            mdata.openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = mdata.m_sqlconnect;
                mcmd.CommandText = "select a.xh from lo_t_cpgylxb a inner join (select gxmc from lo_t_qybmb where id=" + gbbh + ") b on a.gxmc=b.gxmc where a.cpdm='" + cpdm + "'";
                using (SqlDataReader psread = mcmd.ExecuteReader())
                {
                    if (psread.Read())
                    {
                       mxh = Convert.ToInt16(psread["xh"]);
                    }
                }                
            }
            mdata.closeconnect();
            return mxh;
        }

        public short getrevgxmc(string barcode)
        {
            short mxh = -1;
            string cpdm = weblist.getcpdm(barcode);

            //获取整机在线状态表工步对应工序的顺序号
            mdata.openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = mdata.m_sqlconnect;
                mcmd.CommandType = CommandType.Text;
                mcmd.CommandText = "select c.xh from lo_t_cpgylxb c inner join (select a.gxmc from lo_t_gbb a inner join (select zjgb from lo_t_zjzxztb where zjtm='" + barcode + "') b on a.ID=b.zjgb) d on c.gxmc=d.gxmc where c.cpdm='" + cpdm + "'";
                using (SqlDataReader msread = mcmd.ExecuteReader())
                {
                    if (msread.Read())
                    {
                        mxh = Convert.ToInt16(msread["xh"]);
                    }
                }
            }
            mdata.closeconnect();
            return mxh;
        }

        //获取工步对应工序的序列号
        public short getgbgxxh(string barcode, double gbbh)
        {
            short mxh = -1;
            string cpdm = weblist.getcpdm(barcode);
            mdata.openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = mdata.m_sqlconnect;
                mcmd.CommandText = "select a.xh from lo_t_cpgylxb a inner join (select gxmc from lo_t_gbb where id=" + gbbh + ") b on a.gxmc=b.gxmc where a.cpdm='" + cpdm + "'";
                using (SqlDataReader psread = mcmd.ExecuteReader())
                {
                    if (psread.Read())
                    {
                        mxh = Convert.ToInt16(psread["xh"]);
                    }
                }
            }
            mdata.closeconnect();
            return mxh;
        }

        public short chkisfgzt(string barcode,double gbbh)
        {
            short mresult = 0;
            double mtfgb = -1.0;         
            mdata.openconnect();
            using (SqlCommand mcmd = new SqlCommand())
            {
                mcmd.Connection = mdata.m_sqlconnect;
                mcmd.CommandText = "select tfgb from lo_t_zjzxztb where zjtm='"+barcode+"' and zt=1";
                using (SqlDataReader msread = mcmd.ExecuteReader())
                {
                    if (msread.Read())
                    {
                        mtfgb = Convert.ToDouble(msread["tfgb"]);
                    }
                }               
            }
            mdata.closeconnect();
            if (mtfgb == -1.0)
            {
                return mresult;
            }
            //退返工步工序序号
            short mxh1 = getgbgxxh(barcode, mtfgb);
            //当前工步工序序号
            short mxh2 = getgbgxxh(barcode, gbbh);

            if (mxh2 > mxh1)
            {
                mresult = 1;
            }
            else
            {
                mresult = 2;
            }
            return mresult;
        }
    }
}

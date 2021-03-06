﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Song.ServiceInterfaces;
using Song.Entities;
using WeiSha.WebControl;
using System.Xml;
using System.Runtime.InteropServices;
using WeiSha.Common;
namespace Song.Site.Ajax
{
    /// <summary>
    /// InResult._ 的摘要说明
    /// </summary>
    public class InResult : IHttpHandler {
        //获取答题结果
        private string result = WeiSha.Common.Request.Form["result"].String;
        //答题结果的xml对象
        private XmlDocument resXml = new XmlDocument();
        public HttpResponse Response;
        public HttpRequest Request;
        [DllImport("Iphlpapi.dll")]
        private static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 length);
        [DllImport("Ws2_32.dll")]
        private static extern Int32 inet_addr(string ip);
        public void ProcessRequest (HttpContext context) {
            context.Response.ContentType = "text/plain";
            Response = context.Response;
            Request = context.Request;
            Response.ContentType = "text/xml";
            Response.Flush();
            Response.Buffer = true;
            Response.ExpiresAbsolute = DateTime.Now.AddHours(-1);
            Response.Expires = 0;
            Response.CacheControl = "no-cache";
            result =context. Server.UrlDecode(result);
            this.Response.Write(initData());
            Response.End();
        }
        /// <summary>
        /// 初始化常用数据
        /// </summary>
        /// <returns>如果有错误，返回-1，如果考试结束或不存在，则返回0；正常返回1</returns>
        private int initData()
        {
            //如果为空，则返回-1，表示错误
            if (result == "") return -1;
            resXml.LoadXml(result, false);
            XmlNode xn = resXml.SelectSingleNode("results");
            //试卷id，考试id
            int tpid;
            int.TryParse(xn.Attributes["tpid"].Value, out tpid);
            int examid;
            int.TryParse(xn.Attributes["examid"].Value, out examid);
            //考试结束时间
            long lover;
            long.TryParse(xn.Attributes["overtime"].Value, out lover);
            lover = lover * 10000;
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            TimeSpan toNow = new TimeSpan(lover);
            DateTime overTime = dtStart.Add(toNow); //得到转换后的时间
            //学生Id,学生名称
            int stid;
            int.TryParse(xn.Attributes["stid"].Value, out stid);
            string stname = xn.Attributes["stname"].Value.ToString();
            //学生性别，分组，身份证号
            int stsex;
            int.TryParse(xn.Attributes["stsex"].Value, out stsex);
            int stsid;
            int.TryParse(xn.Attributes["stsid"].Value, out stsid);
            string stcardid = xn.Attributes["stcardid"].Value.ToString();
            //学科Id,学科名称
            int sbjid;
            int.TryParse(xn.Attributes["sbjid"].Value, out sbjid);
            string sbjname = xn.Attributes["sbjname"].Value.ToString();
            //UID与考试主题
            string uid = xn.Attributes["uid"].Value.ToString();
            string theme = xn.Attributes["theme"].Value.ToString();
            //提交方式，1为自动提交，2为交卷
            int patter = Convert.ToInt32(xn.Attributes["patter"].Value);
            //
            Song.Entities.Examination exam = Business.Do<IExamination>().ExamSingle(examid);
            //如果考试不存在
            if (exam == null) return 0;
            //如果考试已经结束
            int span = (int)exam.Exam_Span;
            //if (DateTime.Now > ((DateTime)exam.Exam_Date).AddMinutes(span + 5)) return 0;          
            try
            {
                Song.Entities.ExamResults exr = new ExamResults();
                exr.Exr_IsSubmit = patter == 2;
                exr.Exam_ID = examid;
                exr.Exam_Name = exam.Exam_Name;
                exr.Tp_Id = tpid;
                exr.Ac_ID = stid;
                exr.Ac_Name = stname;
                exr.Sts_ID = stsid;
                exr.Ac_Sex = stsex;
                exr.Ac_IDCardNumber = stcardid;
                exr.Sbj_ID = sbjid;
                exr.Sbj_Name = sbjname;                
                exr.Exr_IP = WeiSha.Common.Browser.IP;
                exr.Exr_Mac = getMac();
                exr.Exr_Results = result;
                exr.Exam_UID = uid;
                exr.Exam_Title = theme;
                exr.Exr_IsSubmit = patter == 2;
                if (exr.Exr_IsSubmit) exr.Exr_SubmitTime = DateTime.Now;
                exr.Exr_OverTime = overTime;
                exr.Exr_CrtTime = DateTime.Now;
                exr.Exr_LastTime = DateTime.Now;
                //Business.Do<IExamination>().ResultSubmit(exr);
                string cacheUid = string.Format("ExamResults：{0}-{1}-{2}", examid, tpid, stid);    //缓存的uid
                Business.Do<IQuestions>().CacheUpdate(exr, -1, cacheUid); 
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// 获取Mac地址
        /// </summary>
        /// <returns></returns>
        private string getMac()
        {
            try
            {
                string userip = Request.UserHostAddress;
                string strClientIP = Request.UserHostAddress.ToString().Trim();
                Int32 ldest = inet_addr(strClientIP); //目的地的ip 
                Int32 lhost = inet_addr("");   //本地服务器的ip 
                Int64 macinfo = new Int64();
                Int32 len = 6;
                int res = SendARP(ldest, 0, ref macinfo, ref len);
                string mac_src = macinfo.ToString("X");
                while (mac_src.Length < 12) mac_src = mac_src.Insert(0, "0");
                string mac_dest = "";
                for (int i = 0; i < 11; i++)
                {
                    if (0 == (i % 2))
                        mac_dest = i == 10 ? mac_dest.Insert(0, mac_src.Substring(i, 2)) : "-" + mac_dest.Insert(0, mac_src.Substring(i, 2));
                }
                return mac_dest;
            }
            catch
            {
                return "";
            }
        }
        public bool IsReusable {
            get {
                return false;
            }
        }
    }
}
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using WeiSha.Common;

using Song.ServiceInterfaces;
using Song.Entities;
using WeiSha.WebControl;

namespace Song.Site.Manage.Exam
{
    public partial class Examination : Extend.CustomPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            this.Form.DefaultButton = this.btnSear.UniqueID;
            if (!IsPostBack)
            {
                 BindData(null, null);
            }
        }
        /// <summary>
        /// ���б�
        /// </summary>
        protected void BindData(object sender, EventArgs e)
        {
            
            //�ܼ�¼��
            int count = 0;
            Song.Entities.Examination[] eas = null;
            Song.Entities.Organization org = Business.Do<IOrganization>().OrganCurrent();
            //ʱ������
            int spanType = Convert.ToInt32(ddlTime.SelectedValue);
            DateTime? start = null;
            DateTime? end = null;
            //����ʱ��
            if (spanType == -1)
            {
                eas = Business.Do<IExamination>().GetPager(org.Org_ID,null, null, null, this.tbSear.Text, Pager1.Size, Pager1.Index, out count);
            }
            else
            {
                //����
                if (spanType == 1)
                {
                    start = (DateTime?)DateTime.Now.Date;
                    end = (DateTime?)DateTime.Now.AddDays(1).Date;
                }
                //����
                if (spanType == 2)
                {
                    DateTime week = (DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek + 1)).Date;
                    start = (DateTime?)week;
                    end = (DateTime?)week.AddDays(7);
                }
                //����
                if (spanType == 3)
                {
                    DateTime month = (DateTime.Now.AddDays(-DateTime.Now.Day + 1)).Date;
                    start = (DateTime?)month;
                    end = (DateTime?)month.AddMonths(1);
                }
                eas = Business.Do<IExamination>().GetPager(org.Org_ID, start, end, null, this.tbSear.Text, Pager1.Size, Pager1.Index, out count);
            }
            GridView1.DataSource = eas;
            GridView1.DataKeyNames = new string[] { "Exam_ID" };
            GridView1.DataBind();

            Pager1.RecordAmount = count;
        }
        protected void btnsear_Click(object sender, EventArgs e)
        {
            Pager1.Index = 1;
            BindData(null, null);
        }
        /// <summary>
        /// ɾ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DeleteEvent(object sender, EventArgs e)
        {
            try
            {
                string keys = GridView1.GetKeyValues;
                foreach (string id in keys.Split(','))
                {
                    Business.Do<IExamination>().ExamDelete(Convert.ToInt16(id));
                }
                BindData(null, null);
            }
            catch (Exception ex)
            {
                Message.ExceptionShow(ex);
            } 
        }
        /// <summary>
        /// ����ɾ��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnDel_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                WeiSha.WebControl.RowDelete img = (WeiSha.WebControl.RowDelete)sender;
                int index = ((GridViewRow)(img.Parent.Parent)).RowIndex;
                int id = int.Parse(this.GridView1.DataKeys[index].Value.ToString());
                Business.Do<IExamination>().ExamDelete(id);
                BindData(null, null);
            }
            catch (Exception ex)
            {
                Message.ExceptionShow(ex);
            } 
        }
        /// <summary>
        /// �޸��Ƿ���ʾ��״̬
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void sbUse_Click(object sender, EventArgs e)
        {
            try
            {
                StateButton ub = (StateButton)sender;
                int index = ((GridViewRow)(ub.Parent.Parent)).RowIndex;
                int id = int.Parse(this.GridView1.DataKeys[index].Value.ToString());
                //
                Song.Entities.Examination entity = Business.Do<IExamination>().ExamSingle(id);
                entity.Exam_IsUse = !entity.Exam_IsUse;
                Business.Do<IExamination>().ExamSave(entity);
                BindData(null, null);
            }
            catch (Exception ex)
            {
                this.Alert(ex.Message);
            } 
        }
        /// <summary>
        /// ��ȡ�ο���Ա����
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected string getGroupType(string gtype,string uid)
        {
            int type = Convert.ToInt32(gtype);
            if (type == 1) return "ȫ��ѧ��";
            if (type == 2)
            {
                Song.Entities.StudentSort[] sts = Business.Do<IExamination>().GroupForStudentSort(uid);
                string strDep = "";
                for (int i = 0; i < sts.Length; i++)
                {
                    strDep += sts[i].Sts_Name;
                    if (i < sts.Length - 1) strDep += ",";
                }
                return strDep;
            }
            
            return "";
        }
    }
}

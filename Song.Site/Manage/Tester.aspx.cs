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
using System.Collections.Generic;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System.IO;
using WeiSha.Common;

using Song.ServiceInterfaces;
using Song.Entities;
using WeiSha.WebControl;
using System.Drawing;
using System.Reflection;
using System.Xml.Serialization;
using Spring.Globalization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Xml;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Song.Site.Manage
{
    public partial class Tester : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            //ThreadStart threadStart = new ThreadStart(Calculate);//ͨ��ThreadStartί�и������߳�ִ��ʲô��������

            //Thread thread = new Thread(threadStart);

            //thread.Start();//�������߳�


        }
        public string build_Qrcode()
        {
            //��ά��ͼƬ����
            System.Drawing.Image image = null;
            string url = "΢��";
            string domain = this.Request.Url.Scheme + "://" + this.Request.Url.Host + ":" + this.Request.Url.Port;
            url = string.Format(url, domain, "", "");
            image = WeiSha.Common.QrcodeHepler.Encode(url, 15, "#000", "#fff");

            //image = Rounded(image, null);
            //��imageתΪbase64
            string base64 = WeiSha.Common.Images.ImageTo.ToBase64(image);
            return "data:image/JPG;base64," + base64;
        }
        public static System.Drawing.Image Rounded(System.Drawing.Image image, string sCornerLocation)
        {            
            Graphics g = Graphics.FromImage(image);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
            GraphicsPath rectPath = CreateRoundRectanglePath(rect, image.Width / 2, sCornerLocation); //����Բ���ⲿ·��

            //��ͼ��
            return BitmapCrop((Bitmap)image, rectPath);
        }
        //���� Բ��ͼƬ�ķ���
        #region ����������˵��

        private static GraphicsPath CreateRoundRectanglePath(Rectangle rect, int radius, string sPosition)
        {
            GraphicsPath rectPath = new GraphicsPath();
            switch (sPosition)
            {
                case "TopLeft":
                    {
                        rectPath.AddArc(rect.Left, rect.Top, radius * 2, radius * 2, 180, 90);
                        rectPath.AddLine(rect.Left, rect.Top, rect.Left, rect.Top + radius);
                        break;
                    }

                case "TopRight":
                    {
                        rectPath.AddArc(rect.Right - radius * 2, rect.Top, radius * 2, radius * 2, 270, 90);
                        rectPath.AddLine(rect.Right, rect.Top, rect.Right - radius, rect.Top);
                        break;
                    }

                case "BottomLeft":
                    {
                        rectPath.AddArc(rect.Left, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        rectPath.AddLine(rect.Left, rect.Bottom - radius, rect.Left, rect.Bottom);
                        break;
                    }

                case "BottomRight":
                    {
                        rectPath.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        rectPath.AddLine(rect.Right - radius, rect.Bottom, rect.Right, rect.Bottom);
                        break;
                    }
                default:
                    {
                        rectPath.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                        rectPath.AddLine(rect.X + radius, rect.Y, rect.Right - radius * 2, rect.Y);
                        rectPath.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                        rectPath.AddLine(rect.Right, rect.Y + radius * 2, rect.Right, rect.Y + rect.Height - radius * 2);
                        rectPath.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                        rectPath.AddLine(rect.Right - radius * 2, rect.Bottom, rect.X + radius * 2, rect.Bottom);
                        rectPath.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        rectPath.AddLine(rect.X, rect.Bottom - radius * 2, rect.X, rect.Y + radius * 2);
                        //rectPath.CloseFigure();
                        break;
                    }

            }
            return rectPath;
        }

#endregion


        /// <summary>
        /// ͼƬ�������ͼ
        /// </summary>
        /// <param name="bitmap">ԭͼ</param>
        /// <param name="path">�ü�·��</param>
        /// <returns></returns>
        public static System.Drawing.Image BitmapCrop(Bitmap bitmap, GraphicsPath path)
        {
            RectangleF rect = path.GetBounds();
            int left = (int)rect.Left;
            int top = (int)rect.Top;
            int width = (int)rect.Width;
            int height = (int)rect.Height;
            Bitmap image = (Bitmap)bitmap.Clone();
            Bitmap outputBitmap = new Bitmap(width, height);
            for (int i = left; i < left + width; i++)
            {
                for (int j = top; j < top + height; j++)
                {
                    //�ж������Ƿ���·����   
                    if (path.IsVisible(i, j))
                    {
                        //����ԭͼ��������ص����ͼƬ   
                        outputBitmap.SetPixel(i - left, j - top, image.GetPixel(i, j));
                        //����ԭͼ�ⲿ������Ϊ͸��   
                        image.SetPixel(i, j, Color.FromArgb(0, image.GetPixel(i, j)));
                    }
                    else
                    {
                        outputBitmap.SetPixel(i - left, j - top, Color.FromArgb(0, 255, 255, 255));
                    }
                }
            }
            bitmap.Dispose();
            return (System.Drawing.Image)outputBitmap;
        }
    }
}

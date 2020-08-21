using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Tool
{
    public class GotionAPI
    {

        private string _code;
        private string _cookieHeader;
        private string _pageContent;

        public string Code
        {
            get
            {
                return this._code;
            }
            set
            {
                this._code = value;
            }
        }

        public string CookieHeader
        {
            get
            {
                return this._cookieHeader;
            }
            set
            {
                this._cookieHeader = value;
            }
        }



        public string PageContent
        {
            get
            {
                return this._pageContent;
            }
            set
            {
                this._pageContent = value;
            }
        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="strURL">URL</param>
        /// <param name="strArgs">用户登录数据</param>
        /// <param name="strReferer">引用地址</param>
        /// <param name="code">UTF-8</param>
        /// <param name="method">POST</param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public string PostData(string strURL, string strArgs, string strReferer, string code, string method, string contentType)
        {
            try
            {
                string str = string.Empty;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);
                request.AllowAutoRedirect = true;
                request.KeepAlive = true;
                request.Accept = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/vnd.ms-excel, application/msword, application/x-shockwave-flash, */*";
                request.Referer = strReferer;
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; Maxthon; .NET CLR 2.0.50727)";
                if (string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = "application/x-www-form-urlencoded";
                }
                else
                {
                    request.ContentType = "contentType";
                }
                request.Method = method;
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                if (request.CookieContainer == null)
                {
                    request.CookieContainer = new CookieContainer();
                }
                if (this.CookieHeader.Length > 0)
                {
                    request.Headers.Add("cookie:" + this.CookieHeader);
                    request.CookieContainer.SetCookies(new Uri(strURL), this.CookieHeader);
                }
                byte[] bytes = Encoding.GetEncoding(code).GetBytes(strArgs);
                request.ContentLength = bytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                requestStream.Dispose();
                HttpWebResponse response = null;
                StreamReader reader = null;
                response = (HttpWebResponse)request.GetResponse();
                if (request.CookieContainer != null)
                {
                    this.CookieHeader = request.CookieContainer.GetCookieHeader(new Uri(strURL));
                }
                reader = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(code));
                str = reader.ReadToEnd();
                reader.Close();
                reader.Dispose();
                response.Close();
                return str;
            }
            catch (Exception exception)
            {
                return exception.ToString();
            }
        }


        public string Post(string url, string postData)
        {

            //return Post(url, postData, "application/x-www-form-urlencoded");
            return Post(url, postData, "application/json");
        }

        public string Post(string url, string postData, string contenttype)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = contenttype;
            request.Method = "POST";
            request.Timeout = 300000;

            byte[] bytes = Encoding.UTF8.GetBytes(postData);
            request.ContentLength = bytes.Length;
            Stream write = request.GetRequestStream();
            write.Write(bytes, 0, bytes.Length);
            write.Close();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream() ?? throw new InvalidOperationException(), Encoding.UTF8);
            string result = reader.ReadToEnd();
            response.Close();
            return result;
        }
    }
}

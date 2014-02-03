using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoWeb
{
    public class AutoCheckMoney : IAutoWeb
    {
        private static string URL_LOGIN2ASSET = "https://login.taobao.com/member/login.jhtml?style=alipay&goto=https%3A%2F%2Ffinanceprod.alipay.com%2Ffund%2Fasset.htm";
        private static string URL_LOGIN = "https://login.taobao.com/member/login.jhtml";
        private static string URL_ASSET = "https://financeprod.alipay.com/fund/asset.htm";
        private static string URL_MIAO = "http://ka.tmall.com/";
        private static string URL_ETAO = "http://www.etao.com/";

        private bool _isTotalUpdated = false;
        private bool _completed = false;
        private string _message = string.Empty;
        
        bool IAutoWeb.Completed
        {
            get
            {
                return this._completed;
            }
        }

        string IAutoWeb.Message
        {
            get
            {
                return this._message;
            }
        }

        void IAutoWeb.Start(WebBrowser webBrowser1)
        {
            this._message = "Started";
            webBrowser1.Navigate(URL_LOGIN2ASSET);
        }

        void IAutoWeb.Next(WebBrowser webBrowser1)
        {
            Uri url = webBrowser1.Url;
            // Fix bug: Why sometimes it's null?
            if(url == null)
            {
                return;
            }

            Console.Out.WriteLine("Next>>");
            Console.Out.WriteLine(url);

            if (url.ToString().StartsWith(URL_LOGIN))
            {
                this.LoginSupportedByWangWang(webBrowser1.Document);
            }
            else if (url.ToString().StartsWith(URL_ASSET))
            {
                MoneyDatabaseUpdater dbUpdater = new MoneyDatabaseUpdater();
                if (this._isTotalUpdated == false)
                {
                    this._isTotalUpdated = this.RetrieveTotal(webBrowser1.Document, dbUpdater);
                }

                if (this.RetrieveDaily(webBrowser1.Document, dbUpdater))
                {
                    //this._completed = true;
                    if (!this.ClickNextTab(webBrowser1.Document))
                    {
                        webBrowser1.Navigate(URL_MIAO);
                    }
                }
            }
            else if (url.ToString().StartsWith(URL_MIAO))
            {
                if (this.GetMaoquan(webBrowser1.Document))
                {
                    webBrowser1.Navigate(URL_ETAO);
                }
            }
            else if (url.ToString().StartsWith(URL_ETAO))
            {
                if (this.SignETao(webBrowser1.Document))
                {
                    this._completed = true;
                }
            }
            else
            {

            }
        }

        private void LoginWithNamePassword(HtmlDocument doc)
        {
            HtmlElement ele;
            ele = doc.GetElementById("TPL_username_1");
            ele.SetAttribute("value", "username");
            ele = doc.GetElementById("TPL_password_1");
            ele.SetAttribute("value", "password");
            ele = doc.GetElementById("J_SubmitStatic");
            ele.InvokeMember("click");
        }

        private void LoginSupportedByWangWang(HtmlDocument doc)
        {
            HtmlElement ele1 = doc.GetElementById("J_Static2Quick");
            HtmlElement ele2 = doc.GetElementById("J_SubmitQuick");
            HtmlElement ele3 = doc.GetElementById("J_OtherAccountV"); //使用其他账户登录
            HtmlElement ele4 = doc.GetElementById("ra-0");

            string ss = string.Empty;
            ss += ele1 == null ? "false, " : "J_Static2Quick, ";
            ss += ele2 == null ? "false, " : "J_SubmitQuick, ";
            ss += ele3 == null ? "false, " : "J_OtherAccountV, ";
            ss += ele4 == null ? "false" : "ra-0";
            Console.Out.WriteLine(ss);

            HtmlElement ele = null;
            if (ele2 == null && ele3 == null)
            {
                ele = ele1;
            }
            else if (ele1 == null && ele3 == null)
            {
                ele = ele2;

                // Although WebBrowser.DocumentCompleted event risen
                // The entire page is not ready, maybe because of some AJAX content
                // Ignore in this case
                if(ele4 == null)
                {
                    ele = null;
                }
            }
            else if (ele1 == null && ele2 == null)
            {
                ele = ele3;
            }
            else
            {
                MessageBox.Show(ss);
            }

            if (ele != null)
            {
                ele.InvokeMember("click");
            }
        }

        private bool ClickNextTab(HtmlDocument doc)
        {
            string currentTab = string.Empty;
            foreach (HtmlElement ele in doc.GetElementsByTagName("li"))
            {
                if (ele.GetAttribute("classname").Contains("ui-tab-trigger-item  ui-tab-trigger-item-current"))
                {
                    currentTab = ele.Children[0].InnerText;
                    break;
                }
            }

            string nextTab = string.Empty;
            if (currentTab.Equals("收益"))
            {
                nextTab = "转入";
            }
            else if (currentTab.Equals("转入"))
            {
                nextTab = "转出";
            }
            else if (currentTab.Equals("转出"))
            {
                return false;
            }
            else
            {
                return false;
            }

            foreach (HtmlElement ele in doc.GetElementsByTagName("a"))
            {
                if (ele.InnerText != null &&
                    ele.GetAttribute("classname").Equals("ui-tab-trigger-text ui-bill-tab-text") &&
                    ele.InnerText.Equals(nextTab))
                {
                    ele.InvokeMember("click");
                    return true;
                }
            }

            return false;
        }

        private bool GetMaoquan(HtmlDocument doc)
        {
            foreach (HtmlElement ele in doc.GetElementsByTagName("div"))
            {
                if (ele.InnerText != null)
                {
                    if (ele.GetAttribute("classname").Equals("tc_kapindao cgShow") &&
                        !string.IsNullOrEmpty(ele.Style) && 
                        ele.Style.ToLower().Equals("display: block"))
                    {
                        return true;
                    }

                    if (ele.InnerText.Equals("天天签到领猫券"))
                    {
                        ele.InvokeMember("click");
                    }
                }
            }
            return false;
        }

        private bool SignETao(HtmlDocument doc)
        {
            HtmlElement eleBtn = null;

            foreach (HtmlElement ele in doc.GetElementsByTagName("div"))
            {
                if (ele.InnerText != null && 
                    ele.GetAttribute("classname").Equals("ci_receive") && 
                    ele.InnerText.Equals("签到") )
                {
                    eleBtn = ele;
                    break;
                }
            }

            if (eleBtn != null)
            {
                eleBtn.InvokeMember("click");
                return true;
            }

            return false;
        }

        private double GetAmountFromText(string amountText)
        {
            Match matchText = Regex.Match(amountText, @"(?<num>\d+(\.\d+))\s*元", RegexOptions.Singleline);
            return double.Parse(matchText.Groups["num"].Value);
        }

        private byte GetTypeFromText(string typeName)
        {
            if (typeName.Contains("收益"))
            {
                return 1;
            }
            else if (typeName.Contains("消费"))
            {
                return 2;
            }
            else if (typeName.Contains("单次转入"))
            {
                return 3;
            }
            else if (typeName.Contains("自动转入"))
            {
                return 4;
            }
            else if (typeName.Contains("转出至银行卡"))
            {
                return 5;
            }
            //else if (typeName.Contains("转入"))
            //{
            //    return 6;
            //}
            //else if (typeName.Contains("转出"))
            //{
            //    return 7;
            //}
            else
            {
                return 0;
            }
        }

        private bool RetrieveTotal(HtmlDocument doc, MoneyDatabaseUpdater dbUpdater)
        {
            double earn = 0;
            double left = 0;
            foreach (HtmlElement eleTd in doc.GetElementsByTagName("h3"))
            {
                string textTotal = eleTd.InnerText;
                if (eleTd.InnerText.Contains("总金额"))
                {
                    left = this.GetAmountFromText(eleTd.InnerText);
                }
                else if (eleTd.InnerText.Contains("历史累计收益"))
                {
                    earn = this.GetAmountFromText(eleTd.Parent.GetElementsByTagName("div")[0].InnerText);
                }
            }
            this._message = string.Format("今日\r\n余额：{0}元\r\n累计收益：{1}元", left, earn);

            dbUpdater.AddTotal(DateTime.Now, earn, left);
            return true;
        }

        private bool RetrieveDaily(HtmlDocument doc, MoneyDatabaseUpdater dbUpdater)
        {
            bool isRetrievedAny = false;

            foreach (HtmlElement eleTr in doc.GetElementsByTagName("tr"))
            {
                DateTime billTime = DateTime.Now;
                double amount = 0;
                byte changeType = 0;

                foreach(HtmlElement eleTd in eleTr.Children)
                {
                    string className = eleTd.GetAttribute("classname");
                    if (className.Contains("billTime"))
                    {
                        try
                        {
                            billTime = DateTime.Parse(eleTd.InnerText);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    else if (className.Contains("ft-bold"))
                    {
                        amount = this.GetAmountFromText(eleTd.InnerText);
                    }
                    else if (className.Contains("billAmount"))
                    {
                        changeType = this.GetTypeFromText(eleTd.InnerText);
                    }
                }

                if (amount != 0)
                {
                    Debug.Assert(changeType != 0, "未知的类型出现了！");
                    dbUpdater.AddDaily(billTime, amount, changeType);
                    isRetrievedAny = true;
                }
            }

            return isRetrievedAny;
        }
    }
}

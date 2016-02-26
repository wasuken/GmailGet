using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TKMP.Net;
using TKMP.Reader;
using Hardcodet.Wpf.TaskbarNotification;

namespace MailGet
{
    enum MAIL_TYPE { GMAIL,YMAIL}
    public class MailChecker
    {
        /*
        後要望としては　ログ管理とタイトルも取得したい。
        それは次回
        */

        //yahooの
        //private Mailbox[] yMails;
        //gmailの
        //private MailData[] gMails;
        //メールタイプ
        private MAIL_TYPE type;
        //最後に確認したメールの数
        private int mailCnt;

        public MailChecker(ImapClient imap)
        {
            if (imap.Connect())
            {
                if (imap.HostName.Contains("yahoo")) 
                {//Yahooメール処理
                    if (imap.GetMailBox() != null)
                    {
                        type = MAIL_TYPE.YMAIL;
                        mailCnt = yMailCntGet(imap.GetMailBox());
                    }
                    else MessageBox.Show("メールが存在しませぬ");
                }
                else
                {//GMail処理
                    if (imap.GetMailList() != null)
                    {
                        type = MAIL_TYPE.GMAIL;
                        mailCnt = imap.GetMailList().Count();
                    }
                    else MessageBox.Show("メールが存在しませぬ");
                }
            } else MessageBox.Show("失敗");
        }
        private int yMailCntGet(Mailbox[] boxs)
        {
            int ans=0;
            foreach(Mailbox box in boxs)
                ans += box.ExistsCount;
            return ans;
            
        }
        public void checkImap(ImapClient imap,TaskbarIcon icon)
        {
            if (imap.Connect())
            {
                int getMailCnt;
                if (type == MAIL_TYPE.GMAIL) getMailCnt = imap.GetMailList().Count();
                else getMailCnt = yMailCntGet(imap.GetMailBox());

                if (getMailCnt != mailCnt)
                {
                    if (getMailCnt > mailCnt)
                    {
                        int newCnt = getMailCnt - mailCnt;
                        icon.ShowBalloonTip("メールの通知", "新着メール " + newCnt + "件", BalloonIcon.Info);

                    }
                    mailCnt = getMailCnt;
                }
                //string msg = "getMailCnt:" + getMailCnt + "件\nmailCnt:" + mailCnt;
                //icon.ShowBalloonTip("メールの通知", msg, BalloonIcon.Info);
            }
            
        }

    }
}

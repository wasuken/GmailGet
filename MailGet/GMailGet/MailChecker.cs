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
using SpeechLib;

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
        SpeechLib.SpVoice VoiceSpeeach = null;

        public MailChecker(ImapClient imap)
        {
            
            if (imap.HostName.Contains("yahoo")) 
            {//Yahooメール処理
                if (imap.GetMailBox() != null)
                {
                    type = MAIL_TYPE.YMAIL;
                    mailCnt = imap.GetMailBox().Where(a => a.Name == "Inbox").Equals(null) ? 0 
                        : imap.GetMailBox().Where(a => a.Name == "Inbox").First().ExistsCount;
                }
                    
                    
            }
            else
            {//GMail処理
                if (imap.GetMailList() != null)
                {
                    type = MAIL_TYPE.GMAIL;
                    mailCnt = imap.GetMailList().Count();
                }
                    
            }

            
            

        }
        public void checkImap(ImapClient imap,TaskbarIcon icon)
        {
            
            int getMailCnt;
            string msg;
            if (type == MAIL_TYPE.GMAIL) getMailCnt = imap.GetMailList().Count();
            else getMailCnt = imap.GetMailBox().Where(a => a.Name == "Inbox").Equals(null)?0 
                    : imap.GetMailBox().Where(a => a.Name == "Inbox").First().ExistsCount;

            if (getMailCnt != mailCnt)
            {
                if (getMailCnt > mailCnt)
                {
                    int newCnt = getMailCnt - mailCnt;
                    msg = returnFirstMail(imap);
                    icon.ShowBalloonTip("メールの通知", msg, BalloonIcon.Info);
                }
                mailCnt = getMailCnt;
            }
            msg = "getMailCnt:" + getMailCnt + "件\nmailCnt:" + mailCnt;
            icon.ShowBalloonTip("メールの通知", msg, BalloonIcon.Info);
            
        }
        //はるかさんがインスコされてたら使える　　今のPCでは使えなかった(´・ω・｀)
        private void voiceSpeack()
        {
            //合成音声エンジンを初期化する.
            this.VoiceSpeeach = new SpeechLib.SpVoice();
            //合成音声エンジンで日本語を話す人を探す。(やらなくても動作はするけど、念のため)
            bool hit = false;
            foreach (SpObjectToken voiceperson in this.VoiceSpeeach.GetVoices())
            {
                string language = voiceperson.GetAttribute("Language");
                if (language == "411")
                {//日本語を話す人だ!
                    this.VoiceSpeeach.Voice = voiceperson; //君に読みあげて欲しい
                    hit = true;
                    break;
                }
            }
            if (!hit)
            {
                MessageBox.Show("日本語合成音声が利用できません。\r\n日本語合成音声 MSSpeech_TTS_ja-JP_Haruka をインストールしてください。\r\n");
            }

            this.VoiceSpeeach.Speak("テストだよ", SpeechVoiceSpeakFlags.SVSFlagsAsync | SpeechVoiceSpeakFlags.SVSFIsXML);

        }
        private  string returnFirstMail(ImapClient imap)
        {
            String msg = "";
            IMailData md;
            if (type == MAIL_TYPE.YMAIL)
            {
                Mailbox mb = imap.GetMailBox().Where(a => a.Name == "Inbox").First();
                MailData_Imap[] mdi = mb.MailDatas;

                md = mdi.Last();
            }
            else md = (IMailData)imap.GetMailList().Last();
                
            //Ansyncにしてみたが　他の部分がネックになってGUIが止まっている
            md.ReadBodyAnsync();
            md.ReadHeaderAnsync();
            
            // 本文無し( 本文が必要な場合は、false で、reader.MainText )
            MailReader reader = new MailReader(md.DataStream, false);

            msg += "送信元:" + reader.HeaderCollection.HeaderItem("From").Data +
                        "件名:" + reader.HeaderCollection.HeaderItem("Subject").Data;
            

            return msg;
        }
    }
}

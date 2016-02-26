using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TKMP.Net;
using TKMP.Reader;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;

namespace MailGet
{
    public struct RowItem
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public string MDate { get; set; }
        public string MText { get; set; }
    }
    [Serializable]
    public struct UserInfo
    {
        public string userName { get; set; }
        public string userPass { get; set; }
        public string srvName { get; set; }
    }

    [Serializable]
    public class AppInfo{
        public Dictionary<String, UserInfo> UserDict { get; set; }
    }


    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        CollectionViewSource view;
        ObservableCollection<RowItem> rowItems;

        private SynchronizationContext sc = null;
        private ImapClient imap = null;
        private int port = 993;
        private Dictionary<String, UserInfo> userDict;

        private static string pass = "iusegdhtjhkj:kkopjhgygfgh";

        


        public MainWindow()
        {
            InitializeComponent();
            textBox.IsReadOnly = true;

            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            sc = SynchronizationContext.Current;

            String[] items =   {"imap.gmail.com","imap.mail.yahoo.co.jp" };
            foreach (var item in items)
                combo_server.Items.Add(item);

            readConfig();

            
        }

        private void readConfig()
        {
            using (FileStream fs = returnFileStream(FileMode.OpenOrCreate))
            {

                userDict = new Dictionary<string, UserInfo>();
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();

                    if (fs.Length > 0)
                    {
                        AppInfo info = (AppInfo)bf.Deserialize(fs);

                        userDict = info.UserDict;


                        fs.Close();
                        combo_insert.Items.Clear();
                        foreach (var item in userDict.Keys)
                            combo_insert.Items.Add(PasswordManager.DecryptString(item, pass));
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace);
                }
            }
        }
        //ファイル周りの初期設定ができてない場合初期設定してくれる上に
        //userInfo.configファイルのFileStreamを返す
        private FileStream returnFileStream(FileMode file)
        {

            String cPath = Directory.GetCurrentDirectory();
            if (Directory.GetDirectories(@".", "files", SearchOption.AllDirectories).Count() == 0)
            {
                Directory.CreateDirectory("files");
            }
            return new FileStream(Directory.GetCurrentDirectory() + @"\files\userInfo.config", file);
        }

        //Yahooメール用
        public void YMailDL()
        {
            view = new CollectionViewSource();
            rowItems = new ObservableCollection<RowItem>();
            Mailbox[] mbs = imap.GetMailBox();
            String msg = "";
            foreach (Mailbox mb in mbs)
            {
                msg += mb.Name + "\n";
                MailData_Imap[] mdi = mb.MailDatas;
                
                
                foreach (MailData_Imap item in mdi)
                {
                    item.BodyLoaded += YMailData_BodyLoaded;
                    item.ReadBodyAnsync();
                    
                }

            }
            view.Source = rowItems;
            listView.DataContext = view;
        }
        private void YMailData_BodyLoaded(object sender, EventArgs e)
        {
            MailData_Imap MailData = (MailData_Imap)sender;
            MailData.ReadBody();
            MailData.ReadHeader();

            // 本文無し( 本文が必要な場合は、false で、reader.MainText )
            MailReader reader = new MailReader(MailData.DataStream, false);

            // UI スレッドへの処理( この場合、post_state は null )
            sc.Post((object post_state) =>
            {
                
                RowItem item;
                // ヘッダの一覧より、目的のヘッダを探す
                foreach (TKMP.Reader.Header.HeaderString headerdata in reader.HeaderCollection)
                {
                    
                    string from = reader.HeaderCollection.HeaderItem("From").Data;
                    string subject = reader.HeaderCollection.HeaderItem("Subject").Data;
                    string mdate = reader.HeaderCollection.HeaderItem("Date").Data;

                    item = new RowItem
                    {
                        From = from,
                        Subject = subject,
                        MDate = mdate,
                        MText = reader.MainText
                    };
                    rowItems.Remove(item);
                    rowItems.Insert(0, item);
                }
                // 行追加
            }, null);

            // イベント削除
            MailData.BodyLoaded -= new EventHandler(YMailData_BodyLoaded);
        }
        //Gmail用
        public void GmailDL()
        {
            view = new CollectionViewSource();
            rowItems = new ObservableCollection<RowItem>();


            // 非同期で全て表示
            foreach (var data in imap.GetMailList())
            {
                // 個別にイベント登録
                data.BodyLoaded += new EventHandler(GMailData_BodyLoaded);
                // 非同期処理開始
                data.ReadBodyAnsync();

            }

            view.Source = rowItems;
            listView.DataContext = view;

        }
        private void GMailData_BodyLoaded(object sender, EventArgs e)
        {
            IMailData MailData = (IMailData)sender;
            MailData.ReadBody();
            MailData.ReadHeader();

            // 本文無し( 本文が必要な場合は、false で、reader.MainText )
            MailReader reader = new MailReader(MailData.DataStream, false);

            // UI スレッドへの処理( この場合、post_state は null )
            sc.Post((object post_state) =>
            {

                RowItem item;
                // ヘッダの一覧より、目的のヘッダを探す
                foreach (TKMP.Reader.Header.HeaderString headerdata in reader.HeaderCollection)
                {
                    string from = reader.HeaderCollection.HeaderItem("From").Data;
                    string subject = reader.HeaderCollection.HeaderItem("Subject").Data;
                    string mdate = reader.HeaderCollection.HeaderItem("Date").Data;

                    item = new RowItem
                    {
                        From = from,
                        Subject = subject,
                        MDate = mdate,
                        MText = reader.MainText
                    };
                    rowItems.Remove(item);
                    rowItems.Insert(0, item);
                }

                // 行追加
            }, null);

            // イベント削除
            MailData.BodyLoaded -= new EventHandler(GMailData_BodyLoaded);
        }

        private void imap_CertificateValidation(object sender, TKMP.Net.CertificateValidationArgs e)
        {
            //全ての証明書を信用します
            e.Cancel = false;
        }
        

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 1)
            {
                RowItem item = (RowItem)e.AddedItems[0];
                textBox.Text = item.MText;
            }
        }
        private ImapClient returnImapClient()
        {
            String userName;
            String userPass;
            String srvName;
            if (radio_new.IsChecked.Value)
            {
                userName = textBox_userName.Text;
                userPass = textBox_Pass.Password;
                srvName = combo_server.Text;
            }
            else
            {
                userName = combo_insert.Text.Split('(')[0];
                UserInfo info = userDict[PasswordManager.EncryptString(combo_insert.Text, pass)];
                userPass = 
                    PasswordManager.DecryptString(info.userPass,pass);
                srvName = PasswordManager.DecryptString(info.srvName, pass);
            }
            

            BasicImapLogon logon = new BasicImapLogon(userName, userPass);

            ImapClient imap = new ImapClient(logon, srvName, port);
            //ＳＳＬを使用します
            imap.AuthenticationProtocol = TKMP.Net.AuthenticationProtocols.SSL;
            //証明書に問題があった場合に独自の処理を追加します
            imap.CertificateValidation += new CertificateValidationHandler(imap_CertificateValidation);

            return imap;

        }
        

        private void button_connect_Click(object sender, RoutedEventArgs e)
        {

            if (checkEmp()) return;
            

            /*
            //接続開始
            if (!imap.Connect())
            {
                MessageBox.Show("接続失敗");
                return;
            }
            else 

            if (imap.HostName.Contains("yahoo"))
            {//Yahooメール処理
                if (imap.GetMailBox() != null) YMailDL();
                else MessageBox.Show("メールが存在しませぬ");

            }
            else
            {//GMail処理
                if (imap.GetMailList() != null) GmailDL();
                else MessageBox.Show("メールが存在しませぬ");
            }
            */
            MailChecker mc = new MailChecker(returnImapClient());
            /*
                ３０秒ごとにメール数を比較する。
                増減により更新　減った場合は更新はするが無視、
                増えた場合は増えた分が伝わるように通知する。
                ・・・ImapClientをコンストラクタで渡す
            */
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 10);
            timer.Tick += (sender2, e2) => 
            {

                mc.checkImap(returnImapClient(), MyNotifyIcon);
            };
            timer.Start();
        }
        private void YmailMonitoring(ImapClient imap)
        {
            Mailbox mb = imap.GetMailBox()[imap.GetMailBox().Length - 1];

        }
        private bool checkEmp()
        {
            if ((string.IsNullOrWhiteSpace(combo_server.Text) || string.IsNullOrWhiteSpace(textBox_Pass.Password)
                || string.IsNullOrWhiteSpace(textBox_userName.Text)) && radio_new.IsChecked.Value)
            {
                MessageBox.Show("入力項目に空白があります");
                return true;
            }
            return false;
        }
        
        
        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            if (checkEmp()) return;

            StringBuilder key = new StringBuilder();
            key.AppendFormat(textBox_userName.Text + "({0})", combo_server.Text);
            if (userDict.ContainsKey(PasswordManager.EncryptString(key.ToString(), pass)))
            {
                MessageBox.Show("すでにそのアカウントは存在しています");
                return;
            }
            
            userDict.Add(PasswordManager.EncryptString(key.ToString(), pass),
                new UserInfo
                {
                    userName = PasswordManager.EncryptString(textBox_userName.Text,pass),
                    userPass = PasswordManager.EncryptString(textBox_Pass.Password,pass),
                    srvName = PasswordManager.EncryptString(combo_server.Text, pass)
                });
            configUpdate();
        }
        private void configUpdate()
        {

            using (FileStream fs = returnFileStream(FileMode.Create))
            {
                BinaryWriter bw = new BinaryWriter(fs);
                BinaryFormatter bf = new BinaryFormatter();

                AppInfo info = new AppInfo();
                info.UserDict = userDict;
                bf.Serialize(fs, info);

                fs.Close();
            }
            readConfig();
        }

        private void radio_new_Checked(object sender, RoutedEventArgs e)
        {
            changeInputEnable(true);
        }

        private void radio_old_Checked(object sender, RoutedEventArgs e)
        {
            changeInputEnable(false);
        }
        //radioボタン処理
        private void changeInputEnable(bool flg)
        {
            combo_insert.IsEnabled = !flg;

            combo_server.IsEnabled = flg;
            textBox_userName.IsEnabled = flg;
            textBox_Pass.IsEnabled = flg;
        }

        private void button_clear_Click(object sender, RoutedEventArgs e)
        {
            combo_server.Text = "";
            textBox_userName.Clear();
            textBox_Pass.Clear();
        }

        private void button_insert_delete_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(combo_insert.Text))
            {
                userDict.Remove(PasswordManager.EncryptString(combo_insert.Text,pass));
                configUpdate();
            }
            else MessageBox.Show("アカウントを選択してください");
        }
    }
}

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
using TKMP;
using TKMP.Net;
using TKMP.Reader;
using System.IO;
using System.Net;
using TKMP.Reader.Header;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;

namespace MailGet
{
    public struct RowItem
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public string MDate { get; set; }
        public string MText { get; set; }
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
        private BasicImapLogon logon;

        public MainWindow()
        {
            InitializeComponent();
            textBox.IsReadOnly = true;
            textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.Loaded += Window_Loaded;

            String[] items =   {"imap.gmail.com","imap.mail.yahoo.co.jp" };
            foreach (var item in items)
                combo_server.Items.Add(item);
        }

        public void mailDL()
        {
            view = new CollectionViewSource();
            rowItems = new ObservableCollection<RowItem>();


            // 非同期で全て表示
            foreach (var data in imap.GetMailList())
            {
                // 個別にイベント登録
                data.BodyLoaded += new EventHandler(MailData_BodyLoaded);
                // 非同期処理開始
                data.ReadBodyAnsync();

            }

            view.Source = rowItems;
            listView.DataContext = view;

        }
        public void mailDL2()
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
                    item.BodyLoaded += MailData_BodyLoaded2;
                    item.ReadBodyAnsync();
                    
                }

            }
            MessageBox.Show(msg);
            
            view.Source = rowItems;
            listView.DataContext = view;
        }
        private void MailData_BodyLoaded2(object sender, EventArgs e)
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
            MailData.BodyLoaded -= new EventHandler(MailData_BodyLoaded2);
        }
        private void MailData_BodyLoaded(object sender, EventArgs e)
        {
            IMailData MailData = (IMailData)sender;
            MailData.ReadBody();
            MailData.ReadHeader();

            // 本文無し( 本文が必要な場合は、false で、reader.MainText )
            MailReader reader = new MailReader(MailData.DataStream, false);

            // UI スレッドへの処理( この場合、post_state は null )
            sc.Post((object post_state) => {

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
            MailData.BodyLoaded -= new EventHandler(MailData_BodyLoaded);
        }

        // UI スレッドへの処理用
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            sc = SynchronizationContext.Current;
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

        private void button_connect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(combo_server.Text) || string.IsNullOrWhiteSpace(textBox_Pass.Password)
                || string.IsNullOrWhiteSpace(textBox_port.Text) || string.IsNullOrWhiteSpace(textBox_userName.Text))
            {
                MessageBox.Show("入力項目に空白があります");
                return;
            }
                
            //imap
            logon = new BasicImapLogon(textBox_userName.Text, textBox_Pass.Password);
            imap = new ImapClient(logon, combo_server.Text, int.Parse(textBox_port.Text));

            //ＳＳＬを使用します
            imap.AuthenticationProtocol = TKMP.Net.AuthenticationProtocols.SSL;
            //証明書に問題があった場合に独自の処理を追加します
            imap.CertificateValidation += new CertificateValidationHandler(imap_CertificateValidation);

            //接続開始
            if (!imap.Connect())
            {
                MessageBox.Show("接続失敗");
                return;
            }
            else
            {
                MessageBox.Show("接続成功");
            }

            if (imap.GetMailBox() != null)
            {
                
                
                mailDL2();

            }
            
            
        }
    }
}

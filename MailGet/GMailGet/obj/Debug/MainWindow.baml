   M S B A M L   `   `   `  ����B  >MailGet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:GMailGetGMailGet  5   	       MailGet.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:GMailGet     5   	   \ XHardcodet.Wpf.TaskbarNotification, Version=1.0.5.0, Culture=neutral, PublicKeyToken=null)tb http://www.hardcodet.net/taskbar  5   	     � Title$  MailGet��5   	   $��636.522��5	   	   $��784.399��6   .��5
      � $��
28,0,2,-21q�6   .+�5      3  -Hardcodet.Wpf.TaskbarNotification.TaskbarIcon  -   5          Name$ MyNotifyIcon��5         
IconSource$+ %pack://application:,,,/img/CsIcon.ico��5         ToolTipText$ 通知��5      $��	0,0,-24,0q�6$   5      �� 5   
   -   5         ��Name$ listView��5      $
��Left=�6%   $	��400��6@   $��
20,145,0,0q�6M   $	��Top=�6a   $	��313��6y   ��6�   ��  �� View 5      �� 5      .P�5      �� $
��From��6%    �� DisplayMemberBinding 63   �� *From+�� 5      $��Subject��6%    66   �� *	Subject+�� 5      $��MDate��6%    64   �� *MDate+5      5      �� 5   
   -   5         ��Name$ textBox��5      $
��Left=�6#   $	��400��6>   $��348,145,0,0q�6K    �� TextWrapping$
 Wrap=�6`   $	��Top=�6t   $	��372��6�   �� 5   
   -   5        $ textBox_userName��5      $
��Left=�6,   $��23��6G   $��
119,70,0,0q�6S   $
 Wrap=�6g   $	��Top=�6{   $	��120��6�   �� 5   
   $��userName��6   $
��Left=�6#   $��	42,70,0,0q�6>   $	��Top=�6Q   $��72��6i   �� 5   
   $
��Pass��6   $
��Left=�6   $��
246,70,0,0q�6:   $	��Top=�6N   $��42��6f   G� 5    
   -   5        	 G�Name$	 textBox_Pass��5       $
��Left=�6,   $��
288,72,0,0q�6G   $	��Top=�6[   $��23��6s   $	��139��6   �� 5!   
   $��server��6   $
��Left=�6!   $��
441,68,0,0q�6<   $	��Top=�6P   $��46��6h   �� 5"   
   -   5        
 ��Name$
 button_connect��5"      $��接続��6)   $
��Left=�66   $��
645,70,0,0q�6Q   $	��Top=�6e   $��75��6}   6�   �� 5#   
   -   5         ��Name$ combo_server��5#      $
��Left=�6)   $��
492,70,0,0q�6D   $	��Top=�6X   $	��120��6p   �� 5$   
   -   5        $
 button_save��5$      $��設定の保存��6&   $
��Left=�66   $��645,116,0,0q�6Q   $	��Top=�6f   $��75��6~   6�   �� 5%   
   -	   5        $ combo_insert��5%      $
��Left=�6)   $��
213,18,0,0q�6D   $	��Top=�6X   $	��120��6p   	� 5&   
   -
   5         	�Name$ 	radio_old��5&      $$��登録アカウントを使用��6)   $
��Left=�6>   $��	42,24,0,0q�6Y   $	��Top=�6l    d� 	IsChecked$
 TrueY�6�   6�   	� 5'   
   -   5        $ 	radio_new��5'      $��新たに入力��6)   $
��Left=�69   $��	42,47,0,0q�6T   $	��Top=�6g   $
 TrueY�6   6�   �� 5(   
   -   5        $
 button_clear��5(      $��Clear��6'   $
��Left=�67   $��537,116,0,0q�6R   $	��Top=�6g   $��75��6   6�   �� 5)   
   -   5        $
 button_insert_delete��5)      $��削除��6/   $
��Left=�6<   $��
352,18,0,0q�6W   $	��Top=�6k   $��75��6�   6�   �� 5*   
   -   5         ��Name$ gridSplitter��5*      $
��Left=�6-   $	��100��6H   $��811,300,-67,0q�6U   $	��Top=�6l   $��5��6�   5+      5,      
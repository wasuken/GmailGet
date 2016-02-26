using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MailGet
{
    public class PasswordManager
    {
        //参考・・・パクリ元：http://dobon.net/vb/dotnet/string/encryptstring.html
        //DOBON.NET
        public static string EncryptString(string sourceString, string password)
        {
            //RijndaelManagedオブジェクトを作成
            System.Security.Cryptography.RijndaelManaged rijndael =
                new System.Security.Cryptography.RijndaelManaged();

            //パスワードから共有キーと初期化ベクタを作成
            byte[] key, iv;
            GenerateKeyFromPassword(
                password, rijndael.KeySize, out key, rijndael.BlockSize, out iv);
            rijndael.Key = key;
            rijndael.IV = iv;
            

            //文字列をバイト型配列に変換する
            byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(sourceString);

            //対称暗号化オブジェクトの作成
            System.Security.Cryptography.ICryptoTransform encryptor =
                rijndael.CreateEncryptor();
            //バイト型配列を暗号化する
            byte[] encBytes = encryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
            //閉じる
            encryptor.Dispose();

            //バイト型配列を文字列に変換して返す
            return System.Convert.ToBase64String(encBytes);
        }

        /// <summary>
        /// 暗号化された文字列を復号化する
        /// </summary>
        /// <param name="sourceString">暗号化された文字列</param>
        /// <param name="password">暗号化に使用したパスワード</param>
        /// <returns>復号化された文字列</returns>
        public static string DecryptString(string sourceString, string password)
        {
            //RijndaelManagedオブジェクトを作成
            System.Security.Cryptography.RijndaelManaged rijndael =
                new System.Security.Cryptography.RijndaelManaged();

            //パスワードから共有キーと初期化ベクタを作成
            byte[] key, iv;
            GenerateKeyFromPassword(
                password, rijndael.KeySize, out key, rijndael.BlockSize, out iv);
            rijndael.Key = key;
            rijndael.IV = iv;

            //文字列をバイト型配列に戻す
            byte[] strBytes = System.Convert.FromBase64String(sourceString);

            //対称暗号化オブジェクトの作成
            System.Security.Cryptography.ICryptoTransform decryptor =
                rijndael.CreateDecryptor();
            //バイト型配列を復号化する
            //復号化に失敗すると例外CryptographicExceptionが発生
            byte[] decBytes = decryptor.TransformFinalBlock(strBytes, 0, strBytes.Length);
            //閉じる
            decryptor.Dispose();

            //バイト型配列を文字列に戻して返す
            return System.Text.Encoding.UTF8.GetString(decBytes);
        }

        /// <summary>
        /// パスワードから共有キーと初期化ベクタを生成する
        /// </summary>
        /// <param name="password">基になるパスワード</param>
        /// <param name="keySize">共有キーのサイズ（ビット）</param>
        /// <param name="key">作成された共有キー</param>
        /// <param name="blockSize">初期化ベクタのサイズ（ビット）</param>
        /// <param name="iv">作成された初期化ベクタ</param>
        private static void GenerateKeyFromPassword(string password,
            int keySize, out byte[] key, int blockSize, out byte[] iv)
        {
            //パスワードから共有キーと初期化ベクタを作成する
            //saltを決める
            byte[] salt = System.Text.Encoding.UTF8.GetBytes("saltは必ず8バイト以上");
            //Rfc2898DeriveBytesオブジェクトを作成する
            Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(password, salt);
            
            //.NET Framework 1.1以下の時は、PasswordDeriveBytesを使用する
            //System.Security.Cryptography.PasswordDeriveBytes deriveBytes =
            //    new System.Security.Cryptography.PasswordDeriveBytes(password, salt);
            //反復処理回数を指定する デフォルトで1000回
            deriveBytes.IterationCount = 1000;

            //共有キーと初期化ベクタを生成する
            key = deriveBytes.GetBytes(keySize / 8);
            iv = deriveBytes.GetBytes(blockSize / 8);
        }


        //パクリ元：http://dobon.net/vb/dotnet/string/encryptfile.html
        //またどぼん
        public static void EncryptFile(
            string sourceFile, string destFile, out byte[] key, out byte[] iv)
        {
            //RijndaelManagedオブジェクトを作成
            System.Security.Cryptography.RijndaelManaged rijndael =
                new System.Security.Cryptography.RijndaelManaged();

            //設定を変更するときは、変更する
            //rijndael.KeySize = 256;
            //rijndael.BlockSize = 128;
            //rijndael.FeedbackSize = 128;
            //rijndael.Mode = System.Security.Cryptography.CipherMode.CBC;
            //rijndael.Padding = System.Security.Cryptography.PaddingMode.PKCS7;

            //共有キーと初期化ベクタを作成
            //Key、IVプロパティがnullの時に呼びだすと、自動的に作成される
            //自分で作成するときは、GenerateKey、GenerateIVメソッドを使う
            key = rijndael.Key;
            iv = rijndael.IV;

            //暗号化されたファイルを書き出すためのFileStream
            System.IO.FileStream outFs = new System.IO.FileStream(
                destFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            //対称暗号化オブジェクトの作成
            System.Security.Cryptography.ICryptoTransform encryptor =
                rijndael.CreateEncryptor();
            //暗号化されたデータを書き出すためのCryptoStreamの作成
            System.Security.Cryptography.CryptoStream cryptStrm =
                new System.Security.Cryptography.CryptoStream(
                    outFs, encryptor,
                    System.Security.Cryptography.CryptoStreamMode.Write);

            //暗号化されたデータを書き出す
            System.IO.FileStream inFs = new System.IO.FileStream(
                sourceFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            byte[] bs = new byte[1024];
            int readLen;
            while ((readLen = inFs.Read(bs, 0, bs.Length)) > 0)
            {
                cryptStrm.Write(bs, 0, readLen);
            }

            //閉じる
            inFs.Close();
            cryptStrm.Close();
            encryptor.Dispose();
            outFs.Close();
        }

        /// <summary>
        /// ファイルを復号化する
        /// </summary>
        /// <param name="sourceFile">復号化するファイルパス</param>
        /// <param name="destFile">復号化されたデータを保存するファイルパス</param>
        /// <param name="key">暗号化に使用した共有キー</param>
        /// <param name="iv">暗号化に使用した初期化ベクタ</param>
        public static void DecryptFile(
            string sourceFile, string destFile, byte[] key, byte[] iv)
        {
            //RijndaelManagedオブジェクトの作成
            System.Security.Cryptography.RijndaelManaged rijndael =
                new System.Security.Cryptography.RijndaelManaged();

            //共有キーと初期化ベクタを設定
            rijndael.Key = key;
            rijndael.IV = iv;

            //暗号化されたファイルを読み込むためのFileStream
            System.IO.FileStream inFs = new System.IO.FileStream(
                sourceFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            //対称復号化オブジェクトの作成
            System.Security.Cryptography.ICryptoTransform decryptor =
                rijndael.CreateDecryptor();
            //暗号化されたデータを読み込むためのCryptoStreamの作成
            System.Security.Cryptography.CryptoStream cryptStrm =
                new System.Security.Cryptography.CryptoStream(
                    inFs, decryptor,
                    System.Security.Cryptography.CryptoStreamMode.Read);

            //復号化されたデータを書き出す
            System.IO.FileStream outFs = new System.IO.FileStream(
                destFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            byte[] bs = new byte[1024];
            int readLen;
            //復号化に失敗すると例外CryptographicExceptionが発生
            while ((readLen = cryptStrm.Read(bs, 0, bs.Length)) > 0)
            {
                outFs.Write(bs, 0, readLen);
            }

            //閉じる
            outFs.Close();
            cryptStrm.Close();
            decryptor.Dispose();
            inFs.Close();
        }
    }
}

using UnityEngine;

/// <summary>
/// 暗号化PlayerPrefs
/// </summary>
public static class EncryptedPlayerPrefs
{

    public static void SaveInt(string key, int value)
    {
        string valueString = value.ToString();
        SaveString(key, valueString);
    }
    public static void SaveFloat(string key, float value)
    {
        string valueString = value.ToString();
        SaveString(key, valueString);
    }
    public static void SaveBool(string key, bool value)
    {
        string valueString = value.ToString();
        SaveString(key, valueString);
    }
    public static void SaveString(string key, string value)
    {
        string encKey = Enc.EncryptString(key);
        string encValue = Enc.EncryptString(value.ToString());
        PlayerPrefs.SetString(encKey, encValue);
        PlayerPrefs.Save();
    }

    public static int LoadInt(string key, int defult)
    {
        string defaultValueString = defult.ToString();
        string valueString = LoadString(key, defaultValueString);

        int res;
        if (int.TryParse(valueString, out res))
        {
            return res;
        }
        return defult;
    }
    public static float LoadFloat(string key, float defult)
    {
        string defaultValueString = defult.ToString();
        string valueString = LoadString(key, defaultValueString);

        float res;
        if (float.TryParse(valueString, out res))
        {
            return res;
        }
        return defult;
    }
    public static bool LoadBool(string key, bool defult)
    {
        string defaultValueString = defult.ToString();
        string valueString = LoadString(key, defaultValueString);

        bool res;
        if (bool.TryParse(valueString, out res))
        {
            return res;
        }
        return defult;
    }
    public static string LoadString(string key, string defult)
    {
        string encKey = Enc.EncryptString(key);
        string encString = PlayerPrefs.GetString(encKey, string.Empty);
        if (string.IsNullOrEmpty(encString))
        {
            return defult;
        }
        string decryptedValueString = Enc.DecryptString(encString);
        return decryptedValueString;
    }
    public static void DeleteKey(string key)
    {
        string encKey = Enc.EncryptString(key);
        PlayerPrefs.DeleteKey(encKey);
        PlayerPrefs.Save();
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }


    /// <summary>
    /// 文字列の暗号化・復号化
    /// 参考：http://dobon.net/vb/dotnet/string/encryptstring.html
    /// </summary>
    private static class Enc
    {
        const string PASS = "ynmfNqUYih5sFNQFu3ju";
        const string SALT = "X3Hpevt1jw5EaJK5XCx9";

        static System.Security.Cryptography.RijndaelManaged rijndael;

        static Enc()
        {
            //RijndaelManagedオブジェクトを作成
            rijndael = new System.Security.Cryptography.RijndaelManaged();
            byte[] key, iv;
            GenerateKeyFromPassword(rijndael.KeySize, out key, rijndael.BlockSize, out iv);
            rijndael.Key = key;
            rijndael.IV = iv;
        }


        /// <summary>
        /// 文字列を暗号化する
        /// </summary>
        /// <param name="sourceString">暗号化する文字列</param>
        /// <returns>暗号化された文字列</returns>
        public static string EncryptString(string sourceString)
        {
            //文字列をバイト型配列に変換する
            byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(sourceString);
            //対称暗号化オブジェクトの作成
            System.Security.Cryptography.ICryptoTransform encryptor = rijndael.CreateEncryptor();
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
        /// <returns>復号化された文字列</returns>
        public static string DecryptString(string sourceString)
        {
            //文字列をバイト型配列に戻す
            byte[] strBytes = System.Convert.FromBase64String(sourceString);
            //対称暗号化オブジェクトの作成
            System.Security.Cryptography.ICryptoTransform decryptor = rijndael.CreateDecryptor();
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
        private static void GenerateKeyFromPassword(int keySize, out byte[] key, int blockSize, out byte[] iv)
        {
            //パスワードから共有キーと初期化ベクタを作成する
            //saltを決める
            byte[] salt = System.Text.Encoding.UTF8.GetBytes(SALT);//saltは必ず8byte以上
            //Rfc2898DeriveBytesオブジェクトを作成する
            System.Security.Cryptography.Rfc2898DeriveBytes deriveBytes = new System.Security.Cryptography.Rfc2898DeriveBytes(PASS, salt);
            //.NET Framework 1.1以下の時は、PasswordDeriveBytesを使用する
            //System.Security.Cryptography.PasswordDeriveBytes deriveBytes =
            //    new System.Security.Cryptography.PasswordDeriveBytes(password, salt);
            //反復処理回数を指定する デフォルトで1000回
            deriveBytes.IterationCount = 1000;
            //共有キーと初期化ベクタを生成する
            key = deriveBytes.GetBytes(keySize / 8);
            iv = deriveBytes.GetBytes(blockSize / 8);
        }
    }

}
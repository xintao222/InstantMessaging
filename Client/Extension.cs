using System;
using System.Windows.Forms;
using Newtonsoft.Json;
using Decorator = Client.DecoratorPattern;
using System.Linq;

namespace Client
{
    static class Extension
    {
        /// <summary>
        /// shake window form when come across input error
        /// </summary>
        public static void ShakeForm(this Form form)
        {
            for (int i = 0; i < 4; i++)
            {
                Enumerable.Range(0, 4).ToList().ForEach(x => { form.Left += x; System.Threading.Thread.Sleep(10); });
                Enumerable.Range(0, 4).ToList().ForEach(y => { form.Left -= y; System.Threading.Thread.Sleep(10); });
            }
        }

        public static bool IsNullOrWhiteSpace(this string str)
        {
            return String.IsNullOrWhiteSpace(str);
        }

        #region JSON
        /// <summary>
        /// JSON Serialize
        /// </summary>
        public static string JsonSerialize<T>(this T value)
        {
            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// JSON Deserialize
        /// </summary>
        public static T JsonDeserialize<T>(this string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
        #endregion

        #region  Encrypt/Decrypt

        /// <summary>
        /// Encrypt Message Content
        /// </summary>
        public static string Encrypt(this string value)
        {
            Decorator.Message cryptoMsg = new Decorator.Message { message = value };
            Decorator.DecorateMessage decoratorMessage = new Decorator.DecorateMessage();
            decoratorMessage.Decorate(cryptoMsg);
            return decoratorMessage.GetMessage();
        }

        /// <summary>
        /// Decrypt Message Content
        /// </summary>
        public static string Decrypt(this string value)
        {
            Decorator.CryptoHelper helper = new Decorator.CryptoHelper("ABCDEFGHIJKLMNOP");
            return helper.Decrypt(value);
        }
        #endregion
    }
}

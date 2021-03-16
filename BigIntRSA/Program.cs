using System;
using System.Security.Cryptography;

namespace BigIntRSA
{
    internal class Program
    {
        public static void Main(string[] args)
        {


            Console.WriteLine("e. Зашифровать файл");
            Console.WriteLine("c. Зашифровать данные в консоли");
            var userChoise = Console.ReadLine();
            switch (userChoise)
            {
                case "e":
                    Console.WriteLine("Введите путь к файлу");
                    var encryptedFile = RSA_crypt.EncryptFile(Console.ReadLine());
                    break;
                case "c":
                    Console.WriteLine(
                        new RSA_crypt().Encrypt(Console.ReadLine()));
                    break;
            }
            
        }
    }
}
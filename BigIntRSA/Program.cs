namespace BigIntRSA
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var John = new RSA_crypt(0,0);
            var openKey = John.GetPublicKey();
            var Mary = new RSA_crypt(openKey[0], openKey[1], RSA_crypt.Action.Mary);
            var message = "hrono";
            var res = Mary.Encrypt(message);
            var decRes = John.Decrypt(res);
            
        }
    }
}
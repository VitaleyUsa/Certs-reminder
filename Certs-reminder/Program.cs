using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace CA_reminder
{
    class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]

        static void Main()
        {
            string[] nameParts;
            string CN = string.Empty;
            string cName = string.Empty;
            string cFamily = string.Empty;
            string cPatronymic = string.Empty;
            char[] charsToTrim = { ',' };
            string certs = string.Empty;
            List<Certs_reminder.Certificate> certificates = new List<Certs_reminder.Certificate> { };
            List<string> fio = new List<string> { };

            // Ищем валидные сертификаты
            X509Store store = new X509Store("My", StoreLocation.CurrentUser); // Открываем хранилище сертификатов у текущего пользователя
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly); // Даем права на чтение хранилища

            X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByTimeValid, DateTime.Now, false); // Находим сертификаты только не с истекшим сроком действия
            foreach (var cert in col) // Для каждого сертификата с не истекшим сроком делаем
            {
                DateTime certExpDate = DateTime.Parse(cert.GetExpirationDateString());

                string[] oneItem = cert.Subject.Split('=');
                if (oneItem[0].Trim() == "CN")
                {
                    CN = oneItem[1];
                    if (CN.IndexOf(" ") > 0)
                    { // Делим на ФИО
                        nameParts = CN.Split(' ');
                        cFamily = nameParts[0];
                        cName = nameParts[1];
                        cPatronymic = nameParts[2].TrimEnd(charsToTrim);
                    }
                }

                var crt = new Certs_reminder.Certificate();
                
                crt.Fio = cFamily + " " + cName + " " + cPatronymic;
                crt.Day = certExpDate.DayOfYear;
                crt.Year = certExpDate.Year;
                crt.Expired = false;
                certificates.Add(crt);

                //int certExpDate_day = certExpDate.DayOfYear;
                //int certExpDate_day_now = DateTime.Now.DayOfYear;

                //if (certExpDate.Year == DateTime.Now.Year) // если год равен текущему, то проверяем срок действия
                //{
                //    int certExpDate_day_left = certExpDate_day - certExpDate_day_now; // осталось дней до окончания сертификата

                //    if (certExpDate_day_left <= 35)
                //    {
                //        count++;
                //        string certExpDate_formated = certExpDate.ToString("dd.MM.yy");

                //        string[] oneItem = cert.Subject.Split('=');
                //        if (oneItem[0].Trim() == "CN")
                //        {
                //            CN = oneItem[1];
                //            if (CN.IndexOf(" ") > 0)
                //            { // Делим на ФИО
                //                nameParts = CN.Split(' ');
                //                cFamily = nameParts[0];
                //                cName = nameParts[1];
                //                cPatronymic = nameParts[2].TrimEnd(charsToTrim);
                //            }
                //        }

                //        certs = "Срок действия сертификата (" + cFamily + " " + cName + " " + cPatronymic + ")\nзаканчивается через " + certExpDate_day_left + " дней";
                //        AllCerts += certs + "\n\n";
                //    }
                //}
            }
            //AllCerts += "Выполните процедуру продления сертификатов в ЕИС!";
            //if (count > 0)
            //    MessageBox.Show(AllCerts, "Внимание!");

            foreach (var cert in certificates)
                fio.Add(cert.Fio);

            var duplicates = fio.GroupBy(s => s).SelectMany(g => g.Skip(1));
            var nonduplicates = fio.Except(duplicates);


            foreach (var cert in certificates)
            {
                int cert_day_left = cert.Day - DateTime.Now.DayOfYear;

                foreach (var nondublicate in nonduplicates)
                {
                    if (cert.Fio == nondublicate)
                        if (cert.Year == DateTime.Now.Year)
                            if (cert_day_left <= 35)
                                cert.Expired = true;
                }
                string Declension = GetDeclension(cert_day_left, "день", "дня", "дней");
                if (cert.Expired)
                    certs = "Срок действия сертификата (" + cert.Fio + ")\nзаканчивается через " + cert_day_left + " " + Declension + "\n\n";
            }

            if (!string.IsNullOrEmpty(certs))
            {
                certs += "Выполните процедуру продления сертификатов в ЕИС.";

                MessageBox.Show(certs, "Внимание!");
            }
        }

        public static string GetDeclension(int number, string nominativ, string genetiv, string plural) // Склонение дней
        {
            number = number % 100;
            if (number >= 11 && number <= 19)
            {
                return plural;
            }

            var i = number % 10;
            switch (i)
            {
                case 1:
                    return nominativ;
                case 2:
                case 3:
                case 4:
                    return genetiv;
                default:
                    return plural;
            }

        }
    }

}

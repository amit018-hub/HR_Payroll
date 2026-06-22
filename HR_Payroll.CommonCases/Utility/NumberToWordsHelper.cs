using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Payroll.CommonCases.Utility
{
    public static class NumberToWordsHelper
    {
        private static readonly string[] Ones =
        {
            "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
            "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
            "Seventeen", "Eighteen", "Nineteen"
        };

        private static readonly string[] Tens =
        {
            "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
        };

        public static string ConvertRupeesToWords(decimal amount)
        {
            if (amount < 0)
            {
                return "Minus " + ConvertRupeesToWords(-amount);
            }

            long whole = (long)Math.Floor(amount);

            if (whole == 0)
            {
                return "Zero Rupees Only";
            }

            var sb = new StringBuilder();

            long crore = whole / 10000000; whole %= 10000000;
            long lakh = whole / 100000; whole %= 100000;
            long thousand = whole / 1000; whole %= 1000;
            long hundred = whole / 100; whole %= 100;
            long remainder = whole;

            if (crore > 0) sb.Append(ConvertTwoDigitGroup((int)crore)).Append(" Crore ");
            if (lakh > 0) sb.Append(ConvertTwoDigitGroup((int)lakh)).Append(" Lakh ");
            if (thousand > 0) sb.Append(ConvertTwoDigitGroup((int)thousand)).Append(" Thousand ");
            if (hundred > 0) sb.Append(Ones[hundred]).Append(" Hundred ");
            if (remainder > 0) sb.Append(ConvertTwoDigitGroup((int)remainder)).Append(' ');

            sb.Append("Rupees Only");

            return sb.ToString().Replace("  ", " ").Trim();
        }

        private static string ConvertTwoDigitGroup(int n)
        {
            if (n < 20) return Ones[n];
            int tensDigit = n / 10;
            int onesDigit = n % 10;
            return onesDigit == 0 ? Tens[tensDigit] : $"{Tens[tensDigit]} {Ones[onesDigit]}";
        }
    }
}

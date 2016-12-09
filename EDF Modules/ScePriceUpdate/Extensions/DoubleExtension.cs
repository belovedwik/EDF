#region using

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Databox.Libs.ScePriceUpdate;

#endregion

namespace ScePriceUpdate.Extensions
{
    public static class DoubleExtension
    {
        public static double CountMurkup(this double price, List<PriceMarkup> priceMarkups)
        {
            if (priceMarkups.Count < 1)
            {
                throw new ArgumentException("Price Markup list is empty.");
            }
            foreach (var priceMarkup in priceMarkups)
            {
                if (price >= priceMarkup.From && price <= priceMarkup.To)
                {
                    return price*priceMarkup.Markup;
                }
            }
            return price;
        }

        public static double ParsePrice(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return 0.0;
            int num1 = str.IndexOf("EUR");
            double num2 = ParseDouble(str.Replace("$", "").Replace("EUR", ""));
            if (num1 != -1)
                num2 /= 100.0;
            return num2;
        }

        public static double ParseDouble(this string str)
        {
            if (str == null)
                return 0.0;
            str = str.Trim();
            if (str == "")
                return 0.0;
            int length = str.IndexOf(" ");
            if (length != -1)
                str = str.Substring(0, length);
            str = str.Replace("$", "").Replace(":", "").Replace("MSRP", "");
            CultureInfo cultureInfo = new CultureInfo(Thread.CurrentThread.CurrentCulture.LCID);
            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.NumberGroupSeparator = ",";
            double result = 0.0;
            double.TryParse(str, NumberStyles.Number, cultureInfo.NumberFormat, out result);
            return result;
        }
    }
}

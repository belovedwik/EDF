#region using

using System;
using System.Text.RegularExpressions;

#endregion

namespace ScePriceUpdate.Extensions
{
     public static class StringExtension
     {
          public static string ToLowerCaseTags(this string input)
          {
               return Regex.Replace(
                    input,
                    @"<[^<>]+>",
                    m => { return m.Value.ToLower(); },
                    RegexOptions.Multiline | RegexOptions.Singleline);
          }

          public static string HtmlStrip(this string input)
          {
               input = Regex.Replace(input, @"<style>(.|\n)*?</style>", string.Empty);
               input = Regex.Replace(input, @"<STYLE>(.|\n)*?</STYLE>", string.Empty);
               input = Regex.Replace(input, @"<script(.|\n)*?</script>", string.Empty);
               input = Regex.Replace(input, @"<SCRIPT(.|\n)*?</SCRIPT>", string.Empty);
               input = Regex.Replace(input, @"style(.|\n)*?;", string.Empty);
               input = Regex.Replace(input, @"<xml>.[\n]$", string.Empty);
               //input = Regex.Replace(input, @"<img(.|\n)*?height", string.Empty);
               input = Regex.Replace(input, @"<img(.|\n)*?>;", string.Empty);
               input = Regex.Replace(input, @"<img(.|\n)*?(.gif |.png |.jpg )>", string.Empty);
               input = Regex.Replace(input, @"background-image: url((.|\n)*?);", string.Empty);
               input = Regex.Replace(input, @"<img(.|\n)*? class=prVerified>", string.Empty);
               return input;
          }

          public static string RemoveDirtyData(this string p)
          {
               return
                    p.RemoveUnicode()
                         .Replace("--", "-")
                         .Replace("/", "")
                         .Replace("\"", "")
                         .Replace("*", "")
                         .Replace("€", " ")
                         .Replace("œ", " ")
                         .Replace("â", " ")
                         .Replace("¢", " ")
                         .Replace("Â", " ")
                         .Replace("®", " ")
                         .Replace("„", " ")
                         .Replace("  ", " ")
                         .Replace("[", " ")
                         .Replace("]", " ")
                         .Replace("{", " ")
                         .Replace("}", "")
                         .Replace("~~", "")
                         .Replace("varchar", "")
                         .Replace("sp_", "")
                         .Replace("xp_", "")
                         .Replace("™", "")
                         .Replace("“", "")
                         .Replace("Ã‚â€•", "")
                         .Replace("Ã¢â", "")
                         .Replace("¬â„¢s", "")
                         .Replace("€•", "")
                         .Replace("â€™", "")
                         .Replace("¢", "")
                         .Replace("@@", "")
                         .Replace("â€œ", "")
                         .Replace("â€”", "")
                         .Replace("insert into", "")
                         .Replace("/script", "")
                         .Replace("delete from", "")
                         .Replace("drop table", "")
                         .Replace("exec(", "")
                         .Replace("declare()*@", "")
                         .Replace("cast(", "")
                         .Replace("<strong>", "")
                         .Replace("</strong>", "")
                         .Replace("\r\n", "")
                         .Replace("\r", "")
                         .Replace("\n", "");
          }

          public static string ReplaceTextForAnchorText(this string p)
          {
               return p.RemoveDirtyData().Replace("&", " and ");
          }

          public static string RemoveUnicode(this string input)
          {
               var s = input;
               s = Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);
               return s;
          }

          public static string RemoveAt(this string input)
          {
              if (!string.IsNullOrEmpty(input) && input.StartsWith("@"))
              {
                  input = input.Remove(0, 1);
              }
              return input;
          }

          public static string PrepeadForLoad(this string str)
          {
               if (!string.IsNullOrEmpty(str))
               {
                    return str.RemoveUnicode().ReplaceTextForAnchorText().RemoveDirtyData().Trim();
               }
               return str;
          }
     }
}

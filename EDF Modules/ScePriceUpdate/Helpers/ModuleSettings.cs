#region using

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Databox.Libs.ScePriceUpdate;

#endregion

namespace Databox.Libs.ScePriceUpdate
{
     public class ModuleSettings
     {
          public ModuleSettings()
          {
               ProductsFromSce = new List<SceProduct>();
               PriceMarkups = new List<PriceMarkup>();
          }

          private const bool IsProduction = false;
          public List<SceProduct> ProductsFromSce { get; set; }
          public List<PriceMarkup> PriceMarkups { get; set; }

          private static ModuleSettings _default;

          public static ModuleSettings Default
          {
               get
               {
                    if (_default == null)
                    {
                         _default = ReadConfig();
                    }
                    return _default;
               }
          }

          public void SaveConfig()
          {
               lock (this)
               {
                    var configFile = GetConfigFileName();
                    var tmpFile = configFile + ".tmp";
                    using (var fs = File.Create(tmpFile))
                    {
                         var xs = new XmlSerializer(typeof (ModuleSettings));
                         xs.Serialize(fs, _default);
                    }
                    File.Delete(configFile);
                    File.Move(tmpFile, configFile);
               }
          }

          private static string GetConfigFileName(bool isProduction = false)
          {
               string assemblyFolder;
               if(IsProduction)
               {
                    assemblyFolder = Path . GetDirectoryName(Assembly . GetExecutingAssembly() . Location);
               }
               else
               {
                    assemblyFolder = Path . Combine(Environment . GetFolderPath(Environment . SpecialFolder . ApplicationData), "EDF");
               }
               var configFile = Path.Combine(assemblyFolder, "ScePriceUpdateSettings.config");
               return configFile;
          }

          private static ModuleSettings ReadConfig()
          {
               var configFile = GetConfigFileName();

               if(!IsProduction)
               {
                    if (!File . Exists(configFile))
                    {
                         var settingsDir = Path . GetDirectoryName(Assembly . GetExecutingAssembly() . Location);
                         configFile = Path . Combine(settingsDir, "ScePriceUpdateSettings.config");
                    }
               }

               if (File . Exists(configFile))
               {
                    try
                    {
                         var xs = new XmlSerializer(typeof(ModuleSettings));
                         using (var sr = File . OpenText(configFile))
                         {
                              var xtr = new XmlTextReader(sr);
                              var conf = (ModuleSettings)xs . Deserialize(xtr);
                              return conf;
                         }
                    }
                    catch (Exception err)
                    {
                         return new ModuleSettings();
                    }
               }
               return new ModuleSettings();
          }
     }
}
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wu.CommTool.Models
{
    public class ConfigFile : BindableBase
    {
        public ConfigFile(FileInfo file)
        {
            File = file;
        }

        private FileInfo File { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string Name => File.Name.Replace(File.Extension, "");

        /// <summary>
        /// 文件全称
        /// </summary>
        public string FullName => File.FullName;


    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wu.CommTool.Converters;

namespace Wu.CommTool.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum StopBits
    {
        [Description("1")]
        One = 1,
        [Description("2")]
        Two,
        [Description("1.5")]
        OnePointFive
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Wu.CommTool.Controls
{
    /// <summary>
    /// WuIpBox.xaml 的交互逻辑
    /// </summary>
    public partial class WuIpBox : UserControl
    {
        public WuIpBox()
        {
            InitializeComponent();
            this.SizeChanged += WuIpBox_SizeChanged;
            //黏贴触发事件
            DataObject.AddPastingHandler(ipPart1TextBox, OnPaste);
            DataObject.AddPastingHandler(ipPart2TextBox, OnPaste);
            DataObject.AddPastingHandler(ipPart3TextBox, OnPaste);
            DataObject.AddPastingHandler(ipPart4TextBox, OnPaste);

        }


        /// <summary>
        /// 黏贴事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText) return;
            var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            string strResult = text.Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();
            bool isIp = Regex.IsMatch(strResult, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
            if (isIp)
            {
                Ip = text;

                //var splited = strResult.Split('.');
                //IpPart1 = splited[0];
                //IpPart2 = splited[1];
                //IpPart3 = splited[2];
                //IpPart4 = splited[3];

                //IpPart1 = Convert.ToInt32(splited[0]);
                //IpPart2 = Convert.ToInt32(splited[1]);
                //IpPart3 = Convert.ToInt32(splited[2]);
                //IpPart4 = Convert.ToInt32(splited[3]);
            }
        }

        #region 控件尺寸修改时触发
        private void WuIpBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Refresh();
        }

        /// <summary>
        /// 更新显示
        /// </summary>
        private void Refresh()
        {
        }
        #endregion



        #region 依赖属性
        [Category("Wu")]
        [Description("IpPart1")]
        public string IpPart1
        {
            get { return (string)GetValue(IpPart1Property); }
            set { SetValue(IpPart1Property, value); }
        }
        public static readonly DependencyProperty IpPart1Property =
                    DependencyProperty.Register("IpPart1", typeof(string), typeof(WuIpBox),
                        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                            new PropertyChangedCallback(OnIpPartChanged)));


        [Category("Wu")]
        [Description("IpPart2")]
        public string IpPart2
        {
            get { return (string)GetValue(IpPart2Property); }
            set { SetValue(IpPart2Property, value); }
        }
        public static readonly DependencyProperty IpPart2Property =
                    DependencyProperty.Register("IpPart2", typeof(string), typeof(WuIpBox),
                        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                            new PropertyChangedCallback(OnIpPartChanged)));


        [Category("Wu")]
        [Description("IpPart3")]
        public string IpPart3
        {
            get { return (string)GetValue(IpPart3Property); }
            set { SetValue(IpPart3Property, value); }
        }
        public static readonly DependencyProperty IpPart3Property =
                    DependencyProperty.Register("IpPart3", typeof(string), typeof(WuIpBox),
                        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                            new PropertyChangedCallback(OnIpPartChanged)));


        [Category("Wu")]
        [Description("IpPart4")]
        public string IpPart4
        {
            get { return (string)GetValue(IpPart4Property); }
            set { SetValue(IpPart4Property, value); }
        }
        public static readonly DependencyProperty IpPart4Property =
                    DependencyProperty.Register("IpPart4", typeof(string), typeof(WuIpBox),
                        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                            new PropertyChangedCallback(OnIpPartChanged)));



        [Category("Wu")]
        [Description("Ip")]
        public string Ip
        {
            get { return (string)GetValue(IpProperty); }
            set { SetValue(IpProperty, value); }
        }
        public static readonly DependencyProperty IpProperty =
                    DependencyProperty.Register("Ip", typeof(string), typeof(WuIpBox),
                        new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                            new PropertyChangedCallback(OnIpChanged)));


        [Category("Wu")]
        [Description("标题宽度")]
        public int TitleWidth
        {
            get { return (int)GetValue(TitleWidthProperty); }
            set { SetValue(TitleWidthProperty, value); }
        }
        public static readonly DependencyProperty TitleWidthProperty =
                    DependencyProperty.Register("TitleWidth", typeof(int), typeof(WuIpBox), new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));


        [Category("Wu")]
        [Description("标题")]
        public string IpTitle
        {
            get { return (string)GetValue(IpTitleProperty); }
            set { SetValue(IpTitleProperty, value); }
        }
        public static readonly DependencyProperty IpTitleProperty =
                    DependencyProperty.Register("IpTitle", typeof(string), typeof(WuIpBox), new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion


        /// <summary>
        /// IP部分修改时触发
        /// </summary> 
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnIpPartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WuIpBox self)
            {
                try
                {
                    self.Ip = $"{self.IpPart1}.{self.IpPart2 }.{self.IpPart3}.{self.IpPart4}";
                }
                catch (Exception) { }
            }
        }

        private static void OnIpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WuIpBox self)
            {
                try
                {
                    var splited = self.Ip.Split(".");
                    self.IpPart1 = splited[0];
                    self.IpPart2 = splited[1];
                    self.IpPart3 = splited[2];
                    self.IpPart4 = splited[3];
                    //self.IpPart1 = Convert.ToInt32(splited[0]);
                    //self.IpPart2 = Convert.ToInt32(splited[1]);
                    //self.IpPart3 = Convert.ToInt32(splited[2]);
                    //self.IpPart4 = Convert.ToInt32(splited[3]);
                }
                catch (Exception) { }
            }
        }



        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            //Keys/* key = e.keycode*/
            //System.Windows.Forms.Keys x = e.keycode
            TextBox tbx = sender as TextBox;
            if ((key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9))
            {
                //表示输入IP，暂不做处理
            }
            else if (key == Key.Tab)
            {
                //Tab自动跃点，暂不做处理
            }
            else if (key == Key.Back)
            {
                if (tbx.CaretIndex == 0)
                {
                    SetTbxFocus(tbx, false, false);
                }
            }
            else if (key == Key.Left)
            {
                if (tbx.CaretIndex == 0)
                {
                    SetTbxFocus(tbx, false, false);
                    e.Handled = true;
                }
            }
            else if (key == Key.Right)
            {
                if (tbx.SelectionStart == tbx.Text.Length)
                {
                    SetTbxFocus(tbx, true, false);
                    e.Handled = true;
                }
            }
            //输入 . 
            else if ((key == Key.Decimal || key == Key.OemPeriod || key == Key.ImeProcessed) && tbx.CaretIndex != 0)
            {
                SetTbxFocus(tbx, true, false);
                e.Handled = true;
            }
            //else if (K)
            else
            {
                e.Handled = true;
            }
        }

        private void TextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            TextBox tbx = sender as TextBox;
            if ((key >= Key.D0 && key <= Key.D9) || (key >= Key.NumPad0 && key <= Key.NumPad9))
            {
                // 当前输入框满三个数字后
                // 跳转到后面一个输入框
                if (tbx.Text.Length == 3)
                {
                    if (int.Parse(tbx.Text) < 0 || Int32.Parse(tbx.Text) > 255)
                    {
                        tbx.Focus();
                        MessageBox.Show($"{tbx.Text}不是有效项。请指定一个介于0和255间的值。", "错误", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        tbx.Text = byte.MaxValue.ToString();
                        return;
                    }
                    SetTbxFocus(tbx, true, true);
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 设置当前输入框的前面或后面的输入框获取焦点，以及是否全选内容
        /// </summary>
        /// <param name="curretTbx">当前输入框</param>
        /// <param name="isBack">是否是后面的文本框|True：后 False：前</param>
        /// <param name="isSelectAll">是否全选</param>
        private void SetTbxFocus(TextBox curretTbx, bool isBack, bool isSelectAll)
        {
            List<TextBox> TbxIPList = new();
            foreach (UIElement item in gridIPAddress.Children)
            {
                if (item.GetType() == typeof(TextBox))
                {
                    TbxIPList.Add((TextBox)item);
                }
            }
            TextBox nextTbx = null;
            // 往后
            if (isBack)
            {
                int index = TbxIPList.IndexOf(curretTbx);
                if (index <= 2)
                {
                    nextTbx = TbxIPList[index + 1];
                }
            }
            // 往前
            else
            {
                int index = TbxIPList.IndexOf(curretTbx);
                if (index >= 1)
                {
                    nextTbx = TbxIPList[index - 1];
                }
            }
            if (nextTbx != null)
            {
                nextTbx.Focus();
                nextTbx.SelectAll();
                //if (isSelectAll)
                //{
                //}
            }
        }


    }
}

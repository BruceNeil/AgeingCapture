using AgeingCapture.Models;
using AgeingCapture.Utils;
using AutoCali.Infrastructure;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static AgeingCapture.Utils.TraceInfo;

namespace AgeingCapture
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public MainWindow()
        {
            InitializeComponent();
            txtTimes.IsEnabled = false;
            chk_FactoryMode.IsChecked = false;
        }

        private void btn_ChoosePath_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Multiselect = false; // 设置为 true 可以选择多个文件
            openFileDialog.InitialDirectory = "C:\\"; // 设置对话框的初始目录
            openFileDialog.Filter = "All Files (*.*)|*.*"; // 设置文件过滤器，限制可选文件类型

            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtPath.Text = openFileDialog.FileName;
                if (string.IsNullOrEmpty(txtPath.Text))
                {
                    System.Windows.Forms.MessageBox.Show("路径不允许为空！");
                    return;
                }
            }
        }

        private int SafeParseDecimal(string value, string ErrorMsg)
        {
            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }
            else
            {
                throw new FormatException(ErrorMsg);
            }
        }
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true; // 阻止非数字字符的输入
                txtTime.Text = Regex.Replace(txtTime.Text, "[^0-9]+", "");
            }
        }

        private async void btn_Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtRate.Text))
            {
                System.Windows.Forms.MessageBox.Show("Capture路径不允许为空！");
                return;
            }

            if (string.IsNullOrEmpty(txtPath.Text))
            {
                System.Windows.Forms.MessageBox.Show("Json文件路径不允许为空！");
                return;
            }
            try
            {
                string jsonContent = File.ReadAllText(txtPath.Text);
                var root = JsonConvert.DeserializeObject<AgeingParam>(jsonContent);
                foreach (var data in root.Ageings)//循环每一个老化
                {
                    //读取master.ini中的DeviceModel
                    if (!Directory.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(txtRate.Text), "Config")))
                    {
                        Directory.CreateDirectory(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(txtRate.Text), "Config"));
                    }
                    var ini = new IniFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(txtRate.Text), "Config\\Master.ini"));
                    int DeviceModel = SafeParseDecimal(ini.IniReadValue("default", "DeviceModel"), "Master.ini文件中default节点下的DeviceModel值不正确，请检查！");
                    //将Json文件中的DeviceModel修改成Master.ini中一致。
                    data.DeviceModel = DeviceModel.ToString();
                    string CapPath = txtRate.Text;
                    string iniPath = "";
                    //首先判断机器号，是20还是30
                    switch (DeviceModel)
                    {
                        case (int)EnumDeviceModel.Mode1020S:
                        case (int)EnumDeviceModel.Mode1020MS:
                        case (int)EnumDeviceModel.Mode1020S2:
                        case (int)EnumDeviceModel.Mode1020MS2:
                            iniPath = Write20Ini(data);
                            break;
                        default:
                            iniPath = Write30Ini(data);
                            break;
                    }
                    data.Ini = iniPath;
                    // 创建一个进程对象
                    Process process = new Process();
                    // 设置要执行的可执行文件路径
                    process.StartInfo.FileName = CapPath;
                    // 设置参数字符串
                    process.StartInfo.Arguments = data.ToString();
                    // 异步启动进程
                    await Task.Run(() =>
                    {
                        // 启动进程
                        process.Start();
                        // 等待进程执行完毕，最多等待300秒300000
                        if (!process.WaitForExit(300000))
                        {
                            // 如果超过300秒仍未退出，就强制关闭进程
                            process.Kill();
                            string  processName = "FDK.exe";
                            // 获取同名进程的列表
                            Process[] processes = Process.GetProcessesByName(processName);
                            // 遍历进程列表，关闭除指定进程外的其他进程
                            foreach (Process cess in processes)
                            {
                                cess.Kill();
                            }
                            TraceInfo.LogOut(LogLevels.Error, "响应超时，自动退出！");
                        }
                    });
                    // 获取进程的退出代码
                    int exitCode = process.ExitCode;
                    // 设置间隔时间（单位：毫秒）
                    int interval = SafeParseDecimal(txtTime.Text, "间隔时间值不正确，请检查！")*10000;
                    await Task.Delay(interval);
                }
            }
            catch (Exception ex)
            {
                TraceInfo.LogOut(LogLevels.Error, ex.ToString() + ex.StackTrace);
                System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
            }
        }

        public string Write20Ini(Ageing data)
        {
            var ct = data.CT;
            string filePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(txtRate.Text), "Config\\auto1020.ini");
            // 创建空文件
            File.Create(filePath).Dispose();
            var ini = new IniFile(filePath);
            ini.IniWriteValue("CT", "FovType", ct.FovType.ToString());
            ini.IniWriteValue("CT", "CTModeType", ct.CTModeType.ToString());
            ini.IniWriteValue("CT", "ResolutionType", ct.ResolutionType.ToString());
            ini.IniWriteValue("CT", "Teeth", ct.Teeth.ToString());
            ini.IniWriteValue("CT", "Mar", ct.Mar.ToString());
            ini.IniWriteValue("CT", "Smart3DPano", ct.Smart3DPano.ToString());
            ini.IniWriteValue("CT", "FastScan", ct.FastScan.ToString());
            ini.IniWriteValue("CT", "PatientType", ct.PatientType.ToString());
            ini.IniWriteValue("CT", "KV", ct.KV.ToString());
            ini.IniWriteValue("CT", "MA", ct.MA.ToString());
            var Pano = data.Pano;
            ini.IniWriteValue("Pano", "TMJ", Pano.TMJ.ToString());
            ini.IniWriteValue("Pano", "Autofocus", Pano.Autofocus.ToString());
            ini.IniWriteValue("Pano", "TrajectoryType", Pano.TrajectoryType.ToString());
            ini.IniWriteValue("Pano", "PatientType", Pano.PatientType.ToString());
            ini.IniWriteValue("Pano", "KV", Pano.KV.ToString());
            ini.IniWriteValue("Pano", "MA", Pano.MA.ToString());
            var CEPH = data.CEPH;
            ini.IniWriteValue("CEPH", "CEPHModeType", CEPH.CEPHModeType.ToString());
            ini.IniWriteValue("CEPH", "FastScan", CEPH.FastScan.ToString());
            ini.IniWriteValue("CEPH", "PatientType", CEPH.PatientType.ToString());
            ini.IniWriteValue("CEPH", "KV", CEPH.KV.ToString());
            ini.IniWriteValue("CEPH", "MA", CEPH.MA.ToString());
            return filePath;
        }

        public string Write30Ini(Ageing data)
        {
            var ct = data.CT;
            string filePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(txtRate.Text), "Config\\auto1030.ini");
            // 创建空文件
            File.Create(filePath).Dispose();
            var ini = new IniFile(filePath);
            ini.IniWriteValue("CT", "AutoScanFov", ct.AutoScanFov.ToString());
            ini.IniWriteValue("CT", "CTModeType", ct.CTModeType.ToString());
            ini.IniWriteValue("CT", "Teeth", ct.Teeth.ToString());
            ini.IniWriteValue("CT", "Mar", ct.Mar.ToString());
            ini.IniWriteValue("CT", "Smart3DPano", ct.Smart3DPano.ToString());
            ini.IniWriteValue("CT", "FastScan", ct.FastScan.ToString());
            ini.IniWriteValue("CT", "PatientType", ct.PatientType.ToString());
            ini.IniWriteValue("CT", "KV", ct.KV.ToString());
            ini.IniWriteValue("CT", "MA", ct.MA.ToString());
            var Pano = data.Pano;
            ini.IniWriteValue("Pano", "AutoScanPanoMode", Pano.AutoScanPanoMode.ToString());
            ini.IniWriteValue("Pano", "TrajectoryType", Pano.TrajectoryType.ToString());
            ini.IniWriteValue("Pano", "PanoROI", Pano.PanoROI.ToString());
            ini.IniWriteValue("Pano", "Teeth", Pano.Teeth.ToString());
            ini.IniWriteValue("Pano", "PatientType", Pano.PatientType.ToString());
            ini.IniWriteValue("Pano", "KV", Pano.KV.ToString());
            ini.IniWriteValue("Pano", "MA", Pano.MA.ToString());
            var CEPH = data.CEPH;
            ini.IniWriteValue("CEPH", "CEPHModeType", CEPH.CEPHModeType.ToString());
            ini.IniWriteValue("CEPH", "FastScan", CEPH.FastScan.ToString());
            ini.IniWriteValue("CEPH", "PatientType", CEPH.PatientType.ToString());
            ini.IniWriteValue("CEPH", "KV", CEPH.KV.ToString());
            ini.IniWriteValue("CEPH", "MA", CEPH.MA.ToString());
            return filePath;
        }

        private void btn_newFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Multiselect = false; // 设置为 true 可以选择多个文件
            openFileDialog.InitialDirectory = "C:\\"; // 设置对话框的初始目录
            openFileDialog.Filter = "All Files (*.*)|*.*"; // 设置文件过滤器，限制可选文件类型

            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtRate.Text = openFileDialog.FileName;
                if (string.IsNullOrEmpty(txtRate.Text))
                {
                    System.Windows.Forms.MessageBox.Show("路径不允许为空！");
                    return;
                }
            }
        }

        private void chk_FactoryMode_Checked(object sender, RoutedEventArgs e)
        {
            txtTimes.IsEnabled = true;
        }

        private void chk_FactoryMode_Unchecked(object sender, RoutedEventArgs e)
        {
            txtTimes.IsEnabled = false;
        }

        private void btn_pause_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

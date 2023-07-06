using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgeingCapture.Models
{
    enum EnumDeviceModel
    {
        Mode1020S = 1020,//1020S，老界面
        Mode1020MS = 1021,//1020MS，老界面
        Mode1020S2 = 1025,//1020S，强检界面
        Mode1020MS2 = 1026,//1020MS，强检界面
        Mode1030XS = 1030,//1030XS
        Mode1030XE = 1031,//1030XE
        Mode1030pro = 1035,//1030pro
        Mode1030pro_16x11 = 1036,//1030pro_16x11
        Mode1040 = 1040,
        Mode1040pro = 1045,
    }
    public class AgeingParam
    {
        public List<Ageing> Ageings { get; set; }
    }

    public class Ageing
    {
        [JsonProperty("-auto")]
        public string Auto { get; set; }

        [JsonProperty("-ini")]
        public string Ini { get; set; }

        [JsonProperty("-fov")]
        public int Fov { get; set; }

        [JsonProperty("-number")]
        public string Number { get; set; }

        [JsonProperty("-name")]
        public string Name { get; set; }

        [JsonProperty("-id")]
        public string Id { get; set; }

        [JsonProperty("-sex")]
        public int Sex { get; set; }

        [JsonProperty("-age")]
        public int Age { get; set; }

        [JsonProperty("-birth")]
        public string Birth { get; set; }

        [JsonProperty("-ph")]
        public string Ph { get; set; }

        [JsonProperty("-doc")]
        public string Doc { get; set; }

        [JsonProperty("-desc")]
        public string Desc { get; set; }

        [JsonProperty("-stuid")]
        public string Stuid { get; set; }

        [JsonProperty("-dicompath")]
        public string DicomPath { get; set; }

        [JsonProperty("-Locale")]
        public string Locale { get; set; }

        [JsonProperty("-hostName")]
        public string HostName { get; set; }

        [JsonProperty("-port")]
        public int Port { get; set; }

        [JsonProperty("-userName")]
        public string UserName { get; set; }

        [JsonProperty("-password")]
        public string Password { get; set; }

        [JsonProperty("-DeviceModel")]
        public string DeviceModel { get; set; }

        public CTParams CT { get; set; }
        public PanoParams Pano { get; set; }
        public CephParams CEPH { get; set; }

        public override string ToString()
        {
            return $"-auto{Auto} -ini{Ini} -fov{Fov} -number{Number} -name{Name} -id{Id} -sex{Sex} -age{Age} -birth{Birth} -ph{Ph} -doc{Doc} -desc{Desc} -stuid{Stuid} -dicompath{DicomPath} -Locale{Locale} -hostName{HostName} -port{Port} -userName{UserName} -password{Password} -devicemodel{DeviceModel}";
        }
    }

    public class CTParams
    {
        public int AutoScanFov { get; set; }
        public int CTModeType { get; set; }
        public int Teeth { get; set; }
        public int Mar { get; set; }
        public int Smart3DPano { get; set; }
        public int FastScan { get; set; }
        public int PatientType { get; set; }
        public int KV { get; set; }
        public int MA { get; set; }
        public int FovType { get; set; }
        public int ResolutionType { get; set; }
    }

    public class PanoParams
    {
        public int AutoScanPanoMode { get; set; }
        public int TrajectoryType { get; set; }
        public int PanoROI { get; set; }
        public string Teeth { get; set; }
        public int PatientType { get; set; }
        public int KV { get; set; }
        public int MA { get; set; }
        public int TMJ { get; set; }
        public int Autofocus { get; set; }
    }

    public class CephParams
    {
        public int CEPHModeType { get; set; }

        public int FastScan { get; set; }
        public int PatientType { get; set; }
        public int KV { get; set; }
        public int MA { get; set; }
    }
}

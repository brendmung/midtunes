using Android.App;
using Android.Media;
using Android.OS;
using DeepSound.Helpers.Model;
using DeepSoundClient;
using DeepSoundClient.Classes.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace DeepSound.Helpers.Utils
{
    public static class DeepSoundTools
    {
        public static string GetNameFinal(UserDataObject dataUser)
        {
            try
            {
                if (dataUser == null)
                    return "";

                if (!string.IsNullOrEmpty(dataUser.Name) && !string.IsNullOrWhiteSpace(dataUser.Name))
                    return Methods.FunString.DecodeString(dataUser.Name);

                if (!string.IsNullOrEmpty(dataUser.Username) && !string.IsNullOrWhiteSpace(dataUser.Username))
                    return Methods.FunString.DecodeString(dataUser.Username);

                return Methods.FunString.DecodeString(dataUser.Username);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        public static string GetAboutFinal(UserDataObject dataUser)
        {
            try
            {
                if (dataUser == null)
                    return Application.Context.Resources?.GetString(Resource.String.Lbl_HasNotAnyInfo);

                if (!string.IsNullOrEmpty(dataUser.AboutDecoded) && !string.IsNullOrWhiteSpace(dataUser.AboutDecoded))
                    return Methods.FunString.DecodeString(dataUser.AboutDecoded);

                return Application.Context.Resources?.GetString(Resource.String.Lbl_HasNotAnyInfo);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return Application.Context.Resources?.GetString(Resource.String.Lbl_HasNotAnyInfo);
            }
        }

        public static string GetGender(string type)
        {
            try
            {
                string text;
                switch (type)
                {
                    case "Male":
                    case "male":
                        text = Application.Context.GetText(Resource.String.Lbl_Male);
                        break;
                    case "Female":
                    case "female":
                        text = Application.Context.GetText(Resource.String.Lbl_Female);
                        break;
                    default:
                        text = "";
                        break;
                }
                return text;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return "";
            }
        }

        public static List<SoundDataObject> ListFilter(List<SoundDataObject> list)
        {
            try
            {
                if (list == null || list?.Count == 0)
                    return new List<SoundDataObject>();

                const string sDuration = "0:0";
                const string mDuration = "00:00";
                const string hDuration = "00:00:00";

                list.RemoveAll(a => a.Duration is sDuration or mDuration or hDuration || string.IsNullOrEmpty(a.AudioId));
                list.RemoveAll(a => a.Availability == 1 && a.UserId != UserDetails.UserId);
                List<SoundDataObject> result = list.Except(ListUtils.GlobalNotInterestedList).ToList();

                foreach (var sound in result)
                {
                    if (!sound.Thumbnail.StartsWith("http"))
                    {
                        sound.Thumbnail = GetTheFinalLink(sound.Thumbnail);
                    }

                    if (!sound.AudioLocation.StartsWith("http"))
                    {
                        sound.AudioLocation = GetTheFinalLink(sound.AudioLocation);
                    }

                    if (sound.Price != 0 && !string.IsNullOrEmpty(sound.DemoTrack))
                    {
                        if (!sound.DemoTrack.StartsWith("http"))
                            sound.DemoTrack = GetTheFinalLink(sound.DemoTrack);
                    } 
                }

                return new List<SoundDataObject>(result);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                if (list == null || list?.Count == 0)
                    return new List<SoundDataObject>();
                else
                    return new List<SoundDataObject>(list);
            }
        }

        /// <summary>
        /// var ImageUrl = !item.Thumbnail.Contains(DeepSoundClient.InitializeDeepSound.WebsiteUrl) ? DeepSoundTools.GetTheFinalLink(item.Thumbnail) : item.Thumbnail;
        /// ['amazone_s3'] == 1   >> https://bucket.s3.amazonaws.com . '/' . $media;
        /// ['ftp_upload'] == 1   >> "http://".$wo['config']['ftp_endpoint'] . '/' . $media;
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public static string GetTheFinalLink(string media)
        {
            try
            {
                var path = media;

                if (!media.Contains(InitializeDeepSound.WebsiteUrl))
                    path = InitializeDeepSound.WebsiteUrl + "/" + media;

                var config = ListUtils.SettingsSiteList;
                if (!string.IsNullOrEmpty(config?.S3Upload) && config?.S3Upload == "on")
                {
                    return "https://" + config.S3BucketName + ".s3.amazonaws.com" + "/" + media;
                }

                if (!string.IsNullOrEmpty(config?.FtpUpload) && config?.FtpUpload == "on")
                {
                    return config.FtpEndpoint + "/" + media;
                }

                if (!string.IsNullOrEmpty(config?.Spaces) && config?.Spaces == "on")
                {

                    if (string.IsNullOrEmpty(config?.SpacesKey) || string.IsNullOrEmpty(config?.SpacesSecret) || string.IsNullOrEmpty(config?.SpaceRegion) || string.IsNullOrEmpty(config?.SpaceName))
                    {
                        return InitializeDeepSound.WebsiteUrl + "/" + media;
                    }

                    return "https://" + config?.SpaceName + "." + config?.SpaceRegion + ".digitaloceanspaces.com/" + media;
                }

                return path;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return media;
            }
        }

        public static bool CheckAllowedFileUpload()
        {
            try
            {
                var dataSettings = ListUtils.SettingsSiteList;
                if (dataSettings?.WhoCanUpload == "admin") //just admin 
                {
                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault()?.Admin;
                    if (dataUser == 0) // Not Admin
                    {
                        return false;
                    }
                }
                else if (dataSettings?.WhoCanUpload == "artist") //just artist user  
                {
                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault()?.Artist;
                    if (dataUser == 0) // Not Artist 
                    {
                        return false;
                    }
                }
                else  //"all"
                {
                    return true;
                }

                return true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;
            }
        }

        public static bool CheckAllowedDownloadFile()
        {
            try
            {
                if (!AppSettings.AllowOfflineDownload)
                    return false;

                var dataSettings = ListUtils.SettingsSiteList;
                if (dataSettings?.WhoCanDownload == "admin") //just admin 
                {
                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault()?.Admin;
                    if (dataUser == 0) // Not Admin
                    {
                        return false;
                    }
                }
                else if (dataSettings?.WhoCanDownload == "artist") //just artist user  
                {
                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault()?.Artist;
                    if (dataUser == 0) // Not Artist 
                    {
                        return false;
                    }
                }
                else if (dataSettings?.WhoCanDownload == "pro") //just pro user  
                {
                    var dataUser = ListUtils.MyUserInfoList?.FirstOrDefault()?.IsPro;
                    if (dataUser == 0) // Not Pro 
                    {
                        return false;
                    }
                }
                else  //"all"
                {
                    return true;
                }

                return true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return true;
            }
        }

        public static bool GetStatusAds()
        {
            try
            {
                switch (AppSettings.ShowAds)
                {
                    case ShowAds.AllUsers:
                        return true;
                    case ShowAds.UnProfessional:
                        {
                            var isPro = ListUtils.MyUserInfoList?.FirstOrDefault()?.IsPro ?? 0;
                            if (isPro == 0)
                                return true;
                            else
                                return false;
                        }
                    default:
                        return false;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static bool CheckWallet(long price)
        {
            try
            {
                var wallet = ListUtils.MyUserInfoList.FirstOrDefault()?.WalletFormat.Replace(",", "") ?? "0";
                if (!string.IsNullOrEmpty(wallet) && wallet != "0")
                {
                    bool isParable = double.TryParse(wallet, out var number);
                    if (isParable)
                    {
                        if (number >= price)
                        {
                            return true;
                        }

                        return false;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;
            }
        }

        public static Dictionary<string, string> GetTimezonesList()
        {
            try
            {
                var arrayAdapter = new Dictionary<string, string>
                {
                    {"Pacific/Midway", "(GMT-11:00) Midway Island"},
                    {"US/Samoa", "(GMT-11:00) Samoa"},
                    {"US/Hawaii", "(GMT-10:00) Hawaii"},
                    {"US/Alaska", "(GMT-09:00) Alaska"},
                    {"US/Pacific", "(GMT-08:00) Pacific Time (US &amp; Canada)"},
                    {"America/Tijuana", "(GMT-08:00) Tijuana"},
                    {"US/Arizona", "(GMT-07:00) Arizona"},
                    {"US/Mountain", "(GMT-07:00) Mountain Time (US &amp; Canada)"},
                    {"America/Chihuahua", "(GMT-07:00) Chihuahua"},
                    {"America/Mazatlan", "(GMT-07:00) Mazatlan"},
                    {"America/Mexico_City", "(GMT-06:00) Mexico City"},
                    {"America/Monterrey", "(GMT-06:00) Monterrey"},
                    {"Canada/Saskatchewan", "(GMT-06:00) Saskatchewan"},
                    {"US/Central", "(GMT-06:00) Central Time (US &amp; Canada)"},
                    {"US/Eastern", "(GMT-05:00) Eastern Time (US &amp; Canada)"},
                    {"US/East-Indiana", "(GMT-05:00) Indiana (East)"},
                    {"America/Bogota", "(GMT-05:00) Bogota"},
                    {"America/Lima", "(GMT-05:00) Lima"},
                    {"America/Caracas", "(GMT-04:30) Caracas"},
                    {"Canada/Atlantic", "(GMT-04:00) Atlantic Time (Canada)"},
                    {"America/La_Paz", "(GMT-04:00) La Paz"},
                    {"America/Santiago", "(GMT-04:00) Santiago"},
                    {"Canada/Newfoundland", "(GMT-03:30) Newfoundland"},
                    {"America/Buenos_Aires", "(GMT-03:00) Buenos Aires"},
                    {"Greenland", "(GMT-03:00) Greenland"},
                    {"Atlantic/Stanley", "(GMT-02:00) Stanley"},
                    {"Atlantic/Azores", "(GMT-01:00) Azores"},
                    {"Atlantic/Cape_Verde", "(GMT-01:00) Cape Verde Is."},
                    {"Africa/Casablanca", "(GMT) Casablanca"},
                    {"Europe/Dublin", "(GMT) Dublin"},
                    {"Europe/Lisbon", "(GMT) Lisbon"},
                    {"Europe/London", "(GMT) London"},
                    {"Africa/Monrovia", "(GMT) Monrovia"},
                    {"Europe/Amsterdam", "(GMT+01:00) Amsterdam"},
                    {"Europe/Belgrade", "(GMT+01:00) Belgrade"},
                    {"Europe/Berlin", "(GMT+01:00) Berlin"},
                    {"Europe/Bratislava", "(GMT+01:00) Bratislava"},
                    {"Europe/Brussels", "(GMT+01:00) Brussels"},
                    {"Europe/Budapest", "(GMT+01:00) Budapest"},
                    {"Europe/Copenhagen", "(GMT+01:00) Copenhagen"},
                    {"Europe/Ljubljana", "(GMT+01:00) Ljubljana"},
                    {"Europe/Madrid", "(GMT+01:00) Madrid"},
                    {"Europe/Paris", "(GMT+01:00) Paris"},
                    {"Europe/Prague", "(GMT+01:00) Prague"},
                    {"Europe/Rome", "(GMT+01:00) Rome"},
                    {"Europe/Sarajevo", "(GMT+01:00) Sarajevo"},
                    {"Europe/Skopje", "(GMT+01:00) Skopje"},
                    {"Europe/Stockholm", "(GMT+01:00) Stockholm"},
                    {"Europe/Vienna", "(GMT+01:00) Vienna"},
                    {"Europe/Warsaw", "(GMT+01:00) Warsaw"},
                    {"Europe/Zagreb", "(GMT+01:00) Zagreb"},
                    {"Europe/Athens", "(GMT+02:00) Athens"},
                    {"Europe/Bucharest", "(GMT+02:00) Bucharest"},
                    {"Africa/Cairo", "(GMT+02:00) Cairo"},
                    {"Africa/Harare", "(GMT+02:00) Harare"},
                    {"Europe/Helsinki", "(GMT+02:00) Helsinki"},
                    {"Europe/Istanbul", "(GMT+02:00) Istanbul"},
                    {"Asia/Jerusalem", "(GMT+02:00) Jerusalem"},
                    {"Europe/Kiev", "(GMT+02:00) Kyiv"},
                    {"Europe/Minsk", "(GMT+02:00) Minsk"},
                    {"Europe/Riga", "(GMT+02:00) Riga"},
                    {"Europe/Sofia", "(GMT+02:00) Sofia"},
                    {"Europe/Tallinn", "(GMT+02:00) Tallinn"},
                    {"Europe/Vilnius", "(GMT+02:00) Vilnius"},
                    {"Asia/Baghdad", "(GMT+03:00) Baghdad"},
                    {"Asia/Kuwait", "(GMT+03:00) Kuwait"},
                    {"Africa/Nairobi", "(GMT+03:00) Nairobi"},
                    {"Asia/Riyadh", "(GMT+03:00) Riyadh"},
                    {"Europe/Moscow", "(GMT+03:00) Moscow"},
                    {"Asia/Tehran", "(GMT+03:30) Tehran"},
                    {"Asia/Baku", "(GMT+04:00) Baku"},
                    {"Europe/Volgograd", "(GMT+04:00) Volgograd"},
                    {"Asia/Muscat", "(GMT+04:00) Muscat"},
                    {"Asia/Tbilisi", "(GMT+04:00) Tbilisi"},
                    {"Asia/Yerevan", "(GMT+04:00) Yerevan"},
                    {"Asia/Kabul", "(GMT+04:30) Kabul"},
                    {"Asia/Karachi", "(GMT+05:00) Karachi"},
                    {"Asia/Tashkent", "(GMT+05:00) Tashkent"},
                    {"Asia/Kolkata", "(GMT+05:30) Kolkata"},
                    {"Asia/Kathmandu", "(GMT+05:45) Kathmandu"},
                    {"Asia/Yekaterinburg", "(GMT+06:00) Ekaterinburg"},
                    {"Asia/Almaty", "(GMT+06:00) Almaty"},
                    {"Asia/Dhaka", "(GMT+06:00) Dhaka"},
                    {"Asia/Novosibirsk", "(GMT+07:00) Novosibirsk"},
                    {"Asia/Bangkok", "(GMT+07:00) Bangkok"},
                    {"Asia/Jakarta", "(GMT+07:00) Jakarta"},
                    {"Asia/Krasnoyarsk", "(GMT+08:00) Krasnoyarsk"},
                    {"Asia/Chongqing", "(GMT+08:00) Chongqing"},
                    {"Asia/Hong_Kong", "(GMT+08:00) Hong Kong"},
                    {"Asia/Kuala_Lumpur", "(GMT+08:00) Kuala Lumpur"},
                    {"Australia/Perth", "(GMT+08:00) Perth"},
                    {"Asia/Singapore", "(GMT+08:00) Singapore"},
                    {"Asia/Taipei", "(GMT+08:00) Taipei"},
                    {"Asia/Ulaanbaatar", "(GMT+08:00) Ulaan Bataar"},
                    {"Asia/Urumqi", "(GMT+08:00) Urumqi"},
                    {"Asia/Irkutsk", "(GMT+09:00) Irkutsk"},
                    {"Asia/Seoul", "(GMT+09:00) Seoul"},
                    {"Asia/Tokyo", "(GMT+09:00) Tokyo"},
                    {"Australia/Adelaide", "(GMT+09:30) Adelaide"},
                    {"Australia/Darwin", "(GMT+09:30) Darwin"},
                    {"Asia/Yakutsk", "(GMT+10:00) Yakutsk"},
                    {"Australia/Brisbane", "(GMT+10:00) Brisbane"},
                    {"Australia/Canberra", "(GMT+10:00) Canberra"},
                    {"Pacific/Guam", "(GMT+10:00) Guam"},
                    {"Australia/Hobart", "(GMT+10:00) Hobart"},
                    {"Australia/Melbourne", "(GMT+10:00) Melbourne"},
                    {"Pacific/Port_Moresby", "(GMT+10:00) Port Moresby"},
                    {"Australia/Sydney", "(GMT+10:00) Sydney"},
                    {"Asia/Vladivostok", "(GMT+11:00) Vladivostok"},
                    {"Asia/Magadan", "(GMT+12:00) Magadan"},
                    {"Pacific/Auckland", "(GMT+12:00) Auckland"},
                    {"Pacific/Fiji", "(GMT+12:00) Fiji"},
                };

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new Dictionary<string, string>();
            }
        }


        public static bool IsTabDark()
        {
            try
            {
                return AppSettings.SetTabDarkTheme is TabTheme.Dark;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return false;

            }
        }

        public static Dictionary<string, string> GetCountryList(Activity activity)
        {
            try
            {
                var arrayAdapter = new Dictionary<string, string>
                {
                    {"1",  activity.GetString(Resource.String.Lbl_country1)},
                    {"2",  activity.GetString(Resource.String.Lbl_country2)},
                    {"3",  activity.GetString(Resource.String.Lbl_country3)},
                    {"4",  activity.GetString(Resource.String.Lbl_country4)},
                    {"5",  activity.GetString(Resource.String.Lbl_country5)},
                    {"6",  activity.GetString(Resource.String.Lbl_country6)},
                    {"7",  activity.GetString(Resource.String.Lbl_country7)},
                    {"8",  activity.GetString(Resource.String.Lbl_country8)},
                    {"9",  activity.GetString(Resource.String.Lbl_country9)},
                    {"10", activity.GetString(Resource.String.Lbl_country10)},
                    {"11", activity.GetString(Resource.String.Lbl_country11)},
                    {"12", activity.GetString(Resource.String.Lbl_country12)},
                    {"13", activity.GetString(Resource.String.Lbl_country13)},
                    {"14", activity.GetString(Resource.String.Lbl_country14)},
                    {"15", activity.GetString(Resource.String.Lbl_country15)},
                    {"16", activity.GetString(Resource.String.Lbl_country16)},
                    {"17", activity.GetString(Resource.String.Lbl_country17)},
                    {"18", activity.GetString(Resource.String.Lbl_country18)},
                    {"19", activity.GetString(Resource.String.Lbl_country19)},
                    {"20", activity.GetString(Resource.String.Lbl_country20)},
                    {"21", activity.GetString(Resource.String.Lbl_country21)},
                    {"22", activity.GetString(Resource.String.Lbl_country22)},
                    {"23", activity.GetString(Resource.String.Lbl_country23)},
                    {"24", activity.GetString(Resource.String.Lbl_country24)},
                    {"25", activity.GetString(Resource.String.Lbl_country25)},
                    {"26", activity.GetString(Resource.String.Lbl_country26)},
                    {"27", activity.GetString(Resource.String.Lbl_country27)},
                    {"28", activity.GetString(Resource.String.Lbl_country28)},
                    {"29", activity.GetString(Resource.String.Lbl_country29)},
                    {"30", activity.GetString(Resource.String.Lbl_country30)},
                    {"31", activity.GetString(Resource.String.Lbl_country31)},
                    {"32", activity.GetString(Resource.String.Lbl_country32)},
                    {"34", activity.GetString(Resource.String.Lbl_country34)},
                    {"35", activity.GetString(Resource.String.Lbl_country35)},
                    {"36", activity.GetString(Resource.String.Lbl_country36)},
                    {"37", activity.GetString(Resource.String.Lbl_country37)},
                    {"38", activity.GetString(Resource.String.Lbl_country38)},
                    {"39", activity.GetString(Resource.String.Lbl_country39)},
                    {"40", activity.GetString(Resource.String.Lbl_country40)},
                    {"41", activity.GetString(Resource.String.Lbl_country41)},
                    {"42", activity.GetString(Resource.String.Lbl_country42)},
                    {"43", activity.GetString(Resource.String.Lbl_country43)},
                    {"44", activity.GetString(Resource.String.Lbl_country44)},
                    {"45", activity.GetString(Resource.String.Lbl_country45)},
                    {"46", activity.GetString(Resource.String.Lbl_country46)},
                    {"47", activity.GetString(Resource.String.Lbl_country47)},
                    {"48", activity.GetString(Resource.String.Lbl_country48)},
                    {"49", activity.GetString(Resource.String.Lbl_country49)},
                    {"50", activity.GetString(Resource.String.Lbl_country50)},
                    {"51", activity.GetString(Resource.String.Lbl_country51)},
                    {"52", activity.GetString(Resource.String.Lbl_country52)},
                    {"53", activity.GetString(Resource.String.Lbl_country53)},
                    {"54", activity.GetString(Resource.String.Lbl_country54)},
                    {"55", activity.GetString(Resource.String.Lbl_country55)},
                    {"56", activity.GetString(Resource.String.Lbl_country56)},
                    {"57", activity.GetString(Resource.String.Lbl_country57)},
                    {"58", activity.GetString(Resource.String.Lbl_country58)},
                    {"59", activity.GetString(Resource.String.Lbl_country59)},
                    {"60", activity.GetString(Resource.String.Lbl_country60)},
                    {"61", activity.GetString(Resource.String.Lbl_country61)},
                    {"62", activity.GetString(Resource.String.Lbl_country62)},
                    {"63", activity.GetString(Resource.String.Lbl_country63)},
                    {"64", activity.GetString(Resource.String.Lbl_country64)},
                    {"65", activity.GetString(Resource.String.Lbl_country65)},
                    {"66", activity.GetString(Resource.String.Lbl_country66)},
                    {"67", activity.GetString(Resource.String.Lbl_country67)},
                    {"68", activity.GetString(Resource.String.Lbl_country68)},
                    {"69", activity.GetString(Resource.String.Lbl_country69)},
                    {"70", activity.GetString(Resource.String.Lbl_country70)},
                    {"71", activity.GetString(Resource.String.Lbl_country71)},
                    {"72", activity.GetString(Resource.String.Lbl_country72)},
                    {"73", activity.GetString(Resource.String.Lbl_country73)},
                    {"74", activity.GetString(Resource.String.Lbl_country74)},
                    {"75", activity.GetString(Resource.String.Lbl_country75)},
                    {"76", activity.GetString(Resource.String.Lbl_country76)},
                    {"77", activity.GetString(Resource.String.Lbl_country77)},
                    {"78", activity.GetString(Resource.String.Lbl_country78)},
                    {"79", activity.GetString(Resource.String.Lbl_country79)},
                    {"80", activity.GetString(Resource.String.Lbl_country80)},
                    {"81", activity.GetString(Resource.String.Lbl_country81)},
                    {"82", activity.GetString(Resource.String.Lbl_country82)},
                    {"83", activity.GetString(Resource.String.Lbl_country83)},
                    {"84", activity.GetString(Resource.String.Lbl_country84)},
                    {"85", activity.GetString(Resource.String.Lbl_country85)},
                    {"86", activity.GetString(Resource.String.Lbl_country86)},
                    {"87", activity.GetString(Resource.String.Lbl_country87)},
                    {"88", activity.GetString(Resource.String.Lbl_country88)},
                    {"89", activity.GetString(Resource.String.Lbl_country89)},
                    {"90", activity.GetString(Resource.String.Lbl_country90)},
                    {"91", activity.GetString(Resource.String.Lbl_country91)},
                    {"92", activity.GetString(Resource.String.Lbl_country92)},
                    {"93", activity.GetString(Resource.String.Lbl_country93)},
                    {"94", activity.GetString(Resource.String.Lbl_country94)},
                    {"95", activity.GetString(Resource.String.Lbl_country95)},
                    {"96", activity.GetString(Resource.String.Lbl_country96)},
                    {"97", activity.GetString(Resource.String.Lbl_country97)},
                    {"98", activity.GetString(Resource.String.Lbl_country98)},
                    {"99", activity.GetString(Resource.String.Lbl_country99)},
                    {"100",activity.GetString(Resource.String.Lbl_country100)},
                    {"101",activity.GetString(Resource.String.Lbl_country101)},
                    {"102",activity.GetString(Resource.String.Lbl_country102)},
                    {"103",activity.GetString(Resource.String.Lbl_country103)},
                    {"104",activity.GetString(Resource.String.Lbl_country104)},
                    {"105",activity.GetString(Resource.String.Lbl_country105)},
                    {"106",activity.GetString(Resource.String.Lbl_country106)},
                    {"107",activity.GetString(Resource.String.Lbl_country107)},
                    {"108",activity.GetString(Resource.String.Lbl_country108)},
                    {"109",activity.GetString(Resource.String.Lbl_country109)},
                    {"110",activity.GetString(Resource.String.Lbl_country110)},
                    {"111",activity.GetString(Resource.String.Lbl_country111)},
                    {"112",activity.GetString(Resource.String.Lbl_country112)},
                    {"113",activity.GetString(Resource.String.Lbl_country113)},
                    {"114",activity.GetString(Resource.String.Lbl_country114)},
                    {"115",activity.GetString(Resource.String.Lbl_country115)},
                    {"116",activity.GetString(Resource.String.Lbl_country116)},
                    {"117",activity.GetString(Resource.String.Lbl_country117)},
                    {"118",activity.GetString(Resource.String.Lbl_country118)},
                    {"119",activity.GetString(Resource.String.Lbl_country119)},
                    {"120",activity.GetString(Resource.String.Lbl_country120)},
                    {"121",activity.GetString(Resource.String.Lbl_country121)},
                    {"122",activity.GetString(Resource.String.Lbl_country122)},
                    {"123",activity.GetString(Resource.String.Lbl_country123)},
                    {"124",activity.GetString(Resource.String.Lbl_country124)},
                    {"125",activity.GetString(Resource.String.Lbl_country125)},
                    {"126",activity.GetString(Resource.String.Lbl_country126)},
                    {"127",activity.GetString(Resource.String.Lbl_country127)},
                    {"128",activity.GetString(Resource.String.Lbl_country128)},
                    {"129",activity.GetString(Resource.String.Lbl_country129)},
                    {"130",activity.GetString(Resource.String.Lbl_country130)},
                    {"131",activity.GetString(Resource.String.Lbl_country131)},
                    {"132",activity.GetString(Resource.String.Lbl_country132)},
                    {"133",activity.GetString(Resource.String.Lbl_country133)},
                    {"134",activity.GetString(Resource.String.Lbl_country134)},
                    {"135",activity.GetString(Resource.String.Lbl_country135)},
                    {"136",activity.GetString(Resource.String.Lbl_country136)},
                    {"137",activity.GetString(Resource.String.Lbl_country137)},
                    {"138",activity.GetString(Resource.String.Lbl_country138)},
                    {"139",activity.GetString(Resource.String.Lbl_country139)},
                    {"140",activity.GetString(Resource.String.Lbl_country140)},
                    {"141",activity.GetString(Resource.String.Lbl_country141)},
                    {"142",activity.GetString(Resource.String.Lbl_country142)},
                    {"143",activity.GetString(Resource.String.Lbl_country143)},
                    {"144",activity.GetString(Resource.String.Lbl_country144)},
                    {"145",activity.GetString(Resource.String.Lbl_country145)},
                    {"146",activity.GetString(Resource.String.Lbl_country146)},
                    {"147",activity.GetString(Resource.String.Lbl_country147)},
                    {"148",activity.GetString(Resource.String.Lbl_country148)},
                    {"149",activity.GetString(Resource.String.Lbl_country149)},
                    {"150",activity.GetString(Resource.String.Lbl_country150)},
                    {"151",activity.GetString(Resource.String.Lbl_country151)},
                    {"152",activity.GetString(Resource.String.Lbl_country152)},
                    {"153",activity.GetString(Resource.String.Lbl_country153)},
                    {"154",activity.GetString(Resource.String.Lbl_country154)},
                    {"155",activity.GetString(Resource.String.Lbl_country155)},
                    {"156",activity.GetString(Resource.String.Lbl_country156)},
                    {"157",activity.GetString(Resource.String.Lbl_country157)},
                    {"158",activity.GetString(Resource.String.Lbl_country158)},
                    {"159",activity.GetString(Resource.String.Lbl_country159)},
                    {"160",activity.GetString(Resource.String.Lbl_country160)},
                    {"161",activity.GetString(Resource.String.Lbl_country161)},
                    {"162",activity.GetString(Resource.String.Lbl_country162)},
                    {"163",activity.GetString(Resource.String.Lbl_country163)},
                    {"164",activity.GetString(Resource.String.Lbl_country164)},
                    {"165",activity.GetString(Resource.String.Lbl_country165)},
                    {"166",activity.GetString(Resource.String.Lbl_country166)},
                    {"167",activity.GetString(Resource.String.Lbl_country167)},
                    {"168",activity.GetString(Resource.String.Lbl_country168)},
                    {"169",activity.GetString(Resource.String.Lbl_country169)},
                    {"170",activity.GetString(Resource.String.Lbl_country170)},
                    {"171",activity.GetString(Resource.String.Lbl_country171)},
                    {"172",activity.GetString(Resource.String.Lbl_country172)},
                    {"173",activity.GetString(Resource.String.Lbl_country173)},
                    {"174",activity.GetString(Resource.String.Lbl_country174)},
                    {"175",activity.GetString(Resource.String.Lbl_country175)},
                    {"176",activity.GetString(Resource.String.Lbl_country176)},
                    {"177",activity.GetString(Resource.String.Lbl_country177)},
                    {"178",activity.GetString(Resource.String.Lbl_country178)},
                    {"179",activity.GetString(Resource.String.Lbl_country179)},
                    {"180",activity.GetString(Resource.String.Lbl_country180)},
                    {"181",activity.GetString(Resource.String.Lbl_country181)},
                    {"182",activity.GetString(Resource.String.Lbl_country182)},
                    {"183",activity.GetString(Resource.String.Lbl_country183)},
                    {"184",activity.GetString(Resource.String.Lbl_country184)},
                    {"185",activity.GetString(Resource.String.Lbl_country185)},
                    {"186",activity.GetString(Resource.String.Lbl_country186)},
                    {"187",activity.GetString(Resource.String.Lbl_country187)},
                    {"188",activity.GetString(Resource.String.Lbl_country188)},
                    {"189",activity.GetString(Resource.String.Lbl_country189)},
                    {"190",activity.GetString(Resource.String.Lbl_country190)},
                    {"191",activity.GetString(Resource.String.Lbl_country191)},
                    {"192",activity.GetString(Resource.String.Lbl_country192)},
                    {"193",activity.GetString(Resource.String.Lbl_country193)},
                    {"194",activity.GetString(Resource.String.Lbl_country194)},
                    {"195",activity.GetString(Resource.String.Lbl_country195)},
                    {"196",activity.GetString(Resource.String.Lbl_country196)},
                    {"197",activity.GetString(Resource.String.Lbl_country197)},
                    {"198",activity.GetString(Resource.String.Lbl_country198)},
                    {"199",activity.GetString(Resource.String.Lbl_country199)},
                    {"200",activity.GetString(Resource.String.Lbl_country200)},
                    {"201",activity.GetString(Resource.String.Lbl_country201)},
                    {"202",activity.GetString(Resource.String.Lbl_country202)},
                    {"203",activity.GetString(Resource.String.Lbl_country203)},
                    {"204",activity.GetString(Resource.String.Lbl_country204)},
                    {"205",activity.GetString(Resource.String.Lbl_country205)},
                    {"206",activity.GetString(Resource.String.Lbl_country206)},
                    {"207",activity.GetString(Resource.String.Lbl_country207)},
                    {"208",activity.GetString(Resource.String.Lbl_country208)},
                    {"209",activity.GetString(Resource.String.Lbl_country209)},
                    {"210",activity.GetString(Resource.String.Lbl_country210)},
                    {"211",activity.GetString(Resource.String.Lbl_country211)},
                    {"212",activity.GetString(Resource.String.Lbl_country212)},
                    {"213",activity.GetString(Resource.String.Lbl_country213)},
                    {"214",activity.GetString(Resource.String.Lbl_country214)},
                    {"215",activity.GetString(Resource.String.Lbl_country215)},
                    {"216",activity.GetString(Resource.String.Lbl_country216)},
                    {"217",activity.GetString(Resource.String.Lbl_country217)},
                    {"218",activity.GetString(Resource.String.Lbl_country218)},
                    {"219",activity.GetString(Resource.String.Lbl_country219)},
                    {"220",activity.GetString(Resource.String.Lbl_country220)},
                    {"221",activity.GetString(Resource.String.Lbl_country221)},
                    {"222",activity.GetString(Resource.String.Lbl_country222)},
                    {"223",activity.GetString(Resource.String.Lbl_country223)},
                    {"224",activity.GetString(Resource.String.Lbl_country224)},
                    {"225",activity.GetString(Resource.String.Lbl_country225)},
                    {"226",activity.GetString(Resource.String.Lbl_country226)},
                    {"227",activity.GetString(Resource.String.Lbl_country227)},
                    {"228",activity.GetString(Resource.String.Lbl_country228)},
                    {"229",activity.GetString(Resource.String.Lbl_country229)},
                    {"230",activity.GetString(Resource.String.Lbl_country230)},
                    {"231",activity.GetString(Resource.String.Lbl_country231)},
                    {"232",activity.GetString(Resource.String.Lbl_country232)},
                    {"233",activity.GetString(Resource.String.Lbl_country233)},
                    {"238",activity.GetString(Resource.String.Lbl_country238)},
                    {"239",activity.GetString(Resource.String.Lbl_country239)},
                    {"240",activity.GetString(Resource.String.Lbl_country240)},
                    {"241",activity.GetString(Resource.String.Lbl_country241)},
                    {"242",activity.GetString(Resource.String.Lbl_country242)},
                };

                return arrayAdapter;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return new Dictionary<string, string>();
            }
        }

        // Functions Save Images
        public static async void SaveFile(string id, string folder, string fileName, string url)
        {
            try
            {
                if (url.Contains("http"))
                {
                    string folderDestination = folder + id + "/";

                    string filePath = Path.Combine(folderDestination);
                    string mediaFile = filePath + "/" + fileName;

                    if (File.Exists(mediaFile)) return;

                    HttpClient client;
                    if (AppSettings.TurnSecurityProtocolType3072On)
                    {
                        HttpClientHandler clientHandler = new HttpClientHandler();
                        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
                        clientHandler.SslProtocols = SslProtocols.Tls | SslProtocols.Ssl2 | SslProtocols.Ssl3 | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13 | SslProtocols.Default;

                        // Pass the handler to httpClient(from you are calling api)
                        client = new HttpClient(clientHandler);
                    }
                    else
                    {
                        client = new HttpClient();
                    }

                    var s = await client.GetStreamAsync(new Uri(url));
                    if (s.CanRead)
                    {
                        if (File.Exists(mediaFile)) return;
                        await using FileStream fs = new FileStream(mediaFile, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
                        await s.CopyToAsync(fs);
                    }

                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        // Functions file from folder
        public static string GetFile(string id, string folder, string filename, string url)
        {
            try
            {
                string folderDestination = folder + id + "/";

                if (!Directory.Exists(folderDestination))
                {
                    if (Directory.Exists(Methods.Path.FolderDiskStory))
                        Directory.Delete(Methods.Path.FolderDiskStory, true);

                    Directory.CreateDirectory(folderDestination);
                }

                string imageFile = Methods.MultiMedia.GetMediaFrom_Gallery(folderDestination, filename);
                switch (imageFile)
                {
                    case "File Dont Exists":
                        //This code runs on a new thread, control is returned to the caller on the UI thread.
                        Task.Factory.StartNew(() => { SaveFile(id, folder, filename, url); });
                        return url;
                    default:
                        return imageFile;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return url;
            }
        }

        public static long GetDuration(string mediaFile)
        {
            if (string.IsNullOrEmpty(mediaFile))
                return 0;

            try
            {
                var type = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                if (type == "Audio")
                {
                    long duration;
                    MediaMetadataRetriever retriever;
                    if (mediaFile.Contains("http"))
                    {
                        retriever = new MediaMetadataRetriever();
                        switch ((int)Build.VERSION.SdkInt)
                        {
                            case >= 14:
                                retriever.SetDataSource(mediaFile, new Dictionary<string, string>());
                                break;
                            default:
                                retriever.SetDataSource(mediaFile);
                                break;
                        }

                        duration = long.Parse(retriever.ExtractMetadata(MetadataKey.Duration) ?? "0"); //time In Millisec 
                        retriever.Release();
                    }
                    else
                    {
                        var file = Android.Net.Uri.FromFile(new Java.IO.File(mediaFile));
                        retriever = new MediaMetadataRetriever();
                        //if ((int)Build.VERSION.SdkInt >= 14)
                        //    retriever.SetDataSource(file.Path, new Dictionary<string, string>());
                        //else
                        retriever.SetDataSource(file?.Path);

                        duration = long.Parse(retriever.ExtractMetadata(MetadataKey.Duration) ?? "0");//time In Millisec 
                        retriever.Release();
                    }
                    return duration;
                }

                return 0;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return 0;
            }
        }
    }
}
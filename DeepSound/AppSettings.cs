using DeepSound.Helpers.Model;

namespace DeepSound
{
    //For the accuracy of the icon and logo, please use this website " https://appicon.co " and add images according to size in folders " mipmap " 

    internal static class AppSettings
    {
        /// <summary>
        /// Deep Links To App Content
        /// you should add your website without http in the analytic.xml file >> ../values/analytic.xml .. line 5
        /// <string name="ApplicationUrlWeb">deepsoundscript.com</string>
        /// </summary>  
        public static readonly string Cert = "yGPpfOCvRwkYgu9mIRdWwzQflvo8pgcOpz2uZ4IIOn6DipFKkIZjCOD1K+7NxFUHqUNK9oFgaRmipSMoIwdTCBeI4UUfIgsMfSQezWYoelvohw5e0/IO+S00cD8WD99YZ41BIO4jJASdwnWcBKGXw/FwrcD33zkEdOzpFEDMddpWgzNbMPfvaWOOp65TBvFsKiroh5p/MDM56K2Ak8y5g0BbYHkxobfpRiKMD5kq/i8pOJGODr+nHtuP+rayugAJrUQqxjxoeBwP1Kuc+YQKdnvvUpxH1ZP0Dmn+RhANdC+oj3Iwqh91Vi4K6CQAoh/Buz4GZY+LACETcvX9tQIPh0oivgr/wNh/aJl4x6NEwkpy7yzT6S06oxFGfIn4lQA1L6ER+4yI26Frkzwawv/HjvdJrrlLXe8ScycKdnmiUYFGThCYVjEErlMWKmtHBN9lhxWvjvhj6MQMyHs268e0HHDJC9EhZ4iRkJSzrCEOlo8jV4iHZRGahQb3DLlo5Y+Ty4MPrLuhLB7wIgWFveh1Rj9dv5w8eREqu91K1yxLFW9lzydsbpu7zsFQItv4azl0UWehDtNdfDhWzV0cQSfCqJ8kcsPK8M8A5IFVbYuhJyEEwJrslMDtLG5RUvYKBw8nJJMS7FQnIDI0tJNXFsBN07I/2x93UFVZca9/XfKsvP1eT7FUPv0Ys2jtlP+yEIWuO5xwhRNaaaolmTIlt0K17wuyNEliKdGmny9Owu8O9g7LoSZXgeCeCeNvKZj5ijcrmQDjpJtUMfuNN/STe4nW/Qyj31XTVNDHn3jjwIHIBCT9+9Y9lgBgvFYq0UulKCr10sM6GeKaqQQkvDKN2v/QSWL13A2DoukfDFZr284zIAKnJvhh1FSMlBfjRwyhccw73pHcUYZOKVDhKRr5+sTDshrrYYctE4YOUodGkqvIYZZaDWdADuvOk9rV2eLVelzc/Gkn+VBBOFr3uI63TeYAG6lar3rz9nfFXEpOapIgqn8TaqDGvbo2SpH8HLbfAUSB1rgYlGiMK7Wpf7p+QY4np5+BocYpdHfOAD/Un2uRNhuezKjtfqHv2ty1/jKAvsrLnNR1uSr09nDuo4wtu7upzasX8WQhnVhuFZY2hwOiLFUWg5Zjad62OF8c98s3EAG7h6lCvjt1W0j2w1c3BrEahMW07J1ro+X6QLVv3eW2+ZNU+Oz48IFBFYwQyl9ib1iMucgnG9GiEegerd55u4oIX3+fS97Ivcl+ZdMvYVcDCN3FAfTQLlBN4U4sKKDvn9CGRAhgQzqrCICfCdeQSG2aAWtlY8UqnenH/JdWyG7kRFZvC7gGTR6cqpfs6qXe8noPvLPKMpBC8gqlzM0S8qHt9pQs9LwyFVJlPS6udXZv7kvjz2d+ZVkgWFRKGrfn/IYV3KNyVJhSFOchxiSpoZJP0sMHArZ4FQY1gT55NPwLLsXf2Bm4DQH585cGq9lZhEetCIKO2NIZXfOmtu50nsibapJeCtUDnQka79eMN15LaW6RZLpx1PPmcT0aQl3O9YdskBA1EAy+ufBB8Odt1gFsXmfLv/hGcX9U/LXjb3mdlm67r7FsL1bwfDz5jD0z8kFbTrKAdpdD07r2MQWMyqgXBBEk2QM2Xm/+7tDdDXYw9qBtDVziRZ9oN0S1diPHgNVneQb7TmDfUWGvQNjbHXV337A+pqP7cnocr3NgkvYipllpGrUgfNAZbi7GNcrh22ivyXQujaRIdzfS6p7EblxpnLTliTj1q11MjpxpIMJ0UclT6gDgJIQ9GEsrAMFC+0XLM2beW+tdktaB5YfA1/zJb6cC3Ur9q7gBXHLpq/uc47O8esXZ/lyIdlvSRY1l+djiYROy6+yAxedRA7jQB4/YTB0/GVtHWmxhULdP+zrfz5qfdWiP7uJ9U6xWGYGAPurfyYU6gxH1QV8bBvx1ip07W9SERo/gyfNfIrRtGhNgU7QDZH1cPIs5TvsnnnquW1zWPO3pG5ksUA7AnS4Jqo2g6A/T5VpzYiZcRXbcR2u1NMXQWY06ZT6oaKrPPARuLkrp3CRIqYuv7ZX3WTnI/pjCJwTvDEWurcUH0Eyv6dCN+NkYCfJzaBBPkJNMGhXjjYUnEOTB6zIrYlniQc71mxkEsxeZmKMw9B3w7fn7/t1pqAtV2C3tMmXLPtLBSjanSXxoun7fLOfPbUgYhUUfCXd5xotvdZJp9c5s+CxbzdKIjpKdrID/56J4/G448auN0UUPoM/5U2w8m3V+qqHtVnJZgioE0tKKEmegWLZCTf0eHX9hkkcrH/sLLgbDpixaJHPUbJaDfBKVVIWgqKPE6zT0z5TQW3wWMt0zX/cmXDRQEhzZK+nTzxg0wm9PYj1kPdVuUa1axMLTomOhKQAjCGe2j+y2t1mde8AYrQzk8WHo6xY2pjyuA4OFE1lNPgIKoxi5v6mH77jrfcsq75ENveHBdFeO2SV85GL5FNAqRfs6Gl5wnNtGiHQfshmUf8iABxo8QimIwoCN29btFyClJs9MEwfmOzJGjiZjnjgwB2Z+itzA9vSim5GDl7gh/i1laAKFe03USfTlkDraLm8dkWmwPBTlUtPpaun1PBvqLdhivb2peVHnHF9TmmHEKQwk82JBqimjJYCg4jashIRKV9X38B9v8bhDmGhRM041HDh6IuH8Pr1EbdUJk2E8G4jdybAkxTwjCBUA1KifzhXZ3kkNYclr5JYK2JRDZn7RFqUciEgXsrp7opDiMC7d0Ndjc7y/fDaUOsc9/nUdNp80TS9lOZxrL32Fxy8wzu72D4OPKCRLtAtopRXHXXaPywW96fJpjnRsXz6eNtLznF+ttU3cYSwYca6EgzUfrQjoyQ9RiKtnWda20bPKtyl6Lh5B9biXnkRFUGgfu808MYGlAkfd6e8yzeiCXi4SVEQSyzXPl4vFKB08SfPF6ANCh1vXqmK1KGK5+sHDQLo5HM8akITZLHNeTmxAQahKVRLLJstZs/AydnKKMTqrcJfwpkndPhnczHpR2Oz24U98z2jqkLK2kxGfhv6HxgWlqGBYMKPGUas7ibr0izWeB5c5OJeEOZ1TVuyjMWwIy+/B050/5GxF4p+dZjx+zApSZ/EtWPyx0TCUuEad0PwSeqxQa/Z5EibU2hYumZ6ubWgJQJoNnt6Xiz7vddaQL4JHfCuUn40GYSpJ07HxbIPDnjTrwLdPStUgcpUmjTir31HXj1ev7Z2n1od9nq/pAj28AQ77Na5Hab+dC62HHIghQioNTtVKPGofHWOaBWQBLV1YPqHIHssSs6q7gJC9EtxUCbqjyrXf7XnYC4U1jgQdy+1sDyNsFD6gNnxeZFWKh+bEf+SGgRyfscnQbp+r9ctHEcljlFIo/GwO6Sk2frYYAv6rRkY+bXVu+GU1OtDh1TI+iAvIJE7bUAigESZ3Q4R0R/Gv7SmKxYmRkHoCYe4BWDqS5q76KlODH2b11bJzaVXnIB9f6GkmrsuSL8hfRvXmZkU2JN5iUUiHUdQn7wXLzot0YgqXOrMcXmjYf2iWrzESozsWCTXPfcdj1WRHCytGZxF3AIpLVeFsVuYFvQ67tsFgx8TLqoH+xyWOZyRB2a5+Xqt/9ihcitCaWCOaVR404XC5ObPdk3tYcoUEBLcG/1UU2QRcLU7KgwRPZV6C01Q4omtgHYOfu7XbMBx+JirwGEVcRU5e1gX3A44NyvCUvjlCemMAcTwraw1W1Dw4gR2MH6fheGfN4eA+PYlr8hWMmdgPyYd4D2H8Gptqm72yy8JUYfqRGi6qs5lsyDee5cZXECaKiAi0HUCGB7jW2OZEvi8CDuekQ4x5wselhHktnPTMeBJ+hTiVgVnPOcreBFy9Lo8zooUHuBKrIW+UXP02OI+El/4X3Tglao/7/FJBKaj8r+iMTZzVVXCW4QBGQeItupwsTfkKgY7kvY0uNVa5TGmaKr4wHsTKF6DNefxWHJ+dNJm90ush/iOxjtvaCs+YJ+akBNCmWAza0koWkldM";

        //Main Settings >>>>>
        //*********************************************************
        public static readonly string ApplicationName = "DeepSound";
        public static readonly string DatabaseName = "DeepSoundSong";
        public static string Version = "3.3";

        //Main Colors >>
        //*********************************************************
        public static readonly string MainColor = "#FF8216";

        //Language Settings >> http://www.lingoes.net/en/translator/langcode.htm
        //*********************************************************
        public static bool FlowDirectionRightToLeft = false;
        public static string Lang = ""; //Default language ar_AE

        //Error Report Mode
        //*********************************************************
        public static readonly bool SetApisReportMode = false;

        //Notification Settings >>
        //*********************************************************
        public static bool ShowNotification = true;
        public static string OneSignalAppId = "bfc2e195-6c3c-4123-97ce-1e122f5809a9";

        //AdMob >> Please add the code ads in the Here and analytic.xml 
        //*********************************************************
        public static readonly ShowAds ShowAds = ShowAds.AllUsers;

        public static readonly bool RewardedAdvertisingSystem = true; //#New

        //Three times after entering the ad is displayed
        public static readonly int ShowAdInterstitialCount = 5;
        public static readonly int ShowAdRewardedVideoCount = 5;
        public static int ShowAdNativeCount = 7;
        public static readonly int ShowAdAppOpenCount = 3;

        public static readonly bool ShowAdMobBanner = true;
        public static readonly bool ShowAdMobInterstitial = true;
        public static readonly bool ShowAdMobRewardVideo = true;
        public static readonly bool ShowAdMobNative = true;
        public static readonly bool ShowAdMobAppOpen = true;
        public static readonly bool ShowAdMobRewardedInterstitial = true;

        public static readonly string AdInterstitialKey = "ca-app-pub-5135691635931982/6646750931";
        public static readonly string AdRewardVideoKey = "ca-app-pub-5135691635931982/6981792857";
        public static readonly string AdAdMobNativeKey = "ca-app-pub-5135691635931982/1394424252";
        public static readonly string AdAdMobAppOpenKey = "ca-app-pub-5135691635931982/1906896275";
        public static readonly string AdRewardedInterstitialKey = "ca-app-pub-5135691635931982/4054725070";

        //FaceBook Ads >> Please add the code ad in the Here and analytic.xml 
        //*********************************************************
        public static readonly bool ShowFbBannerAds = false;
        public static readonly bool ShowFbInterstitialAds = false;
        public static readonly bool ShowFbRewardVideoAds = false;
        public static readonly bool ShowFbNativeAds = false;

        //YOUR_PLACEMENT_ID
        public static readonly string AdsFbBannerKey = "250485588986218_554026418632132";
        public static readonly string AdsFbInterstitialKey = "250485588986218_554026125298828";
        public static readonly string AdsFbRewardVideoKey = "250485588986218_554072818627492";
        public static readonly string AdsFbNativeKey = "250485588986218_554706301897477";

        //Ads AppLovin >> Please add the code ad in the Here 
        //*********************************************************  
        public static readonly bool ShowAppLovinBannerAds = true;
        public static readonly bool ShowAppLovinInterstitialAds = true;
        public static readonly bool ShowAppLovinRewardAds = true;

        public static readonly string AdsAppLovinBannerId = "93a37dd25bd3f699";
        public static readonly string AdsAppLovinInterstitialId = "5fec6909ce79fb49";
        public static readonly string AdsAppLovinRewardedId = "3fdddf11aca6ce57";
        //********************************************************* 

        //Social Logins >>
        //If you want login with facebook or google you should change id key in the analytic.xml file  
        //Facebook >> ../values/analytic.xml .. line 10 - 11
        //Google >> ../values/analytic.xml .. line 15
        //*********************************************************
        public static readonly bool EnableSmartLockForPasswords = false;

        public static readonly bool ShowFacebookLogin = true;
        public static readonly bool ShowGoogleLogin = true;
        public static readonly bool ShowWoWonderLogin = true;

        public static readonly string ClientId = "104516058316-9vjdctmsk63o35nbpp872is04qqa84vc.apps.googleusercontent.com";

        public static string AppNameWoWonder = "WoWonder";
        public static readonly string WoWonderDomainUri = "https://demo.wowonder.com";
        public static readonly string WoWonderAppKey = "131c471c8b4edf662dd0ebf7adf3c3d7365838b9";

        //*********************************************************
        public static readonly bool ShowPrice = true;
        public static readonly bool ShowSkipButton = true;

        //in album
        public static readonly bool ShowCountPurchases = true;

        public static readonly ShareSystem ShareSystem = ShareSystem.ApplicationShortUrl; //#New 

        //Set Theme Full Screen App
        //*********************************************************
        public static readonly bool EnableFullScreenApp = false;

        public static readonly bool EnableOptimizationApp = false;

        public static readonly bool ShowSettingsRateApp = true;
        public static readonly int ShowRateAppCount = 5;

        public static readonly bool ShowSettingsHelp = true;
        public static readonly bool ShowSettingsTermsOfUse = true;
        public static bool ShowSettingsAbout = true;
        public static readonly bool ShowSettingsDeleteAccount = true;

        public static readonly bool ShowSettingsUpdateManagerApp = false;

        public static readonly bool ShowTextWithSpace = false;

        //Set Blur Screen Comment
        //*********************************************************
        public static readonly bool EnableBlurBackgroundComment = true;

        //Set the radius of the Blur. Supported range 0 < radius <= 25
        public static readonly float BlurRadiusComment = 25f;

        //Import && Upload Videos >>  
        //*********************************************************
        public static bool ShowButtonUploadSingleSong { get; set; } = true;
        public static bool ShowButtonUploadAlbum { get; set; } = true;
        public static bool ShowButtonImportSong { get; set; } = true;

        //Story
        //*********************************************************
        public static readonly bool ShowStory = true;
        public static readonly bool EnableStorySeenList = true;

        //Tap profile
        //*********************************************************
        public static readonly bool ShowStore = true;
        public static readonly bool ShowStations = true;
        public static readonly bool ShowPlaylist = true;
        public static readonly bool ShowEvent = true;
        public static readonly bool ShowProduct = true;
        public static readonly bool ShowAdvertise = true;

        //Offline Sound >>  
        //*********************************************************
        public static readonly bool AllowOfflineDownload = true;

        //Profile >>  
        //*********************************************************
        public static readonly bool ShowEmail = true;

        //Player >>  
        //*********************************************************
        public static readonly PlayerTheme PlayerTheme = PlayerTheme.Theme2;

        public static readonly bool ShowForwardTrack = true;
        public static readonly bool ShowBackwardTrack = true;

        //Show Title Album Only on song
        public static readonly bool ShowTitleAlbumOnly = false;


        //Settings Page >>  
        //*********************************************************
        public static readonly bool ShowEditPassword = true;
        public static readonly bool ShowWithdrawals = true;
        public static readonly bool ShowGoPro = true;
        public static readonly bool ShowBlockedUsers = true;
        public static readonly bool ShowBlog = true;
        public static readonly bool ShowSettingsTwoFactor = true;
        public static readonly bool ShowSettingsManageSessions = true;
        public static readonly bool ShowSettingsMyAffiliates = true;
        public static readonly bool ShowBecomeAnArtist = true;

        //Last_Messages Page >>
        //********************************************************* 
        public static readonly bool RunSoundControl = true;
        public static readonly int RefreshAppAPiSeconds = 6000; // 6 Seconds
        public static readonly int MessageRequestSpeed = 3000; // 3 Seconds

        //Set Theme App >> Color - Tab
        //*********************************************************
        public static TabTheme SetTabDarkTheme = TabTheme.Light;

        //Bypass Web Erros 
        //*********************************************************
        public static readonly bool TurnTrustFailureOnWebException = true;
        public static readonly bool TurnSecurityProtocolType3072On = true;

        //Payment System
        //*********************************************************
        /// <summary>
        /// if you want this feature enabled go to Properties -> AndroidManefist.xml and remove comments from below code
        /// <uses-permission android:name="com.android.vending.BILLING" />
        /// </summary>
        public static readonly bool ShowInAppBilling = false;

        /// <summary>
        /// Paypal and google pay using Braintree Gateway https://www.braintreepayments.com/
        /// 
        /// Add info keys in Payment Methods : https://prnt.sc/1z5bffc & https://prnt.sc/1z5b0yj
        /// To find your merchant ID :  https://prnt.sc/1z59dy8
        ///
        /// Tokenization Keys : https://prnt.sc/1z59smv
        /// </summary>
        public static readonly bool ShowPaypal = true;
        public static readonly string MerchantAccountId = "test";

        public static readonly string SandboxTokenizationKey = "sandbox_kt2f6mdh_hf4c******";
        public static readonly string ProductionTokenizationKey = "production_t2wns2y2_dfy45******";

        public static readonly bool ShowBankTransfer = true;
        public static readonly bool ShowCreditCard = true;

        public static readonly bool ShowCashFree = true;

        /// <summary>
        /// Currencies : http://prntscr.com/u600ok
        /// </summary>
        public static readonly string CashFreeCurrency = "INR";

        /// <summary>
        /// If you want RazorPay you should change id key in the analytic.xml file
        /// razorpay_api_Key >> .. line 24 
        /// </summary>
        public static readonly bool ShowRazorPay = true;

        /// <summary>
        /// Currencies : https://razorpay.com/accept-international-payments
        /// </summary>
        public static readonly string RazorPayCurrency = "USD";

        public static readonly bool ShowPayStack = true;
        public static readonly bool ShowPaySera = false;

        public static readonly bool ShowPayUmoney = true;
        public static readonly bool ShowAuthorizeNet = true;
        public static readonly bool ShowSecurionPay = true;
        public static readonly bool ShowIyziPay = true;
        public static readonly bool ShowAamarPay = true;

        /// <summary>
        /// FlutterWave get Api Keys From https://app.flutterwave.com/dashboard/settings/apis/live
        /// </summary>
        public static readonly bool ShowFlutterWave = true;//#New 
        public static readonly string FlutterWaveCurrency = "NGN";//#New 
        public static readonly string FlutterWavePublicKey = "FLWPUBK_TEST-9c877b3110438191127e631c8*****";//#New 
        public static readonly string FlutterWaveEncryptionKey = "FLWSECK_TEST298f1****";//#New 

        //********************************************************* 
        public static readonly bool AllowDeletingDownloadedSongs = true;

    }
}
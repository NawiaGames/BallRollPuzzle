#if HAS_LION_APPLOVIN_SDK
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LionStudios.Suite.Core;
using System.Reflection;
using System;
using LionStudios.Suite.Debugging;


namespace LionStudios.Suite.RemoteConfig
{
    public class RemoteConfig : ILionSettingsProvider
    {
 
        public static RemoteConfigSettings _settings = new RemoteConfigSettings();

        private static HashSet<string> remoteDataValues = new HashSet<string>();

        public class RemoteConfigSettings : ILionSettingsInfo
        {
            [Header("Max Remote Variables")]
            public bool rewardedAdsDisabled = false;
            public bool interstitialsDisabled = false;
            public bool bannersDisabled = false;
            public bool crossPromoDisabled = false;

            public float interstitialTimer;
            public float rvInterstitialTimer;
            public float interstitialStartTimer;
            public int interstitialStartLevel;

            [Header("General")]
            [SerializeField] public RemoteConfigValuesClass[] remoteValArray = new RemoteConfigValuesClass[10];
        }

        public static RemoteConfig.RemoteConfigSettings lionSettings =  new RemoteConfig.RemoteConfigSettings();

  
        public void ApplySettings(ILionSettingsInfo newSettings)
        {
            _settings = (RemoteConfigSettings)newSettings; 
        }  


        public ILionSettingsInfo GetSettings() 
        {
 
            if (_settings == null)
            {
                _settings = new RemoteConfigSettings();
            }
            return _settings;

        }

        [Serializable]
        public class RemoteConfigValuesClass
        {
            public enum myEnum 
            {
                Int_, 
                Float_, 
                Bool_,
                Long_,
                Double_,
                String_
            };

            [SerializeField] public string MaxExperimentVar;
            [SerializeField] public myEnum varType = myEnum.Int_;
            [SerializeField] public string defaultValue;
        } 


        public static T LoadRemoteData<T>(string variableName)
        {
            
            if (variableName == null)
            {
                LionDebug.Log("Aborting - Attempting to load remote data for a null object.");
                return (T) default;
            }
            
            if (MaxSdk.IsInitialized() == false)
            {
                LionDebug.Log("Aborting - Attempting to retrieve remote parameters for an instance before the Max Sdk has initialized. Try using 'AppLovin.WhenInitialized'");
                return  (T) default;
            }

            string remoteValStr = MaxSdk.VariableService.GetString(variableName);
        

            if (!string.IsNullOrEmpty(remoteValStr))
            {
                
                LionDebug.Log($"Remote Variable found from server! : remoteValStr = {remoteValStr}");

                if (bool.TryParse(remoteValStr, out bool value))
                {
                    return (T) Convert.ChangeType(value, typeof(T));
                }

                if (int.TryParse(remoteValStr, out int intValue))
                {
                    return (T) Convert.ChangeType(intValue, typeof(T));
                }
                if (float.TryParse(remoteValStr, out float floatValue))
                {
                    return (T) Convert.ChangeType(floatValue, typeof(T));
                } 

                if (double.TryParse(remoteValStr, out double doubleValue))
                {
                    return (T) Convert.ChangeType(doubleValue, typeof(T));
                }

 
                if (long.TryParse(remoteValStr, out long longValue))
                {
                    return (T) Convert.ChangeType(longValue, typeof(T));
                }
                else
                {
                    return (T) Convert.ChangeType(remoteValStr, typeof(T));
                }

            }
            else
            {
                for (int i = 0; i < _settings.remoteValArray.Length; i++)
                {
                    if (_settings.remoteValArray[i].MaxExperimentVar == variableName)
                    {
                        if (typeof(T) == typeof(int))
                        {
                            if (_settings.remoteValArray[i].varType == RemoteConfigValuesClass.myEnum.Int_)
                            {
                                if (int.TryParse(_settings.remoteValArray[i].defaultValue, out int intValue2))
                                {
                                    return (T) Convert.ChangeType(intValue2, typeof(T));
                                }
                            }
                            else continue;
                        }

                        if (typeof(T) == typeof(bool))
                        {
                            if (_settings.remoteValArray[i].varType == RemoteConfigValuesClass.myEnum.Bool_)
                            {
                                if (bool.TryParse(_settings.remoteValArray[i].defaultValue, out bool boolValue2))
                                {
                                    return (T) Convert.ChangeType(boolValue2, typeof(T));
                                }
                            }
                            else continue;
                        }

                        if (typeof(T) == typeof(float))
                        {
                            if (_settings.remoteValArray[i].varType == RemoteConfigValuesClass.myEnum.Float_)
                            {
                                if (float.TryParse(_settings.remoteValArray[i].defaultValue, out float floatValue2))
                                {
                                    return (T) Convert.ChangeType(floatValue2, typeof(T));
                                }
                            }
                            else continue;
                        }


                        if (typeof(T) == typeof(double))
                        {
                            if (_settings.remoteValArray[i].varType == RemoteConfigValuesClass.myEnum.Double_)
                            {
                                if (double.TryParse(_settings.remoteValArray[i].defaultValue, out double doubleValue2))
                                {
                                    return (T) Convert.ChangeType(doubleValue2, typeof(T));
                                }
                            }
                            else continue;
                        }


                        if (typeof(T) == typeof(long))
                        {
                            if (_settings.remoteValArray[i].varType == RemoteConfigValuesClass.myEnum.Long_)
                            {
                                if (long.TryParse(_settings.remoteValArray[i].defaultValue, out long longValue2))
                                {
                                    return (T) Convert.ChangeType(longValue2, typeof(T));
                                }
                            }
                            else continue;
                        } 

                        if (typeof(T) == typeof(string))
                        {
                            if (_settings.remoteValArray[i].varType == RemoteConfigValuesClass.myEnum.String_)
                            {
                                return (T) Convert.ChangeType(_settings.remoteValArray[i].defaultValue, typeof(T));
                            }
                            else continue;
                        }
                        LionDebug.Log("VARIABLE NOT FOUND IN SETTINGS WINDOW");
                        return (T) default;
                    }
                }
            }
            
            return (T) default;
        }

 

        public static T LoadMaxVar<T>(string variableName)
        {
            
            if (variableName == null)
            {
                LionDebug.Log("Aborting - Attempting to load remote data for a null object.");
                return (T) default;
            }
            
            if (MaxSdk.IsInitialized() == false)
            {
                LionDebug.Log("Aborting - Attempting to retrieve remote parameters for an instance before the Max Sdk has initialized. Try using 'AppLovin.WhenInitialized'");
                return  (T) default;
            }

            string remoteValStr = MaxSdk.VariableService.GetString(variableName);

            if (!string.IsNullOrEmpty(remoteValStr))
            {
                
                LionDebug.Log($"Remote Variable found from server! : remoteValStr = {remoteValStr}");

                if (bool.TryParse(remoteValStr, out bool value))
                {
                    return (T) Convert.ChangeType(value, typeof(T));
                }

                if (int.TryParse(remoteValStr, out int intValue))
                {
                    return (T) Convert.ChangeType(intValue, typeof(T));
                }
                if (float.TryParse(remoteValStr, out float floatValue))
                {
                    return (T) Convert.ChangeType(floatValue, typeof(T));
                }

                if (double.TryParse(remoteValStr, out double doubleValue))
                {
                    return (T) Convert.ChangeType(doubleValue, typeof(T));
                }

 
                if (long.TryParse(remoteValStr, out long longValue))
                {
                    return (T) Convert.ChangeType(longValue, typeof(T));
                }
                else
                {
                    return (T) Convert.ChangeType(remoteValStr, typeof(T));
                }

            }
            else
            {

                Dictionary<string,object> MaxSettings = new Dictionary<string,object>()
                {
                    {"rvInterstitialTimer", _settings.rvInterstitialTimer},
                    {"interstitialTimer", _settings.interstitialTimer},
                    {"interstitialStartLevel", _settings.interstitialStartLevel},
                    {"bannersDisabled", _settings.bannersDisabled},
                    {"crossPromoDisabled", _settings.crossPromoDisabled},
                    {"rewardedAdsDisabled", _settings.rewardedAdsDisabled},
                    {"interstitialsDisabled", _settings.interstitialsDisabled},
                };

                if(MaxSettings.ContainsKey(variableName))
                {
                    return (T) Convert.ChangeType(MaxSettings[variableName],typeof(T));
                }
                else
                {
                    return (T) default; 
                }
                
            }
            
        }

    }
}
#endif
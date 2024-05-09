using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using SmartSongSuggest.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace SmartSongSuggest.Managers
{
    internal class BSMLReflectionsManager
    {
        private static BSMLReflectionsManager _instance;
        private static Assembly BSMLAssembly;

        public static BSMLReflectionsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BSMLReflectionsManager();
                    BSMLAssembly = typeof(MenuButton).Assembly;
                }

                return _instance;
            }
        }

        public void Parse(string content, GameObject parent, object host = null)
        {
            // Get the type of the BSMLParser class dynamically
            Type type = BSMLAssembly.GetType("BeatSaberMarkupLanguage.BSMLParser");
            // Get instance dynamically
            object instance = GetInstance(type);
            // Get the Parse method dynamically
            MethodInfo method = type.GetMethod("Parse", new Type[] { typeof(string), typeof(GameObject), typeof(object) });
            // Call the AddTab method dynamically
            method.Invoke(instance, new object[] { content, parent, host });
        }

        public void AddTab(string name, string resource, object host)
        {
            // Get the type of the GameplaySetup class dynamically
            Type gameplaySetupType = BSMLAssembly.GetType("BeatSaberMarkupLanguage.GameplaySetup.GameplaySetup");
            // Get instance dynamically
            object gameplaySetupInstance = GetInstance(gameplaySetupType);
            // Get the AddTab method dynamically
            MethodInfo addTabMethod = gameplaySetupType.GetMethod("AddTab", new Type[] { typeof(string), typeof(string), typeof(object) });
            // Call the AddTab method dynamically
            addTabMethod.Invoke(gameplaySetupInstance, new object[] { name, resource, host });
        }

        public void RegisterButton(MenuButton menuButton)
        {
            // Get the type of the MenuButtons class dynamically
            Type menuButtonsType = BSMLAssembly.GetType("BeatSaberMarkupLanguage.MenuButtons.MenuButtons");
            // Get instance dynamically
            object menuButtonsInstance = GetInstance(menuButtonsType);
            // Get the AddTab method dynamically
            MethodInfo registerButtonMethod = menuButtonsType.GetMethod("RegisterButton");
            // Call the RegisterButton method dynamically
            registerButtonMethod.Invoke(menuButtonsInstance, new object[] { menuButton });
        }

        private object GetInstance(Type type)
        {
            PropertyInfo buttonsInstanceProperty = type.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            // Get the value of the instance property
            return buttonsInstanceProperty.GetValue(null, null);
        }

    }
}

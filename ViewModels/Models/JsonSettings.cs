using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VkLikerMVVM.ViewModels
{
    public class JsonSettings : BaseVM
    {
        #region Storage

        private readonly object _operations = new object();
        private readonly string _currentSettingsFilePath;

        #endregion

        #region Constructors

        protected JsonSettings()
        {
            _currentSettingsFilePath = GetAbsoluteSettingsPath(GetType().Name);
            Load();
        }

        #endregion

        /// <summary>
        /// Абсолютный путь директории настроек.
        /// </summary>
        public static string SettingsDirectoryPath
        {
            get => Path.Combine(Directory.GetCurrentDirectory(), "Settings");
        }

        /// <summary>
        /// Получить абсолютный путь к файлу настроек с указанным названием.
        /// </summary>
        public static string GetAbsoluteSettingsPath(string fileName)
        {
            if (!fileName.EndsWith(".json"))
            {
                fileName = $"{fileName}.json";
            }

            return Path.Combine(SettingsDirectoryPath, fileName);
        }

        /// <summary>
        /// Сохранить текущий класс в указанный файл.
        /// </summary>
        public void Save()
        {
            lock (_operations)
            {
                if (!Directory.Exists(SettingsDirectoryPath))
                {
                    Directory.CreateDirectory(SettingsDirectoryPath);
                }

                File.WriteAllText(_currentSettingsFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        /// <summary>
        /// Загрузить 
        /// </summary>
        public void Load()
        {
            lock (_operations)
            {
                if (!File.Exists(_currentSettingsFilePath))
                {
                    Save();
                    return;
                }

                string text = File.ReadAllText(_currentSettingsFilePath);
                if (string.IsNullOrWhiteSpace(text))
                {
                    SetSettingsFileInvalid();
                    return;
                }

                try
                {
                    JsonConvert.PopulateObject(text, this);
                }
                catch (JsonException)
                {
                    SetSettingsFileInvalid();
                }
            }
        }

        private void SetSettingsFileInvalid()
        {
            File.Move(_currentSettingsFilePath, $"{_currentSettingsFilePath}.{Environment.TickCount}.invalid");
        }
    }
}

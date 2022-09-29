using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace EdlinSoftware.Safe.Services;

internal interface ILanguagesService
{
    IReadOnlyList<CultureInfo> AvailableLanguages { get; }
    CultureInfo CurrentLanguage { get; set; }
    void LoadLanguage(string cultureName);
}

internal class LanguagesService : ILanguagesService
{
    public LanguagesService()
    {
        AvailableLanguages = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("ru-RU")
        };
    }

    public IReadOnlyList<CultureInfo> AvailableLanguages { get; }

    public CultureInfo CurrentLanguage
    {
        get => Thread.CurrentThread.CurrentUICulture;
        set
        {
            if(value == null) throw new ArgumentNullException(nameof(value));
            if(value.Equals(Thread.CurrentThread.CurrentUICulture)) return;

            Thread.CurrentThread.CurrentUICulture = value;

            ResourceDictionary dict = new ResourceDictionary();
            switch(value.Name){
                case "ru-RU": 
                    dict.Source = new Uri($"Resources/Texts.{value.Name}.xaml", UriKind.Relative);
                    break;
                default:
                    dict.Source = new Uri("Resources/Texts.xaml", UriKind.Relative);
                    break;
            }

            ResourceDictionary oldDict = (from d in Application.Current.Resources.MergedDictionaries
                where d.Source != null && d.Source.OriginalString.StartsWith("Resources/Texts.")
                select d).First();
            if (oldDict != null)
            {
                int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
            } 
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
        }
    }

    public void LoadLanguage(string cultureName)
    {
        CultureInfo culture;

        try
        {
            culture = CultureInfo.GetCultureInfo(cultureName);
        }
        catch
        {
            culture = AvailableLanguages[0];
        }

        CurrentLanguage = culture;
    }

}
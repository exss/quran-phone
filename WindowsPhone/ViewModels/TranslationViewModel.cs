﻿#region

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using QuranPhone.Common;
using QuranPhone.Data;
using QuranPhone.Utils;

#endregion

namespace QuranPhone.ViewModels
{
    public class TranslationViewModel : ViewModelBase
    {
        public const int PAGES_TO_PRELOAD = 2;

        public TranslationViewModel()
        {
            Pages = new ObservableCollection<PageViewModel>();
        }

        private readonly Dictionary<string, DatabaseHandler> translationDatabases =
            new Dictionary<string, DatabaseHandler>();

        private int currentPageIndex;
        private int currentPageNumber;
        private bool showTranslation;
        private string translationFile;

        public string TranslationFile
        {
            get { return translationFile; }
            set
            {
                if (value == translationFile)
                    return;

                translationFile = value;
                if (!translationDatabases.ContainsKey(translationFile))
                    translationDatabases[translationFile] = new DatabaseHandler(translationFile);
                base.OnPropertyChanged(() => TranslationFile);
            }
        }

        public bool ShowTranslation
        {
            get { return showTranslation; }
            set
            {
                if (value == showTranslation)
                    return;

                showTranslation = value;
                changePageShowTranslations();

                base.OnPropertyChanged(() => ShowTranslation);
            }
        }

        public ObservableCollection<PageViewModel> Pages { get; private set; }

        public int CurrentPageNumber
        {
            get { return currentPageNumber; }
            set
            {
                if (value == currentPageNumber)
                    return;

                currentPageNumber = value;
                base.OnPropertyChanged(() => CurrentPageNumber);
            }
        }

        public int CurrentPageIndex
        {
            get { return currentPageIndex; }
            set
            {
                if (value == currentPageIndex)
                    return;

                currentPageIndex = value;
                if (value >= 0)
                    UpdatePages();
                base.OnPropertyChanged(() => CurrentPageIndex);
            }
        }

        public bool IsDataLoaded { get; protected set; }

        /// <summary>
        ///     Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            this.CurrentPageIndex = PAGES_TO_PRELOAD;
            this.IsDataLoaded = true;
        }

        public void UpdatePages()
        {
            if (Pages.Count == 0)
            {
                for (int i = CurrentPageNumber + PAGES_TO_PRELOAD; i >= CurrentPageNumber - PAGES_TO_PRELOAD; i--)
                {
                    var page = (i <= 0 ? Constants.PAGES_LAST + i : i);
                    Pages.Add(new PageViewModel(page) {ShowTranslation = this.ShowTranslation});
                }
            }

            CurrentPageNumber = Pages[CurrentPageIndex].PageNumber;

            if (CurrentPageIndex == PAGES_TO_PRELOAD - 1)
            {
                var lastPage = Pages[Pages.Count - 1].PageNumber;
                var newPage = (lastPage + 1 >= Constants.PAGES_LAST ? Constants.PAGES_LAST - lastPage - 1 : lastPage + 1);
                Pages.Add(new PageViewModel(newPage) { ShowTranslation = this.ShowTranslation });
            }
            else if (CurrentPageIndex == Pages.Count - PAGES_TO_PRELOAD)
            {
                var firstPage = Pages[0].PageNumber;
                var newPage = (firstPage - 1 <= 0 ? Constants.PAGES_LAST + firstPage - 1 : firstPage - 1);
                Pages.Insert(0, new PageViewModel(newPage) { ShowTranslation = this.ShowTranslation });
                CurrentPageIndex++;
            }

            loadPage(CurrentPageIndex, false);
            loadPage(CurrentPageIndex + 1, false);
            loadPage(CurrentPageIndex - 1, false);

            cleanPage(CurrentPageIndex + PAGES_TO_PRELOAD);
            cleanPage(CurrentPageIndex - PAGES_TO_PRELOAD);
        }

        protected void OnDispose()
        {
            foreach (var page in Pages)
            {
                cleanPage(Pages.IndexOf(page));
            }
            foreach (var db in translationDatabases.Keys)
            {
                translationDatabases[db].Dispose();
            }
            translationDatabases.Clear();
        }

        private void changePageShowTranslations()
        {
            foreach (var page in Pages)
            {
                page.ShowTranslation = this.ShowTranslation;
            }
        }

        private void cleanPage(int pageIndex)
        {
            var pageModel = Pages[pageIndex];
            pageModel.ImageSource = null;
            pageModel.Verses.Clear();
        }

        private async void loadPage(int pageIndex, bool force)
        {
            var pageModel = Pages[pageIndex];
            // Set image
            pageModel.ImageSource = QuranFileUtils.GetImageFromWeb(QuranFileUtils.GetPageFileName(pageModel.PageNumber));
            // Set translation
            if (string.IsNullOrEmpty(this.TranslationFile) ||
                !this.translationDatabases.ContainsKey(this.TranslationFile))
                return;

            if (!force && pageModel.Verses.Count > 0)
                return;

            pageModel.Verses.Clear();
            var db = translationDatabases[this.TranslationFile];
            List<QuranAyah> verses = await Task.Run(() => db.GetVerses(pageModel.PageNumber));
            List<QuranAyah> versesArabic = null;

            //if (SettingsUtils.Get<bool>(Constants.PREF_SHOW_ARABIC_IN_TRANSLATION)) 
            //{
            //    try
            //    {
            //        DatabaseHandler dbArabic = new ArabicDatabaseHandler();
            //        versesArabic = dbArabic.GetVerses(pageModel.PageNumber);
            //    }
            //    catch
            //    {
            //        //Not able to get Arabic text - skipping
            //    }
            //}

            int tempSurah = -1;
            for (int i = 0; i < verses.Count; i++)
            {
                var verse = verses[i];
                if (verse.Sura != tempSurah)
                {
                    pageModel.Verses.Add(new VerseViewModel
                        {
                            IsTitle = true,
                            Text = QuranInfo.GetSuraName(verse.Sura, true)
                        });
                    tempSurah = verse.Sura;
                }
                var vvm = new VerseViewModel
                    {
                        IsTitle = false,
                        VerseNumber = verse.Ayah,
                        SurahNumber = verse.Sura,
                        Text = verse.Text
                    };
                if (versesArabic != null && i < versesArabic.Count)
                    vvm.QuranText = versesArabic[i].Text;
                pageModel.Verses.Add(new VerseViewModel
                    {
                        IsTitle = false,
                        VerseNumber = verse.Ayah,
                        SurahNumber = verse.Sura,
                        Text = verse.Text
                    });
            }
        }

        private int inversePageNumber(int number)
        {
            //return Constants.PAGES_LAST - number + 1;
            return number;
        }
    }
}
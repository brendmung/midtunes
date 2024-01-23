namespace DeepSound.Helpers.Model
{
    public class Classes
    {
        public class LibraryItem
        {
            public string SectionId { get; set; }
            public string SectionText { get; set; }
            public int SongsCount { get; set; }
            public string BackgroundImage { get; set; }
        }

        public class Categories
        {
            public string CategoriesId { get; set; }
            public string CategoriesName { get; set; }
            public string CategoriesColor { get; set; }
            public string CategoriesIcon { get; set; }
        }


        public class ItemOptionObject
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public int Icon { get; set; }
            public string Color { get; set; }
        }

    }

    public enum ShowAds
    {
        AllUsers,
        UnProfessional,
    }

    public enum TabTheme
    {
        Dark,
        Light,
    }

    public enum PlayerTheme
    {
        Theme1,
        Theme2,
    }

    public enum ShareSystem
    {
        WebsiteUrl,
        ApplicationShortUrl,
    }

}
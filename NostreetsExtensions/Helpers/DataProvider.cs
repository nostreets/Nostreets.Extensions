using NostreetsExtensions.Interfaces;

namespace NostreetsExtensions.Helpers
{
    public sealed class DataProvider
    {
        private DataProvider() { }

        public static IDao SqlInstance
        {
            get
            {
                return SqlDao.Instance;
            }
        }

    }
}

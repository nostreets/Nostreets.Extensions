using NostreetsExtensions.Interfaces;

namespace NostreetsExtensions.Helpers
{
    public sealed class DataProvider
    {
        private DataProvider() { }

        public static ISqlDao SqlInstance
        {
            get
            {
                return SqlDao.Instance;
            }
        }

        public static IOleDbDao OleDbInstance
        {
            get
            {
                return OleDbDao.Instance;
            }
        }

    }
}

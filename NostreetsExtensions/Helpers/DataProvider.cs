using NostreetsExtensions.Interfaces;

namespace NostreetsExtensions.Helpers
{
    public sealed class DataProvider
    {
        private DataProvider() { }

        public static ISqlExecutor SqlInstance
        {
            get
            {
                return SqlExecutor.Instance;
            }
        }

        public static IOleDbExecutor OleDbInstance
        {
            get
            {
                return OleDbExecutor.Instance;
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThemeClass = JJManager.Class.App.Config.Theme.Theme;
using StartOnBootClass = JJManager.Class.App.Config.OthersConfigs.StartOnBoot;

namespace JJManager.Class.App.Config
{
    public static class Config
    {
        private static DatabaseConnection _dbConnection = new DatabaseConnection();
        private static ThemeClass _theme = new ThemeClass(_dbConnection);
        private static StartOnBootClass _startOnBoot = new StartOnBootClass();

        public static ThemeClass Theme
        {
            get => _theme;
        }

        public static StartOnBootClass StartOnBoot
        {
            get => _startOnBoot;
        }
    }
}

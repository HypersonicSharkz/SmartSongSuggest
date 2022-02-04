using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSongSuggest.UI
{
    class MapListController : BSMLAutomaticViewController, TableView.IDataSource
    {
        public TableCell CellForIdx(TableView tableView, int idx)
        {
            throw new NotImplementedException();
        }

        public float CellSize() => 14f;

        public int NumberOfCells()
        {
            throw new NotImplementedException();
        }
    }
}

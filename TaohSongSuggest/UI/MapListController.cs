using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;

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

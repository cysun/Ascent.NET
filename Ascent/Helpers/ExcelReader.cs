using NPOI.SS.UserModel;

namespace Ascent.Helpers
{
    public class ExcelReader : IDisposable
    {
        private readonly IWorkbook _workbook;
        private readonly ISheet _sheet;
        private readonly DataFormatter _dataFormatter;

        private int _currentRowIndex = -1;
        private string[] _currentRow;
        private int _currentEmptyCellCount;

        private readonly Dictionary<string, int> _colIndexes = new Dictionary<string, int>();

        public int RowCount => _sheet.PhysicalNumberOfRows;
        public int ColCount => _sheet.GetRow(0).PhysicalNumberOfCells;

        public ExcelReader(Stream input)
        {
            _workbook = WorkbookFactory.Create(input);
            _sheet = _workbook.GetSheetAt(0);
            _currentRow = new string[ColCount];

            // NPOI's DataFormatter doesn't seem to support POI's setUseCachedValuesForFormulaCells() yet. See
            // https://stackoverflow.com/questions/7608511/java-poi-how-to-read-excel-cell-value-and-not-the-formula-computing-it
            _dataFormatter = new DataFormatter();

            Next(); // First row should be a header row
            for (int i = 0; i < ColCount; ++i)
                if (_currentRow[i] != "")
                    _colIndexes.Add(_currentRow[i], i);
        }

        public bool HasNext() => _currentRowIndex + 1 < RowCount;

        public bool Next()
        {
            if (++_currentRowIndex >= RowCount) return false;

            _currentEmptyCellCount = 0;
            for (int i = 0; i < ColCount; ++i)
            {
                // FormatCellValue() always returns a string (i.e. never returns null)
                _currentRow[i] = _dataFormatter.FormatCellValue(_sheet.GetRow(_currentRowIndex).GetCell(i)).Trim();
                if (_currentRow[i] == "")
                    ++_currentEmptyCellCount;
            }

            return true;
        }

        public string Get(int colIndex) => _currentRow[colIndex];

        public string Get(string colName) => _currentRow[_colIndexes[colName]];

        public string[] GetAll() => _currentRow;

        public int EmptyCellCount() => _currentEmptyCellCount;

        public bool HasColumn(string colName) => _colIndexes.ContainsKey(colName);

        public bool HasColumns(string[] colNames)
        {
            foreach (var colName in colNames)
                if (!HasColumn(colName)) return false;
            return true;
        }

        void IDisposable.Dispose() => _workbook.Dispose();
    }
}

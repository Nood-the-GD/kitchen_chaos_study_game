using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableManager : Singleton<TableManager>
{
    [SerializeField] private List<Table> _tables = new List<Table>();
    private TableModel[] _tableModels;
    public TableModel[] TableModels => _tableModels;

    private void OnEnable()
    {
        GameEvent.s.OnStartDay += ConvertToTableModels;
    }
    private void OnDisable()
    {
        GameEvent.s.OnStartDay -= ConvertToTableModels;
    }
    public void AddTable(Table table)
    {
        _tables.Add(table);
    }
    public void RemoveTable(Table table)
    {
        if (_tables.Contains(table))
            _tables.Remove(table);
    }
    public List<TableModel> GetAvailableTableModels(int numberOfPeople)
    {
        return _tableModels.Where(table => table.IsAvailable && table.NumberOfChairs >= numberOfPeople).ToList();
    }

    public TableModel GetTableModel(int tableIndex)
    {
        return _tableModels[tableIndex];
    }

    public TableModel GetTableModel(Table table)
    {
        int index = _tables.IndexOf(table);
        return _tableModels[index];
    }

    public void TableCleared(Table table)
    {
        TableModel tableModel = GetTableModel(table);
        tableModel.ClearTable();
    }

    private void ConvertToTableModels() // This will active when start day
    {
        _tableModels = new TableModel[_tables.Count];
        for (int i = 0; i < _tables.Count; i++)
        {
            Table table = _tables[i];
            table.ActiveChair();
            TableModel tableModel = new TableModel();
            tableModel.TableIndex = i;
            tableModel.NumberOfChairs = table.ChairNumber;
            tableModel.TablePosition = table.transform.position;
            tableModel.ChairsPositions = table.Chairs.Select(chair => chair.position).ToList();
            tableModel.ChairsRotations = table.Chairs.Select(chair => chair.rotation).ToList();
            tableModel.IsAvailable = true;
            _tableModels[i] = tableModel;
        }
    }
}

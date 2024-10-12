using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableManager : Singleton<TableManager>
{
    [SerializeField] private List<Table> _tables;
    private List<TableModel> _tableModels = new List<TableModel>();
    public List<TableModel> TableModels => _tableModels;

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

    private void ConvertToTableModels()
    {
        _tableModels.Clear();
        foreach (var table in _tables)
        {
            table.ActiveChair();
            TableModel tableModel = new TableModel();
            tableModel.Table = table;
            tableModel.NumberOfChairs = table.ChairNumber;
            tableModel.ChairsPosition = table.Chairs.Select(chair => chair.position).ToList();
            tableModel.ChairsRotation = table.Chairs.Select(chair => chair.rotation).ToList();
            tableModel.IsAvailable = true;
            _tableModels.Add(tableModel);
        }
    }
}

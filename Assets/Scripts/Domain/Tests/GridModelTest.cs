using NUnit.Framework;
using UnityEngine;
using CityBuilder.Domain.Models;

[TestFixture]
public class GridModelTests
{
    private GridModel _gridModel;
    private BuildingInstanceModel _building1;
    private Vector2Int _pos1 = new Vector2Int(5, 5);
    private Vector2Int _pos2 = new Vector2Int(10, 10);

    [SetUp]
    public void SetUp()
    {
        this._gridModel = new GridModel();
        this._building1 = new BuildingInstanceModel(instanceId: 1, type: BuildingType.Farm, position: this._pos1);
    }

    [Test]
    public void AddBuilding_OnEmptyCell_Succeeds()
    {
        // Act
        bool success = this._gridModel.AddBuilding(this._building1);
        
        // Assert
        Assert.IsTrue(success);
        Assert.IsTrue(this._gridModel.IsCellOccupied(this._pos1));
        Assert.AreEqual(this._building1, this._gridModel.GetBuildingAt(this._pos1));
    }

    [Test]
    public void AddBuilding_OnOccupiedCell_Fails()
    {
        // Arrange
        this._gridModel.AddBuilding(this._building1);
        BuildingInstanceModel building2 = new BuildingInstanceModel(instanceId: 2, type: BuildingType.Farm, position: this._pos1);

        // Act
        bool success = this._gridModel.AddBuilding(building2);

        // Assert
        Assert.IsFalse(success);
        Assert.AreEqual(this._building1, this._gridModel.GetBuildingAt(this._pos1)); // Убеждаемся, что старое здание на месте
    }

    [Test]
    public void MoveBuilding_ToEmptyCell_Succeeds()
    {
        // Arrange
        this._gridModel.AddBuilding(this._building1);

        // Act
        bool success = this._gridModel.MoveBuilding(this._pos1, this._pos2);

        // Assert
        Assert.IsTrue(success);
        Assert.IsFalse(this._gridModel.IsCellOccupied(this._pos1)); // Старая клетка свободна
        Assert.IsTrue(this._gridModel.IsCellOccupied(this._pos2));  // Новая занята
        Assert.AreEqual(this._building1, this._gridModel.GetBuildingAt(this._pos2));
        Assert.AreEqual(this._pos2, this._building1.Position); // Модель тоже обновила позицию
    }
    
    [Test]
    public void MoveBuilding_ToOccupiedCell_Fails()
    {
        // Arrange
        BuildingInstanceModel building2 = new BuildingInstanceModel(instanceId: 2, type: BuildingType.Farm, position: this._pos2);
        this._gridModel.AddBuilding(this._building1);
        this._gridModel.AddBuilding(building2);
        
        // Act
        bool success = this._gridModel.MoveBuilding(this._pos1, this._pos2);

        // Assert
        Assert.IsFalse(success);
        Assert.IsTrue(this._gridModel.IsCellOccupied(this._pos1)); // Здание 1 на старом месте
        Assert.IsTrue(this._gridModel.IsCellOccupied(this._pos2)); // Здание 2 на своем месте
        Assert.AreEqual(this._building1, this._gridModel.GetBuildingAt(this._pos1));
        Assert.AreEqual(building2, this._gridModel.GetBuildingAt(this._pos2));
    }
}
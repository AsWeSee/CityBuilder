using NUnit.Framework;
using System.Collections.Generic;
using CityBuilder.Domain.Models;

[TestFixture]
public class ResourcesModelTests
{
    private PlayerResourcesModel _resourcesModel;

    [SetUp]
    public void SetUp()
    {
        this._resourcesModel = new PlayerResourcesModel();
    }

    [Test]
    public void GetAmount_OnEmptyModel_ReturnsZero()
    {
        Assert.AreEqual(0, this._resourcesModel.GetAmount(ResourceType.Gold));
    }

    [Test]
    public void Add_Resource_IncreasesAmount()
    {
        // Act
        this._resourcesModel.Add(ResourceType.Gold, 100);
        
        // Assert
        Assert.AreEqual(100, this._resourcesModel.GetAmount(ResourceType.Gold));
    }

    [Test]
    public void HasEnough_WithSufficientResources_ReturnsTrue()
    {
        // Arrange
        this._resourcesModel.Add(ResourceType.Wood, 50);
        Dictionary<ResourceType, int> cost = new Dictionary<ResourceType, int> { { ResourceType.Wood, 30 } };

        // Act & Assert
        Assert.IsTrue(this._resourcesModel.HasEnough(cost));
    }

    [Test]
    public void HasEnough_WithInsufficientResources_ReturnsFalse()
    {
        // Arrange
        this._resourcesModel.Add(ResourceType.Wood, 20);
        Dictionary<ResourceType, int> cost = new Dictionary<ResourceType, int> { { ResourceType.Wood, 30 } };

        // Act & Assert
        Assert.IsFalse(this._resourcesModel.HasEnough(cost));
    }

    [Test]
    public void Spend_WithSufficientResources_DecreasesAmountAndReturnsTrue()
    {
        // Arrange
        this._resourcesModel.Add(ResourceType.Gold, 100);
        Dictionary<ResourceType, int> cost = new Dictionary<ResourceType, int> { { ResourceType.Gold, 70 } };

        // Act
        bool success = this._resourcesModel.Spend(cost);

        // Assert
        Assert.IsTrue(success);
        Assert.AreEqual(30, this._resourcesModel.GetAmount(ResourceType.Gold));
    }

    [Test]
    public void Spend_WithInsufficientResources_DoesNotChangeAmountAndReturnsFalse()
    {
        // Arrange
        this._resourcesModel.Add(ResourceType.Gold, 50);
        Dictionary<ResourceType, int> cost = new Dictionary<ResourceType, int> { { ResourceType.Gold, 70 } };

        // Act
        bool success = this._resourcesModel.Spend(cost);

        // Assert
        Assert.IsFalse(success);
        Assert.AreEqual(50, this._resourcesModel.GetAmount(ResourceType.Gold));
    }
}
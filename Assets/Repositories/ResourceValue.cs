using System;
using CityBuilder.Domain.Models;

namespace CityBuilder.Repositories
{
    [Serializable]
    public struct ResourceValue
    {
        public ResourceType Type;
        public int Amount;
    }
}
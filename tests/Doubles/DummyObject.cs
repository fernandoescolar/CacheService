using System;

namespace CacheService.Tests
{
    public class DummyObject
    {
        public string Id { get; set; }

        public DummyObject() : this(Guid.NewGuid().ToString())
        {
        }

        public DummyObject(string id)
        {
            Id = id;
        }

        public override bool Equals(object? obj)
            => Equals(obj as DummyObject);

        public bool Equals(DummyObject? obj)
        {
            if (obj is null) return false;
            if (Id is null) return false;

            return Id.Equals(obj.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
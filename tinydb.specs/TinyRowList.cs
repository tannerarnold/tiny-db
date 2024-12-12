using TinyDb.Library;
using TinyDb.Library.Interfaces;
using Xunit;

namespace TinyDb.Specs;

public class Entity(int id) : IWithId, IEquatable<Entity>
{
    public int Id { get; set; } = id;

    public bool Equals(Entity? other)
    {
        if (other == null)
            return false;
        return Id.Equals(other.Id);
    }

    public override string ToString()
    {
        return Id.ToString();
    }
}

public class TinyRowListTests
{
    readonly TinyRowList<Entity> _list;

    public TinyRowListTests()
    {
        _list = new TinyRowList<Entity>(5);
        Assert.True(_list.IsEmpty);
    }

    [Fact]
    public void Insert_Should_AddNewValueToList()
    {
        Entity val = new Entity(1);
        _list.Insert(val);

        Assert.False(_list.IsEmpty);
        Assert.Equal(1, _list.Length);
        Assert.Equal(val, _list[0]);
    }

    [Fact]
    public void Insert_MultipleValues_Should_AddNewValuesToList()
    {
        Entity[] vals = [new Entity(1), new Entity(2), new Entity(3), new Entity(4)];
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        for (int i = 0; i < vals.Length; i++)
        {
            Assert.Equal(vals[i], _list[i]);
        }
    }

    [Fact]
    public void Get_Should_ReturnValueWhenIndexInBounds()
    {
        Entity[] vals = [new Entity(1), new Entity(2), new Entity(3)];
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        Assert.Equal(vals[1], _list[1]);
    }

    [Fact]
    public void Get_Should_ThrowWhenIndexOutOfBounds()
    {
        Entity val = new Entity(1);
        _list.Insert(val);

        Assert.False(_list.IsEmpty);
        Assert.Equal(1, _list.Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => _list[-1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => _list[1]);
    }

    [Fact]
    public void Set_Should_SetValueWhenIndexInBounds()
    {
        Entity[] vals = [new Entity(1), new Entity(2), new Entity(4), new Entity(5)];
        Entity newVal = new Entity(3);
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        for (int i = 0; i < vals.Length; i++)
        {
            Assert.Equal(vals[i], _list[i]);
        }

        _list[1] = newVal;
        Assert.Equal(newVal, _list[1]);
    }

    [Fact]
    public void Set_Should_ThrowWhenIndexOutOfBounds()
    {
        Entity val = new Entity(1);
        Entity newVal = new Entity(2);
        _list.Insert(val);

        Assert.False(_list.IsEmpty);
        Assert.Equal(1, _list.Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => _list[-1] = newVal);
        Assert.Throws<ArgumentOutOfRangeException>(() => _list[1] = newVal);
    }

    [Fact]
    public void InsertAt_Should_InsertValueIntoList()
    {
        Entity[] vals = [new Entity(1), new Entity(2), new Entity(4), new Entity(5)];
        Entity newVal = new Entity(3);
        Entity[] expectedVals = [new Entity(1), new Entity(2), new Entity(3), new Entity(4), new Entity(5)];
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        for (int i = 0; i < vals.Length; i++)
        {
            Assert.Equal(vals[i], _list[i]);
        }

        _list.InsertAt(2, newVal);

        Assert.False(_list.IsEmpty);
        Assert.Equal(expectedVals.Length, _list.Length);
        for (int i = 0; i < expectedVals.Length; i++)
        {
            Assert.Equal(expectedVals[i], _list[i]);
        }
    }

    [Fact]
    public void InsertAt_Should_InsertValueWhenOnlyOneValueInList()
    {
        Entity val = new Entity(2);
        Entity newVal = new Entity(1);
        Entity[] expectedVals = [new Entity(1), new Entity(2)];
        _list.Insert(val);

        Assert.False(_list.IsEmpty);
        Assert.Equal(1, _list.Length);
        Assert.Equal(val, _list[0]);

        _list.InsertAt(0, newVal);

        Assert.False(_list.IsEmpty);
        Assert.Equal(expectedVals.Length, _list.Length);
        for (int i = 0; i < expectedVals.Length; i++)
        {
            Assert.Equal(expectedVals[i], _list[i]);
        }
    }

    [Fact]
    public void InsertAt_Should_ThrowWhenIndexOutOfBounds()
    {
        Entity val = new Entity(1);
        Entity newVal = new Entity(2);
        _list.Insert(val);

        Assert.False(_list.IsEmpty);
        Assert.Equal(1, _list.Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => _list.InsertAt(-1, newVal));
        Assert.Throws<ArgumentOutOfRangeException>(() => _list.InsertAt(1, newVal));
    }

    [Fact]
    public void RemoveAt_Should_RemoveValueFromList()
    {
        Entity[] vals = [new Entity(1), new Entity(2), new Entity(3), new Entity(4), new Entity(5)];
        Entity[] expectedVals = [new Entity(1), new Entity(2), new Entity(4), new Entity(5)];
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        for (int i = 0; i < vals.Length; i++)
        {
            Assert.Equal(vals[i], _list[i]);
        }

        _list.RemoveAt(2);

        Assert.False(_list.IsEmpty);
        Assert.Equal(expectedVals.Length, _list.Length);
        for (int i = 0; i < expectedVals.Length; i++)
        {
            Assert.Equal(expectedVals[i], _list[i]);
        }
    }

    [Fact]
    public void RemoveAt_Should_RemoveValueWhenOnlyOneValueInList()
    {
        Entity val = new Entity(1);
        _list.Insert(val);

        Assert.False(_list.IsEmpty);
        Assert.Equal(1, _list.Length);
        Assert.Equal(val, _list[0]);

        _list.RemoveAt(0);

        Assert.True(_list.IsEmpty);
        Assert.Equal(0, _list.Length);
    }

    [Fact]
    public void RemoveAt_Should_ThrowWhenIndexOutOfBounds()
    {
        Entity val = new Entity(1);
        _list.Insert(val);

        Assert.False(_list.IsEmpty);
        Assert.Equal(1, _list.Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => _list.RemoveAt(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _list.RemoveAt(1));
    }

    [Fact]
    public void Sort_Should_SortEmptyList()
    {
        _list.Sort();

        Assert.True(_list.IsEmpty);
        Assert.Equal(0, _list.Length);
    }

    [Fact]
    public void Sort_Should_SortListWithOneElement()
    {
        Entity val = new Entity(1);
        _list.Insert(val);

        Assert.False(_list.IsEmpty);
        Assert.Equal(1, _list.Length);
        Assert.Equal(val, _list[0]);

        _list.Sort();

        Assert.False(_list.IsEmpty);
        Assert.Equal(1, _list.Length);
        Assert.Equal(val, _list[0]);
    }

    [Fact]
    public void Sort_Should_SortListWithMultipleElementsInAscendingOrder()
    {
        Entity[] vals = [new Entity(1), new Entity(3), new Entity(5), new Entity(2), new Entity(4)];
        Entity[] expectedVals = [new Entity(1), new Entity(2), new Entity(3), new Entity(4), new Entity(5)];
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        for (int i = 0; i < vals.Length; i++)
        {
            Assert.Equal(vals[i], _list[i]);
        }

        _list.Sort();

        Assert.False(_list.IsEmpty);
        Assert.Equal(expectedVals.Length, _list.Length);
        for (int i = 0; i < expectedVals.Length; i++)
        {
            Assert.Equal(expectedVals[i], _list[i]);
        }
    }

    [Fact]
    public void Sort_Should_SortListWithMultipleAndDuplicateElementsInAscendingOrder()
    {
        Entity[] vals = [new Entity(1), new Entity(3), new Entity(5), new Entity(3), new Entity(5)];
        Entity[] expectedVals = [new Entity(1), new Entity(3), new Entity(3), new Entity(5), new Entity(5)];
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        for (int i = 0; i < vals.Length; i++)
        {
            Assert.Equal(vals[i], _list[i]);
        }

        _list.Sort();

        Assert.False(_list.IsEmpty);
        Assert.Equal(expectedVals.Length, _list.Length);
        for (int i = 0; i < expectedVals.Length; i++)
        {
            Assert.Equal(expectedVals[i], _list[i]);
        }
    }

    [Fact]
    public void Sort_Should_SortListWithMultipleElementsInDescendingOrder()
    {
        Entity[] vals = [new Entity(1), new Entity(3), new Entity(5), new Entity(2), new Entity(4)];
        Entity[] expectedVals = [new Entity(5), new Entity(4), new Entity(3), new Entity(2), new Entity(1)];
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        for (int i = 0; i < vals.Length; i++)
        {
            Assert.Equal(vals[i], _list[i]);
        }

        _list.Sort(SortOrder.Descending);

        Assert.False(_list.IsEmpty);
        Assert.Equal(expectedVals.Length, _list.Length);
        for (int i = 0; i < expectedVals.Length; i++)
        {
            Assert.Equal(expectedVals[i], _list[i]);
        }
    }

    [Fact]
    public void Sort_Should_SortListWithMultipleAndDuplicateElementsInDescendingOrder()
    {
        Entity[] vals = [new Entity(1), new Entity(3), new Entity(5), new Entity(3), new Entity(5)];
        Entity[] expectedVals = [new Entity(5), new Entity(5), new Entity(3), new Entity(3), new Entity(1)];
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        for (int i = 0; i < vals.Length; i++)
        {
            Assert.Equal(vals[i], _list[i]);
        }

        _list.Sort(SortOrder.Descending);

        Assert.False(_list.IsEmpty);
        Assert.Equal(expectedVals.Length, _list.Length);
        for (int i = 0; i < expectedVals.Length; i++)
        {
            Assert.Equal(expectedVals[i], _list[i]);
        }
    }

    [Fact]
    public void Find_Should_FindElementInSortedList()
    {
        Entity[] vals = [new Entity(1), new Entity(3), new Entity(4), new Entity(6), new Entity(7)];
        Entity expectedVal = new Entity(6);
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        for (int i = 0; i < vals.Length; i++)
        {
            Assert.Equal(vals[i], _list[i]);
        }

        Entity? actualVal = _list.Find(expectedVal.Id);

        Assert.NotNull(actualVal);
        Assert.Equal(expectedVal, actualVal);
    }

    [Fact]
    public void Find_Should_NotFindElementInSortedListWhenNotExists()
    {
        Entity[] vals = [new Entity(1), new Entity(3), new Entity(4), new Entity(6), new Entity(7)];
        Entity expectedVal = new Entity(5);
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        for (int i = 0; i < vals.Length; i++)
        {
            Assert.Equal(vals[i], _list[i]);
        }

        Entity? actualVal = _list.Find(expectedVal.Id);

        Assert.Null(actualVal);
    }

    [Fact]
    public void Find_Should_NotFindElementInUnsortedList()
    {
        Entity[] vals = [new Entity(1), new Entity(4), new Entity(6), new Entity(3), new Entity(7)];
        Entity expectedVal = new Entity(5);
        for (int i = 0; i < vals.Length; i++)
        {
            _list.Insert(vals[i]);
        }

        Assert.False(_list.IsEmpty);
        Assert.Equal(vals.Length, _list.Length);
        for (int i = 0; i < vals.Length; i++)
        {
            Assert.Equal(vals[i], _list[i]);
        }

        Entity? actualVal = _list.Find(expectedVal.Id);

        Assert.Null(actualVal);
    }
}
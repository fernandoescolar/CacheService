using CacheService.Core;

namespace CacheService.Tests;

public class PooledBufferWriterTests
{

	[Fact]
	public void Advance_WithNegativeCount_Throws()
	{
		using var writer = new PooledBufferWriter();

		Assert.Throws<ArgumentOutOfRangeException>(() => writer.Advance(-1));
	}

    [Fact]
	public void Advance_To_Limit()
	{
		using var writer = new PooledBufferWriter();

		writer.GetSpan(32);
		writer.Advance(31);
		writer.Advance(1); // This should not throw
	}

	[Fact]
	public void Advance_PastBuffer_Throws()
	{
		using var writer = new PooledBufferWriter();

		writer.GetSpan(32);
		writer.Advance(31);

		Assert.Throws<InvalidOperationException>(() => writer.Advance(2));
	}

	[Fact]
	public void GetSpan_WritesBytes_ReadsViaReadOnlyMemory()
	{
		using var writer = new PooledBufferWriter();

		var span = writer.GetSpan(3);
		span[0] = 1;
		span[1] = 2;
		span[2] = 3;
		writer.Advance(3);

		Assert.Equal(new byte[] { 1, 2, 3 }, writer.AsReadOnlyMemory().ToArray());
	}

	[Fact]
	public void GetMemory_WithSizeHint_ProvidesCapacity()
	{
		using var writer = new PooledBufferWriter();

		var memory = writer.GetMemory(5);
		memory.Span[0] = 10;
		memory.Span[1] = 20;
		memory.Span[2] = 30;
		memory.Span[3] = 40;
		memory.Span[4] = 50;
		writer.Advance(5);

		Assert.True(memory.Length >= 5);
		Assert.Equal(new byte[] { 10, 20, 30, 40, 50 }, writer.ToArray());
	}

	[Fact]
	public void ToArray_WhenEmpty_ReturnsEmptyArray()
	{
		using var writer = new PooledBufferWriter();

		var result = writer.ToArray();

		Assert.Empty(result);
	}

	[Fact]
	public void GetSpan_GrowsBuffer_PreservesExistingData()
	{
		using var writer = new PooledBufferWriter();

		var initial = writer.GetSpan(2);
		initial[0] = 10;
		initial[1] = 20;
		writer.Advance(2);

		var expanded = writer.GetSpan(4);
		expanded[0] = 30;
		expanded[1] = 40;
		expanded[2] = 50;
		expanded[3] = 60;
		writer.Advance(4);

		Assert.Equal(new byte[] { 10, 20, 30, 40, 50, 60 }, writer.ToArray());
	}

}

using System;
using CapitalGains.Domain.Entities;
using FluentAssertions;
using Xunit;

public class EntitiesTests
{
	[Fact]
	public void Operation_should_throw_when_quantity_is_non_positive()
	{
		Action act = () => new Operation(OperationType.Buy, unitCost: 10m, quantity: 0);
		act.Should().Throw<ArgumentOutOfRangeException>()
			.WithParameterName("quantity");
	}

	[Fact]
	public void Operation_should_throw_when_unit_cost_is_negative()
	{
		Action act = () => new Operation(OperationType.Buy, unitCost: -0.01m, quantity: 1);
		act.Should().Throw<ArgumentOutOfRangeException>()
			.WithParameterName("unitCost");
	}

	[Fact]
	public void Operation_should_create_with_valid_inputs()
	{
		var operation = new Operation(OperationType.Sell, unitCost: 15.50m, quantity: 10);
		operation.Type.Should().Be(OperationType.Sell);
		operation.UnitCost.Should().Be(15.50m);
		operation.Quantity.Should().Be(10);
	}

	[Fact]
	public void PortfolioState_Empty_should_have_zero_values()
	{
		var empty = PortfolioState.Empty;
		empty.TotalQuantity.Should().Be(0);
		empty.WeightedAveragePrice.Should().Be(0m);
		empty.AccumulatedLoss.Should().Be(0m);
	}

	[Fact]
	public void PortfolioState_should_throw_when_any_value_is_negative()
	{
		Action qty = () => new PortfolioState(-1, 0m, 0m);
		Action avg = () => new PortfolioState(0, -0.01m, 0m);
		Action loss = () => new PortfolioState(0, 0m, -0.01m);

		qty.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("totalQuantity");
		avg.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("weightedAveragePrice");
		loss.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("accumulatedLoss");
	}

	[Fact]
	public void OperationType_enum_should_have_expected_values()
	{
		((int)OperationType.Buy).Should().Be(0);
		((int)OperationType.Sell).Should().Be(1);
	}
}



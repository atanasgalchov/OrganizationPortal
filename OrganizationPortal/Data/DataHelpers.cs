using System;
using System.Collections.Generic;
using System.Linq;

namespace OrganizationPortal.Data
{
    public class DbParameters<T>
    {
        public T Filters { get; set; }
        public Dictionary<string, FilterOperators> FiltersOperators { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";
        public int PageIndex { get; set; } = 0;
        public int PageSize { get; set; } = 20;
        public int TotalRows { get; set; }
    }

	public enum FilterOperators
	{
		Eq,
		Contains,
		Gt,
		GtEq,
		Lt,
		LtEq,
		IsNull
	}

	public static class FilterDataHelper 
	{
		public static IQueryable<T> FilterColection<T>(this IQueryable<T> list, Filters<T> filters)
		{
			if (filters.FiltersModel == null)
				return list;

			IQueryable<T> result = list;
			foreach (var prop in filters.FiltersModel.GetType().GetProperties())
			{
				string propName = prop.Name;
				Type propType = prop.GetType();
				object propValue = prop.GetValue(filters.FiltersModel);

				if (filters.Comparator == Comparatos.And)
				{
					FilterOperators op = filters.Operators != null && filters.Operators.ContainsKey(propName) ? filters.Operators[propName] : FilterOperators.Eq;
					result = list.Where(x => IsMathFilter(x, propType, propValue, op));
				}
				// TODO Implement Or
			}

			return result;
		}
		public static bool IsMathFilter<T>(T item, Type filterProp, object filterPropValue, FilterOperators filterOperator)
		{
			if (filterPropValue != null)
				if (item.GetType().GetProperty(filterProp.Name) != null && item.GetType().GetProperty(filterProp.Name).GetType() == filterProp)
				{
					Type itemPropType = item.GetType().GetProperty(filterProp.Name).GetType();
					// TODO Check for object type and call method recursivly
					object itemPropertyValue = item.GetType().GetProperty(filterProp.Name).GetValue(item);
					if (filterOperator == FilterOperators.Eq && CanApplyOperator(itemPropType, filterOperator))
					{
						return itemPropertyValue == filterPropValue;
					}
					else if (filterOperator == FilterOperators.Gt && CanApplyOperator(itemPropType, filterOperator))
					{
						// DateTime
						if (itemPropType == typeof(DateTime) && filterProp == typeof(DateTime))
							return (DateTime)itemPropertyValue > (DateTime)filterPropValue;
						else if (itemPropType == typeof(DateTime?) && filterProp == typeof(DateTime?))
							return (DateTime)itemPropertyValue > (DateTime)filterPropValue;
						// Short
						else if (itemPropType == typeof(short) && filterProp == typeof(short))
							return (short)itemPropertyValue > (short)filterPropValue;
						else if (itemPropType == typeof(short?) && filterProp == typeof(short?))
							return (short)itemPropertyValue > (short)filterPropValue;
						// Int
						else if (itemPropType == typeof(int) && filterProp == typeof(int))
							return (int)itemPropertyValue > (int)filterPropValue;
						else if (itemPropType == typeof(int?) && filterProp == typeof(int?))
							return (int)itemPropertyValue > (int)filterPropValue;
						// Long
						else if (itemPropType == typeof(long) && filterProp == typeof(long))
							return (long)itemPropertyValue > (long)filterPropValue;
						else if (itemPropType == typeof(long?) && filterProp == typeof(long?))
							return (long)itemPropertyValue > (long)filterPropValue;

						// Float
						else if (itemPropType == typeof(float) && filterProp == typeof(float))
							return (float)itemPropertyValue > (float)filterPropValue;
						else if (itemPropType == typeof(float?) && filterProp == typeof(float?))
							return (float)itemPropertyValue > (float)filterPropValue;

						// Double
						else if (itemPropType == typeof(double) && filterProp == typeof(double))
							return (double)itemPropertyValue > (double)filterPropValue;
						else if (itemPropType == typeof(double?) && filterProp == typeof(double?))
							return (double)itemPropertyValue > (double)filterPropValue;

						// Decimal
						else if (itemPropType == typeof(decimal) && filterProp == typeof(decimal))
							return (decimal)itemPropertyValue > (decimal)filterPropValue;
						else if (itemPropType == typeof(decimal?) && filterProp == typeof(decimal?))
							return (decimal)itemPropertyValue > (decimal)filterPropValue;
					}
					else if (filterOperator == FilterOperators.GtEq && CanApplyOperator(itemPropType, filterOperator))
					{
						// DateTime
						if (itemPropType == typeof(DateTime) && filterProp == typeof(DateTime))
							return (DateTime)itemPropertyValue >= (DateTime)filterPropValue;
						else if (itemPropType == typeof(DateTime?) && filterProp == typeof(DateTime?))
							return (DateTime)itemPropertyValue >= (DateTime)filterPropValue;
						// Short
						else if (itemPropType == typeof(short) && filterProp == typeof(short))
							return (short)itemPropertyValue >= (short)filterPropValue;
						else if (itemPropType == typeof(short?) && filterProp == typeof(short?))
							return (short)itemPropertyValue >= (short)filterPropValue;
						// Int
						else if (itemPropType == typeof(int) && filterProp == typeof(int))
							return (int)itemPropertyValue >= (int)filterPropValue;
						else if (itemPropType == typeof(int?) && filterProp == typeof(int?))
							return (int)itemPropertyValue >= (int)filterPropValue;
						// Long
						else if (itemPropType == typeof(long) && filterProp == typeof(long))
							return (long)itemPropertyValue >= (long)filterPropValue;
						else if (itemPropType == typeof(long?) && filterProp == typeof(long?))
							return (long)itemPropertyValue >= (long)filterPropValue;

						// Float
						else if (itemPropType == typeof(float) && filterProp == typeof(float))
							return (float)itemPropertyValue >= (float)filterPropValue;
						else if (itemPropType == typeof(float?) && filterProp == typeof(float?))
							return (float)itemPropertyValue >= (float)filterPropValue;

						// Double
						else if (itemPropType == typeof(double) && filterProp == typeof(double))
							return (double)itemPropertyValue >= (double)filterPropValue;
						else if (itemPropType == typeof(double?) && filterProp == typeof(double?))
							return (double)itemPropertyValue >= (double)filterPropValue;

						// Decimal
						else if (itemPropType == typeof(decimal) && filterProp == typeof(decimal))
							return (decimal)itemPropertyValue >= (decimal)filterPropValue;
						else if (itemPropType == typeof(decimal?) && filterProp == typeof(decimal?))
							return (decimal)itemPropertyValue >= (decimal)filterPropValue;
					}

					else if (filterOperator == FilterOperators.Lt && CanApplyOperator(itemPropType, filterOperator))
					{
						// DateTime
						if (itemPropType == typeof(DateTime) && filterProp == typeof(DateTime))
							return (DateTime)itemPropertyValue < (DateTime)filterPropValue;
						else if (itemPropType == typeof(DateTime?) && filterProp == typeof(DateTime?))
							return (DateTime)itemPropertyValue < (DateTime)filterPropValue;
						// Short
						else if (itemPropType == typeof(short) && filterProp == typeof(short))
							return (short)itemPropertyValue < (short)filterPropValue;
						else if (itemPropType == typeof(short?) && filterProp == typeof(short?))
							return (short)itemPropertyValue < (short)filterPropValue;
						// Int
						else if (itemPropType == typeof(int) && filterProp == typeof(int))
							return (int)itemPropertyValue < (int)filterPropValue;
						else if (itemPropType == typeof(int?) && filterProp == typeof(int?))
							return (int)itemPropertyValue < (int)filterPropValue;
						// Long
						else if (itemPropType == typeof(long) && filterProp == typeof(long))
							return (long)itemPropertyValue < (long)filterPropValue;
						else if (itemPropType == typeof(long?) && filterProp == typeof(long?))
							return (long)itemPropertyValue < (long)filterPropValue;

						// Float
						else if (itemPropType == typeof(float) && filterProp == typeof(float))
							return (float)itemPropertyValue < (float)filterPropValue;
						else if (itemPropType == typeof(float?) && filterProp == typeof(float?))
							return (float)itemPropertyValue < (float)filterPropValue;

						// Double
						else if (itemPropType == typeof(double) && filterProp == typeof(double))
							return (double)itemPropertyValue < (double)filterPropValue;
						else if (itemPropType == typeof(double?) && filterProp == typeof(double?))
							return (double)itemPropertyValue < (double)filterPropValue;

						// Decimal
						else if (itemPropType == typeof(decimal) && filterProp == typeof(decimal))
							return (decimal)itemPropertyValue < (decimal)filterPropValue;
						else if (itemPropType == typeof(decimal?) && filterProp == typeof(decimal?))
							return (decimal)itemPropertyValue < (decimal)filterPropValue;
					}
					else if (filterOperator == FilterOperators.LtEq && CanApplyOperator(itemPropType, filterOperator))
					{
						// DateTime
						if (itemPropType == typeof(DateTime) && filterProp == typeof(DateTime))
							return (DateTime)itemPropertyValue <= (DateTime)filterPropValue;
						else if (itemPropType == typeof(DateTime?) && filterProp == typeof(DateTime?))
							return (DateTime)itemPropertyValue <= (DateTime)filterPropValue;
						// Short
						else if (itemPropType == typeof(short) && filterProp == typeof(short))
							return (short)itemPropertyValue <= (short)filterPropValue;
						else if (itemPropType == typeof(short?) && filterProp == typeof(short?))
							return (short)itemPropertyValue <= (short)filterPropValue;
						// Int
						else if (itemPropType == typeof(int) && filterProp == typeof(int))
							return (int)itemPropertyValue <= (int)filterPropValue;
						else if (itemPropType == typeof(int?) && filterProp == typeof(int?))
							return (int)itemPropertyValue <= (int)filterPropValue;
						// Long
						else if (itemPropType == typeof(long) && filterProp == typeof(long))
							return (long)itemPropertyValue <= (long)filterPropValue;
						else if (itemPropType == typeof(long?) && filterProp == typeof(long?))
							return (long)itemPropertyValue <= (long)filterPropValue;

						// Float
						else if (itemPropType == typeof(float) && filterProp == typeof(float))
							return (float)itemPropertyValue <= (float)filterPropValue;
						else if (itemPropType == typeof(float?) && filterProp == typeof(float?))
							return (float)itemPropertyValue <= (float)filterPropValue;

						// Double
						else if (itemPropType == typeof(double) && filterProp == typeof(double))
							return (double)itemPropertyValue <= (double)filterPropValue;
						else if (itemPropType == typeof(double?) && filterProp == typeof(double?))
							return (double)itemPropertyValue <= (double)filterPropValue;

						// Decimal
						else if (itemPropType == typeof(decimal) && filterProp == typeof(decimal))
							return (decimal)itemPropertyValue <= (decimal)filterPropValue;
						else if (itemPropType == typeof(decimal?) && filterProp == typeof(decimal?))
							return (decimal)itemPropertyValue <= (decimal)filterPropValue;
					}
					if (filterOperator == FilterOperators.Contains && CanApplyOperator(itemPropType, filterOperator))
					{
						return itemPropertyValue.ToString().Contains(filterPropValue.ToString());
					}
					else if (filterOperator == FilterOperators.IsNull)
					{
						return itemPropertyValue == null;
					}
				}

			return true;
		}
		public static bool CanApplyOperator(Type type, FilterOperators op)
		{
			if (op == FilterOperators.Eq)
				return true;
			else if (op == FilterOperators.Gt || op == FilterOperators.GtEq || op == FilterOperators.GtEq || op == FilterOperators.Lt || op == FilterOperators.LtEq)
				return type == typeof(DateTime) || type == typeof(DateTime?) || type.IsValueType;
			else if (op == FilterOperators.Contains)
				return type == typeof(string);
			else if (op == FilterOperators.IsNull)
				return Nullable.GetUnderlyingType(type) != null;

			return false;
		}
	}

	public class Filters<T>
	{
		public T FiltersModel { get; set; }
		public Dictionary<string, FilterOperators> Operators { get; set; }
		public Comparatos Comparator { get; set; }
	}
	public enum Comparatos
	{
		Or,
		And
	}
}

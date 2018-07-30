using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using LazyCache;
using MagicMaids.DataAccess;
using MagicMaids.ViewModels;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MagicMaidsTesting.BookingFactory_Tests")]
namespace MagicMaids
{
	public class BookingFactory
	{
		#region Fields
		private IList<CleanerJobMatchVM> resultsList;
		private SearchVM criteria;

		#endregion 

		#region Constructor
		public BookingFactory(List<CleanerJobMatchVM> searchResults, SearchVM searchCriteria)
		{
			resultsList = searchResults;
			criteria = searchCriteria;

			if (criteria != null)
			{
				SearchZoneList = GetZoneListBySuburb(criteria.Suburb);
			}
		}
		#endregion

		#region Properties, Private
		private List<String> SearchZoneList
		{
			get;
			set;
		}
		#endregion 

		#region Methods, Public
		public IEnumerable<CleanerJobMatchVM> GetProcessedResults()
		{
			if (resultsList == null)
			{
				resultsList = new List<CleanerJobMatchVM>();
			}

			foreach(CleanerJobMatchVM item in resultsList)
			{
				yield return ParseLocation(item);
			}
			
		}

		public static List<string> GetZoneListBySuburb(string suburbName)
		{
			if (String.IsNullOrWhiteSpace(suburbName))
			{
				return new List<string>();
			}

			IAppCache cache = new CachingService();
			List<string> zoneList = cache.GetOrAdd($"SuburbZones_{suburbName}", () => GetZoneList(suburbName), new TimeSpan(0, 20, 0));

			if (zoneList.Count == 0)
			{
				cache.Remove($"SuburbZones_{suburbName}");
			}

			return zoneList;
		}

		public String FormatStyleForHome(CleanerJobMatchVM cleaner)
		{
			if (cleaner == null)
			{
				return "";
			}

			var isFirstJob = cleaner.IsFirstJob;

			if (isFirstJob)
			{
				return NamedColours.FirstJobColor;
			}

			var cleanerBaseZoneList = GetZoneListBySuburb(cleaner.PhysicalAddress.Suburb);
			if (applyColor(cleanerBaseZoneList, SearchZoneList))
			{
				return NamedColours.PrimaryJobColor;
			}

			if (applyColor(cleanerBaseZoneList, cleaner.SecondaryZoneList))
			{
				return NamedColours.SecondaryJobColor;
			}

			return "";

		}

		public String FormatStyleForJobZone(bool forPrevJob, CleanerJobMatchVM cleaner)
		{
			String searchJobLocation = (criteria == null ? "" : criteria.Suburb);
			if (String.IsNullOrWhiteSpace(searchJobLocation))
			{
				return "";
			}

			if (cleaner == null)
			{
				return "";
			}

			var prevZoneList = GetZoneListBySuburb(cleaner.PreviousJobLocation);
			var nextZoneList = GetZoneListBySuburb(cleaner.NextJobLocation);

			// if search suburb is in cleaner primary zone and prev/next job is also in primary zone
			if (applyColor(cleaner.PrimaryZoneList, SearchZoneList))
			{
				if (forPrevJob && applyColor(cleaner.PrimaryZoneList, prevZoneList))
				{
					return NamedColours.PrimaryJobColor;
				}
				else if (!forPrevJob && applyColor(cleaner.PrimaryZoneList, nextZoneList))
				{
					return NamedColours.PrimaryJobColor;
				}
			}

			// if search suburb is in cleaner secondary zone and prev/next job is also in secondary zone
			if (applyColor(cleaner.SecondaryZoneList, SearchZoneList))
			{
				if (forPrevJob && applyColor(cleaner.SecondaryZoneList, prevZoneList))
				{
					return NamedColours.SecondaryJobColor;
				}
				else if (!forPrevJob && applyColor(cleaner.SecondaryZoneList, nextZoneList))
				{
					return NamedColours.SecondaryJobColor;
				}
			}

			// if search suburb is in cleaner approved zone and prev/next job is also in approved zone
			if (applyColor(cleaner.ApprovedZoneList, SearchZoneList))
			{
				if (forPrevJob && applyColor(cleaner.ApprovedZoneList, prevZoneList))
				{
					return NamedColours.ApprovedJobColor;
				}
				else if (!forPrevJob && applyColor(cleaner.ApprovedZoneList, nextZoneList))
				{
					return NamedColours.ApprovedJobColor;
				}
			}

			return "";
		}

		#endregion 

		#region Methods, Private

		private CleanerJobMatchVM ParseLocation(CleanerJobMatchVM item)
		{
			StringBuilder _output = new StringBuilder();
			_output.Append("<table class='table bb'>");
			_output.Append("<tr>");
			_output.Append("<td>Previous Job:</td>");
			_output.Append($"<td class='{FormatStyleForJobZone(true, item)}'>").Append(item.IsFirstJob ? "-" : item.PreviousJobLocation).Append("</td>");
			_output.Append("</tr>");

			_output.Append("<tr>");
			_output.Append("<td>Next Job:</td>");
			_output.Append($"<td class='{FormatStyleForJobZone(false, item)}'>").Append(String.IsNullOrWhiteSpace(item.NextJobLocation) ? "-" : item.NextJobLocation).Append("</td>");
			_output.Append("</tr>");

			_output.Append("<tr>");
			_output.Append("<td>Base:</td>");
			_output.Append($"<td class='{FormatStyleForHome(item)}'>").Append(String.IsNullOrWhiteSpace(item.PhysicalAddress.Suburb) ? "-" : item.PhysicalAddress.Suburb).Append("</td>");
			_output.Append("</tr>");
			_output.Append("</table>");

			item.DisplayLocation =  _output.ToString();
			return item;
		}

		private static List<string> GetZoneList(string suburbName)
		{
			if (String.IsNullOrWhiteSpace(suburbName))
			{
				return null;
			}

			List<String> _zoneList = new List<String>();
			using (DBManager db = new DBManager())
			{
				_zoneList = db.getConnection().Query<String>($"select Zone+','+LinkedZones from SuburbZones where SuburbName like '%{suburbName}%' or PostCode = '{suburbName}'").ToList();

				// load system default list
				if (_zoneList.Count == 0)
				{
					_zoneList = db.getConnection().Query<String>($"select Zone+','+LinkedZones from SuburbZones where FranchiseId is not null").ToList();
				}
			}


			var _zoneCSV = String.Join(",", _zoneList);
			_zoneList = _zoneCSV.Split(new char[] { ',', ';' })
						  .Distinct()
						  .ToList();

			_zoneList.Sort();

			return _zoneList;
		}

		private Boolean applyColor(List<String> cleanerZoneList, List<String> compareList)
		{
			if (cleanerZoneList.Intersect(compareList).Any())
			{
				return true;
			}

			return false;
		}

		#endregion
	}
}

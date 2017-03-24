using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AlconTrendAnalysis
{
	class CSVReader
	{
		public static List<TagData> LoadFromCSV(string path)
		{
			List<TagData> data = new List<TagData>();

			using (StreamReader reader = new StreamReader(path))
			{
				// Read first line to find the min and max second times
				string line = reader.ReadLine();
				var times = line.Substring(20).Split(',');

				// convert times to long
				long[] sectimes = new long[times.Count()];
				for (int loopX = 0; loopX < times.Count(); loopX++)
					sectimes[loopX] = long.Parse(times[loopX]);

				// Now, every line is a new tag, followed by values
				while (!reader.EndOfStream)
				{
					string[] row = reader.ReadLine().Split(',');
					TagData tag = new TagData();
					// 0 = tag description
					tag.Description = row[0];
					// 1 = tag name
					tag.Name = row[1];
					// 2+ = value @ sectime	@ index - 2
					if (row.Count() != (sectimes.Count() + 2)) throw new Exception("In LoadFromCSV, tag found with not the same number of times!");
					tag.Values = new List<TagValue>();
					for (int loopX = 2; loopX < row.Count(); loopX++)
						tag.Values.Add(
							new TagValue(
								sectimes[loopX - 2],
								string.IsNullOrWhiteSpace(row[loopX]) ? 0 : double.Parse(row[loopX])));

					data.Add(tag);
				}
			}

			return data;
		}
	}
}

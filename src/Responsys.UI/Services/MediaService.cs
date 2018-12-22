using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Enivate.ResponseHub.Responsys.UI.Services
{
    public class MediaService
    {

		public static string MapBoxStaticImage(double lat, double lng, int zoom, string size)
		{

			// If the latitude is not between -90 and 90, lng is not between -180 and 180 and the zoom is not between 1 and 20, throw bad request
			if (lat > 90 || lat < -90)
			{
				throw new ApplicationException("The 'lat' parameter must be between -90 and 90 (inclusive).");
			}
			if (lng > 180 || lng < -180)
			{
				throw new ApplicationException("The 'lng' parameter must be between -180 and 180 (inclusive).");
			}
			if (zoom > 20 || zoom < 1)
			{
				throw new ApplicationException("The 'zoom' parameter must be between 1 and 20 (inclusive).");
			}

			// Get the static maps image path
			string staticImagePath = ConfigurationManager.AppSettings["StaticMapImagesPath"];
			if (String.IsNullOrEmpty(staticImagePath))
			{
				throw new ApplicationException("The 'staticMapImagesPath' configuration attribute is null or empty.");
			}
			// Get path if required
			if (staticImagePath[0] == '.')
			{
				staticImagePath = String.Format("{0}{1}{2}", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.DirectorySeparatorChar, staticImagePath);
			}

			int latFloor = (int)lat;

			// Create the image filename
			string imageFilename = String.Format("{0}\\{1}\\{2}_{3}_{4}_{5}.png", staticImagePath, latFloor, lat, lng, zoom, size);

			// If the file does not exist, we need to get it from the google maps static api and save to disc
			if (!File.Exists(imageFilename))
			{

				string mapBoxImageUrl = String.Format("https://api.mapbox.com/v4/juzzbott.mn25f8nc/pin-m+f6821f({0},{1})/{0},{1},{2}/{3}.png64?access_token=pk.eyJ1IjoianV6emJvdHQiLCJhIjoiMDlmN2JlMzMxMWI2YmNmNGY2NjFkZGFiYTFiZWVmNTQifQ.iKlZsVrsih0VuiUCzLZ1Lg",
					lng,
					lat,
					zoom,
					size);

				// Create the client and request objects
				HttpWebRequest client = HttpWebRequest.CreateHttp(mapBoxImageUrl);
				HttpWebResponse response = (HttpWebResponse)client.GetResponse();

				using (Stream responseStream = response.GetResponseStream())
				{

					// Ensure the directory exists
					if (!Directory.Exists(Path.GetDirectoryName(imageFilename)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(imageFilename));
					}

					// Write the file to disk
					using (FileStream fs = new FileStream(imageFilename, FileMode.Create))
					{
						byte[] buffer = new byte[8 * 1024];
						int len;
						while ((len = responseStream.Read(buffer, 0, buffer.Length)) > 0)
						{
							fs.Write(buffer, 0, len);
						}
					}
				}

			}

			// return the image file.
			return imageFilename;
		}

	}
}

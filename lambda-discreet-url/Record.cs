using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Lambda.Models
{
  public class APIModel
  {
    private string _url;
    public string Url
    {
      get
      {
        return _url;
      }

      set
      {
        Regex r = new Regex(@"^(?:(?:https?|ftp)://)(?:\S+(?::\S*)?@)?(?:(?!10(?:\.\d{1,3}){3})(?!127(?:\.\d{1,3}){3})(?!169\.254(?:\.\d{1,3}){2})(?!192\.168(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\x{00a1}-\x{ffff}0-9]+-?)*[a-z\x{00a1}-\x{ffff}0-9]+)(?:\.(?:[a-z\x{00a1}-\x{ffff}0-9]+-?)*[a-z\x{00a1}-\x{ffff}0-9]+)*(?:\.(?:[a-z\x{00a1}-\x{ffff}]{2,})))(?::\d{2,5})?(?:/[^\s]*)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        if (!r.IsMatch(value))
        {
          throw new UriFormatException("Invalid URL.");
        }

        _url = value;
      }
    }
  }

  public class Record
  {
    public string Id { get; set; }

    public string Url { get; set; }
  }
}

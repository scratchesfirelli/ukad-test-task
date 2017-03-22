using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;
using ukad_test_task.Models;
using static ukad_test_task.Models.Context;

namespace ukad_test_task.Controllers
{
    public class SitemapController : Controller
    {
        // GET: Sitemap
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Determine(string url)
        {
            List<UrlResponseTime> responses = new List<UrlResponseTime>();

            using (var db = new UrlResponseContext())
            {
                //checking if already searched
                var responsesFromDB = db.UrlResponseTime
                    .Where(record => record.Url.Contains(url))
                    .OrderByDescending(record => record.MaxResponseTime);
                if (responsesFromDB.Count() > 0)
                {
                    responses = responsesFromDB.ToList();
                }
                else
                {
                    url = url.Contains("http") ? url : "http://" + url;
                    //Adding root url and its children urls
                    HashSet<string> links = new HashSet<string>();
                    links.Add(url);
                    HashSet<string> linksAlreadySearched = new HashSet<string>();

                    while (links.Count != 0)
                    {
                        //getting first link from list 
                        var link = links.First();
                        linksAlreadySearched.Add(link);
                        var doc = new HtmlWeb().Load(link);
                        links.Remove(link);

                        if (doc.DocumentNode.SelectNodes("//a[@href]") != null)
                            foreach (HtmlNode anchor in doc.DocumentNode.SelectNodes("//a[@href]"))
                            {
                                var l = anchor.Attributes["href"].Value.Contains("http") ? anchor.Attributes["href"].Value : url + anchor.Attributes["href"].Value;
                                //adding every unique anchor's ref to links list
                                if (!linksAlreadySearched.Contains(l) && !links.Contains(l) && l.Contains(url) && UrlIsValid(l))
                                {
                                    links.Add(l);
                                }
                            }
                    }

                    //measuring responses
                    foreach (var link in linksAlreadySearched)
                    {
                        var responsesTimesList = new List<double>();
                        for (int i = 0; i < 3; i++)
                        {
                            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link);

                            Stopwatch timer = new Stopwatch();
                            timer.Start();
                            try
                            {
                                HttpWebResponse res = (HttpWebResponse)request.GetResponse();
                                timer.Stop();
                                responsesTimesList.Add(timer.Elapsed.TotalSeconds);
                            }
                            catch (WebException ex)
                            {
                                //if there's a reference on the site, but it's 404
                                responsesTimesList.Add(404);
                            }
                            finally
                            {
                                timer.Stop();
                            }
                        }
                        responses.Add(new UrlResponseTime()
                        {
                            Url = link,
                            MaxResponseTime = responsesTimesList.Max(),
                            MinResponseTime = responsesTimesList.Min()
                        });
                    }
                    //replacing the 404 max and min responses with maxresponse from all responses
                    var indexesWith404 = responses.Select( (record, index) => new { Index = index});
                    var maxResponseTime = responses.Max(record => record.MaxResponseTime);
                    foreach (var item in indexesWith404)
                    {
                        responses[item.Index].MaxResponseTime = maxResponseTime;
                        responses[item.Index].MinResponseTime = maxResponseTime;
                    }
                    responses = responses.OrderByDescending(link => link.MaxResponseTime).ToList();
                    db.UrlResponseTime.AddRange(responses);
                    db.SaveChanges();
                }

                return View(responses);
            }
        }


        private bool UrlIsValid(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
                   && !url.Contains("mailto");

        }
        public ActionResult ChartBoxPlot()
        {
            //        var _context = new TestEntities();

            //        List<double> xValue = new List<double>();
            //        List<double> yValue = new List<double>();

            //        var results = (from c in _context.tblMVCCharts select c);

            //        results.ToList().ForEach(rs => xValue.Add(rs.Growth_Year));
            //        results.ToList().ForEach(rs => yValue.Add(rs.Growth_Value));

            //        new Chart(width: 600, height: 400, theme: ChartTheme.Green)

            ///// SeriesChartType.BoxPlot
            //        .AddTitle("Chart for Growth [BoxPlot Chart]")
            //                .AddSeries("Default", chartType: "BoxPlot", xValue: xValue, yValues: yValue)
            //                .Write("bmp");

            return null;
        }
    }
}
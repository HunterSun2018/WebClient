using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
// using System.Net.Http.Headers;
// using System.Xml.XPath;
// using System.Xml;
using HtmlAgilityPack;

namespace WebClient
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("QDII Fund List!");

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                await ProcessRepositories();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        struct Fund
        {
            public string code;
            public string name;
            public double value;
            public string update;
            public double ChangeInDay;
            public double ChangeInWeek;
            public double ChangeInMonth;
        }

        private static async Task ProcessRepositories()
        {
            // client.DefaultRequestHeaders.Accept.Clear();
            // client.DefaultRequestHeaders.Accept.Add(
            //     new MediaTypeWithQualityHeaderValue("text/html");
            // client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var response = await client.GetAsync("http://fund.eastmoney.com/trade/qdii.html");

            Stream stream = await response.Content.ReadAsStreamAsync();

            StreamReader read = new StreamReader(stream, Encoding.GetEncoding("gb2312"));

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(read);

            //Console.Write(pageDocument);

            var nodes = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"tblite_qdii\"]/tbody/tr");
            Fund[] funds = new Fund[nodes.Count];
            int i = 0;

            foreach (HtmlNode node in nodes)
            {
                var columes = node.SelectNodes("child::td");
                var spans = columes[2].SelectNodes("child::span");

                funds[i].code = columes[0].InnerText;  // code
                funds[i].name = columes[1].InnerText;
                funds[i].value = Convert.ToDouble(spans[0].InnerText);
                funds[i].update = spans[1].InnerText;
                funds[i].ChangeInDay = Convert.ToDouble(columes[3].InnerText.Replace('%', '0'));
                funds[i].ChangeInWeek = Convert.ToDouble(columes[4].InnerText.Replace('%', '0'));
                funds[i].ChangeInMonth = Convert.ToDouble(columes[5].InnerText.Replace('%', '0'));

                i++;
            }

            Array.Sort(funds, (x1, x2) => x1.ChangeInMonth.CompareTo(x2.ChangeInMonth));

            foreach (var fund in funds)
            {
                Console.WriteLine(
                    "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                    fund.code,
                    fund.name,
                    fund.value,
                    fund.update,
                    fund.ChangeInDay,
                    fund.ChangeInWeek,
                    fund.ChangeInMonth);
            }
        }
    }
}

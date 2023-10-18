using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp6
{
    public class EstraiNomi
    {
        public static async Task<JArray> GetJsonArrayFromUrl(string url)


        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string htmlContent = await response.Content.ReadAsStringAsync();

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    HtmlNodeCollection elements = doc.DocumentNode.SelectNodes("//div[@class='page-wrapper']//script");

                    if (elements != null && elements.Count >= 3)
                    {
                        HtmlNode element = elements.Last();

                        string scriptText = element.InnerText;

                        string stringToRemove = "var staticImpressions = staticImpressions || {};";
                        scriptText = scriptText.Replace(stringToRemove, string.Empty);

                        int startIndex = scriptText.IndexOf("{");
                        int endIndex = scriptText.LastIndexOf("}");
                        string jsonString = scriptText.Substring(startIndex, endIndex - startIndex + 1);

                        // Modifica il testo JSON in modo che sia un array di oggetti JSON
                        jsonString = "[" + jsonString + "]";

                        return JArray.Parse(jsonString);
                    }
                }

                return null;
            }
        }

        public static async Task<List<string>> GetPricesForNames(string url)
        {
            List<string> prices = new List<string>();

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                    string htmlContent = Encoding.UTF8.GetString(bytes);

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(htmlContent);

                    HtmlNodeCollection nameElements = doc.DocumentNode.SelectNodes("//*[@id=\"maincontent\"]/div[4]/div[2]/div[3]/ol/li");

                    if (nameElements != null)
                    {
                        foreach (HtmlNode nameElement in nameElements)
                        {
                            string price = nameElement.InnerText.Trim();

                            string pattern = @"(\d+\,\d{2})"; // Cerca una sequenza di numeri con una virgola per i decimali
                            Match match = Regex.Match(price, pattern);

                            if (match.Success)
                            {
                                price = match.Value;
                                prices.Add(price);
                            }
                        }
                    }
                }

                return prices;
            }
        }

        public static async Task<List<Fumetto>> ExtractFumetti(string url)
        {
            List<Fumetto> fumetti = new List<Fumetto>();

            JArray jsonArray = await EstraiNomi.GetJsonArrayFromUrl(url);
            List<string> prices = await EstraiNomi.GetPricesForNames(url);

            if (jsonArray != null && prices.Count > 0)
            {
                for (int i = 0; i < jsonArray.Count && i < prices.Count; i++)
                {
                    string name = jsonArray[i]["name"].ToString();
                    string price = prices[i];

                    // Creazione dell'oggetto Fumetto
                    Fumetto fumetto = new Fumetto
                    {
                        Nome = name,
                        Prezzo = price
                    };

                    fumetti.Add(fumetto);
                }
            }

            return fumetti;
        }

    }

    public class Fumetto
    {
        public string Nome { get; set; }
        public string Prezzo { get; set; }
    }

}

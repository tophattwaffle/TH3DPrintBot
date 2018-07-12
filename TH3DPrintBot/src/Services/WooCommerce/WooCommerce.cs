using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v2;

namespace TH3DPrintBot.src.Services.WooCommerce
{
    public class WooCommerce
    {
        private readonly DataService _dataService;

        public WooCommerce(DataService data)
        {
            _dataService = data;
        }

        public List<string> GetProductImages(Product product)
        {
            return product.images.Select(i => i.src.Substring(0, i.src.IndexOf(@"?fit", StringComparison.Ordinal))
                .Substring(i.src.IndexOf("wp.com/", StringComparison.Ordinal) + 7).Insert(0, @"https://")).ToList();
        }

        public async Task<List<Product>> SearchProducts(string search) => await GetItems(search);

        private async Task<List<Product>> GetItems(string search = null)
        {
            RestAPI rest = new RestAPI("https://th3dstudio.com/wp-json/wc/v2/",
                _dataService.RootSettings.program_settings.wooCommerceKey,
                _dataService.RootSettings.program_settings.wooCommerceSecret, false);
            WCObject wc = new WCObject(rest);

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("per_page", "100");

            if(search != null)
                dic.Add("search", search);

            int pageNumber = 1;
            dic.Add("page", pageNumber.ToString());
            List<Product> products = new List<Product>();
            bool endWhile = false;
            while (!endWhile)
            {
                var productsTemp = await wc.Product.GetAll(dic);
                if (productsTemp.Count > 0)
                {
                    products.AddRange(productsTemp);
                    pageNumber++;
                    dic["page"] = pageNumber.ToString();
                }
                else
                {
                    endWhile = true;
                }
            }
            return products;
        }
	}
}

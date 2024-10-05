import yfinance as yf

def main():
    """
    Used for Stock Tree Map
    Info Required:
     - Change in Closing Price compared to previous day (in %)
    """

    semiconductor_stock_codes = ["NVDA", "TSM", "AVGO", "AMD", "ARM",
                   "ADI", "MU", "TXN", "INTC", "MRVL",
                   "AMD", "ON", "SLAB", "TSEM", "SITM",
                   "MTSI", "CRUS", "NVEC"]
    
    internet_stock_codes = ["GOOG", "GOOGL", "META", "SPOT", "PINS",
                            "TME", "DASH", "RDDT", "MTCH",
                            "Z", "BIDU", "ZG", "YELP", "BZFD"]
    
    internet_retail_stock_codes = ["AMZN", "NEGG", "BABA", "PDD", "JD", "W", "ETSY",
                                   "LOGC", "BYON", "LOGC"]
    
    stock_industry_code = ["semiconductors", "internet-content-information", "internet-retail"]
    stock_codes_arr = [semiconductor_stock_codes, internet_stock_codes, internet_retail_stock_codes]

    output_dict = {}

    for i in range(len(stock_codes_arr)):
        output_dict[stock_industry_code[i]] = {}
        for stock_code in stock_codes_arr[i]:
            stock_code = stock_code.upper()
            stock_data = yf.Ticker(stock_code)

            # Stock information
            info = stock_data.info
            current_price = info.get('currentPrice', 0)
            previous_price = info.get('previousClose', 0)
            change = round(current_price - previous_price, 2)
            change_percent = round((change / previous_price) * 100, 2)
            output_dict[stock_industry_code[i]][stock_code] = change_percent

    print(output_dict)


if __name__ == "__main__":
    main()
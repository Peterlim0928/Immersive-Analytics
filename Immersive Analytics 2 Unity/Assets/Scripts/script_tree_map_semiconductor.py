import yfinance as yf

def main():
    """
    Used for Stock Tree Map
    Info Required:
     - Change in Closing Price compared to previous day (in %)
    """

    stock_codes = ["NVDA", "TSM", "AVGO", "AMD", "ARM",
                   "ADI", "MU", "TXN", "INTC", "MRVL",
                   "AMD", "ON", "SLAB", "TSEM", "SITM",
                   "MTSI", "CRUS", "NVEC"]
    output_dict = {}

    for stock_code in stock_codes:
        stock_code = stock_code.upper()
        stock_data = yf.Ticker(stock_code)

        # Stock information
        info = stock_data.info
        current_price = info.get('currentPrice', 0)
        previous_price = info.get('previousClose', 0)
        change = round(current_price - previous_price, 2)
        change_percent = round((change / previous_price) * 100, 2)
        output_dict[stock_code] = change_percent

    print(output_dict)


if __name__ == "__main__":
    main()
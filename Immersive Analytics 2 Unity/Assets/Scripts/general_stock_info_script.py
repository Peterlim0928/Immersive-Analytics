import yfinance as yf
import pandas as pd
import sys

def main(stock_code):

    stock_code = stock_code.upper()
    stock_data = yf.Ticker(stock_code)

    # Stock information
    info = stock_data.info
    name = info.get('shortName', 'N/A')
    country = info.get('country', 0)
    website = info.get('website', 'N/A')
    sector = info.get('sector', 'N/A')
    long_business_summary = info.get('longBusinessSummary', 'N/A')
    total_revenue = info.get('totalRevenue', 'N/A')
    financial_currency = info.get('financialCurrency', 'N/A')
    output_dict = {
        "Name": name,
        "Stock Code": stock_code,
        "Country": country,
        "Website": website,
        "Sector": sector,
        "Long Business Summary": long_business_summary,
        "Total Revenue": total_revenue,
        "Financial Currency": financial_currency
    }
    print(output_dict)


if __name__ == "__main__":
    _, stock_code = sys.argv
    main(stock_code)
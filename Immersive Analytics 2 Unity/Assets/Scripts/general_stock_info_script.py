import yfinance as yf
import pandas as pd
import random

def main():
    """
    Used for Main Info Page
    Info Required:
     - Stock Code
     - Comp Name
     - Lastest Closing Price
     - Change in Closing Price compared to previous day (in actual figure)
     - Change in Closing Price compared to previous day (in %)
    """

    # Randomly select 20 stock codes
    num_of_stocks = 0
    stock_codes_csv = pd.read_csv("./Assets/Scripts/companies.csv")
    stock_codes = stock_codes_csv["Symbol"].tolist()
    selected_stock_codes = random.sample(stock_codes, num_of_stocks)
    selected_stock_codes.append("AAPL")

    output_dict = {}

    for stock_code in selected_stock_codes:
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
        logo_url = info.get('logo_url', 'N/A')
        output_dict[stock_code] = {
            "Name": name,
            "Country": country,
            "Website": website,
            "Sector": sector,
            "Long Business Summary": long_business_summary,
            "Total Revenue": total_revenue,
            "Financial Currency": financial_currency,
            "Logo URL": logo_url
        }

    print(output_dict)


if __name__ == "__main__":
    main()
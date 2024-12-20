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
    num_of_stocks = 50
    stock_codes_csv = pd.read_csv("./Assets/Datasets/companies.csv")
    stock_codes = stock_codes_csv["Symbol"].tolist()
    selected_stock_codes = random.sample(stock_codes, num_of_stocks)

    output_dict = {}

    for stock_code in selected_stock_codes:
        stock_code = stock_code.upper()
        stock_data = yf.Ticker(stock_code)

        # Stock information
        info = stock_data.info
        name = info.get('shortName', 'N/A')
        current_price = round(info.get('currentPrice', 0), 2)
        previous_price = info.get('previousClose', 0)
        change = round(current_price - previous_price, 2)
        change_percent = round((change / previous_price) * 100, 2)
        logo_url = info.get('logo_url', 'N/A')
        output_dict[stock_code] = {
            "Name": name,
            "Previous Price": previous_price,
            "Current Price": current_price,
            "Change Figure": change,
            "Change Percentage": change_percent,
            "Logo URL": logo_url
        }

    print(output_dict)

    return(output_dict)


if __name__ == "__main__":
    main()
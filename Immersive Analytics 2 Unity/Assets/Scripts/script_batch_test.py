import sys
import yfinance as yf
import pandas as pd

def main(stock_codes: list, time_period: str, stock_interval: str = "30m"):
    stock_codes = [code.upper() for code in stock_codes]
    stock_data = yf.download(tickers=stock_codes, period=time_period, interval=stock_interval)

    pd.DataFrame(stock_data).to_csv(f"./Assets/Datasets/test.csv")
    # Save the data to a CSV file for each stock
    for code in stock_codes:
        stock_df = stock_data.xs(code, level=1, axis=1)  # Extract data for the specific stock
        stock_df.to_csv(f"./Assets/Datasets/Test/{code}-{time_period}.csv")


if __name__ == "__main__":
    # _, stock_code, time_period = sys.argv
    stock_code = ["AAPL", "MSFT", "GOOGL", "AMZN", "TSLA"]
    time_period = "1d"
    main(stock_code, time_period)
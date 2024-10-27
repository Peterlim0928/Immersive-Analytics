import sys
import yfinance as yf
import pandas as pd

def main(stock_code: str, time_period: str, time_interval: str = "30m"):
    """
    Function to get the stock data from Yahoo Finance API
    """
    # convert the stock code to uppercase
    stock_code = stock_code.upper()

    # get the stock data
    stock_data = yf.Ticker(stock_code)

    # get the historical data based on the time period and interval
    hist = stock_data.history(period = time_period, interval = time_interval)

    # save the data to a csv file
    pd.DataFrame(hist).to_csv(f"./Assets/Datasets/{stock_code}-{time_period}.csv")


if __name__ == "__main__":
    _, stock_code, time_period, time_inveral = sys.argv
    main(stock_code, time_period, time_inveral)


